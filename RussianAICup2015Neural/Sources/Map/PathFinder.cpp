//
//File: PathFinder.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 13:49 1/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "PathFinder.h"
#include "Common/SIALogger.h"
#include "Common/Extensions.h"
#include "Common/Constants.h"
#include <math.h>
#include <unordered_set>

const double PathFinder::sWeightMult = 0.75;
const double PathFinder::sBackwardWeight = 16*16*2;

void PathFinder::findPath(const model::Car& car, const model::World& world, const ConnectionMap& map, const size_t maxDepth) {
  const PointIndex beginPointIndex = pointIndexByCar(car, map, Constants::instance().game.getTrackTileSize());
  setBackwardIndexes(beginPointIndex, nextPositionForCar(beginPointIndex, car, map), map);

  std::vector<PointIndex> pointIndexPath;

  int wayPointIndex = car.getNextWaypointIndex();
  PointIndex pointIndex = beginPointIndex;

  while (pointIndexPath.size() < maxDepth) {
    fillPointsData(pointIndex, map);

    const SIA::Position endPos = positionByWayPointIndex(wayPointIndex, world);
    const PointIndex toIndex = minPointIndexInPos(endPos, map);

    const auto& newPath = findPathPointIndex(pointIndex, toIndex, map);
    pointIndexPath.insert(pointIndexPath.end(), newPath.begin(), newPath.end());

    wayPointIndex++;
    pointIndex = toIndex;

    setBackwardIndexes(pointIndex, endPos, map);
  }

  fillPathByPointIndex(pointIndexPath, map);

  clearJoinsUserInfo();
}

void PathFinder::setBackwardIndexes(PointIndex pointIndex, const SIA::Position pos, const ConnectionMap& map) {
  clearJoinsUserInfo();

  for (const ConnectionJoinData& join : map.getConnectionPointByIndex(pointIndex).joins) {
    bool found = false;
    for (size_t i = 0; i < Constants::dirsCount; i++) {
      PointIndex index = map.getPointIndexByTileAndDir(pos.x, pos.y, Constants::dirs[i].x, Constants::dirs[i].y);
      found |= (join.index == index);
    }

    if (!found && 0 == joinsUserInfo.count(join.data)) {
      joinsUserInfo[join.data] = sBackwardWeight;
      join.data->userInfo = &joinsUserInfo[join.data];
    }
  }
}

void PathFinder::clearJoinsUserInfo() {
  for (const auto& join : joinsUserInfo) {
    join.first->userInfo = NULL;
  }
  joinsUserInfo.clear();
}

SIA::Position PathFinder::nextPositionForCar(PointIndex pointIndex, const model::Car& car, const ConnectionMap& map) const {
  const auto& tilePoss = map.getTiles(pointIndex);

  SIA::Position pos = tilePosition(car.getX(), car.getY());

  return (tilePoss[0] != pos) ? tilePoss[0] : tilePoss[1];
}

void PathFinder::fillPathByPointIndex(const std::vector<PointIndex>& points, const ConnectionMap& map) {
  path.clear();
  for (const PointIndex& pointIndex : points) {
    const auto& pointData = map.getConnectionPointByIndex(pointIndex);
    path.push_back(pointData.pos);
  }
}

PointIndex PathFinder::pointIndexByCar(const model::Car& car, const ConnectionMap& map, double moveLength) const {
  const SIA::Vector carDir(cos(car.getAngle()), sin(car.getAngle()));
  const SIA::Vector carPos = SIA::Vector(car.getX() + carDir.x * moveLength, car.getY() + carDir.y * moveLength);
  const SIA::Position tilePos = tilePosition(car.getX(), car.getY());

  PointIndex minIndex = map.invalidPointIndex();
  double minDistance = DBL_MAX;

  for (size_t i = 0; i < Constants::dirsCount; i++) {
    int dx = Constants::dirs[i].x;
    int dy = Constants::dirs[i].y;

    PointIndex index = map.getPointIndexByTileAndDir(tilePos.x, tilePos.y, dx, dy);

    double distance = (carPos - vectorByAnchor(tilePos.x, tilePos.y, (double)(dx + 1) * 0.5, (double)(dy + 1) * 0.5)).length2();
    if (map.validPointIndex(index) && distance < minDistance) {
      minIndex = index;
      minDistance = distance;
    }
  }

  return minIndex;
}

PointIndex PathFinder::minPointIndexInPos(SIA::Position pos, const ConnectionMap& map) const {
  PointIndex minIndex = map.invalidPointIndex();
  double weight = DBL_MAX;
  for (size_t i = 0; i < Constants::dirsCount; i++) {
    PointIndex index = map.getPointIndexByTileAndDir(pos.x, pos.y, Constants::dirs[i].x, Constants::dirs[i].y);
    if (map.validPointIndex(index) && pointWeight[index] < weight) {
      minIndex = index;
      weight = pointWeight[index];
    }
  }

  return minIndex;
}

SIA::Position PathFinder::positionByWayPointIndex(int wayPointIndex, const model::World& world) const {
  SIAAssert(0 <= wayPointIndex);

  const auto& wayPoints = world.getWaypoints();

  const auto& wayPoint = wayPoints[wayPointIndex % wayPoints.size()];
  SIAAssert(2 == wayPoint.size());

  return SIA::Position(wayPoint[0], wayPoint[1]);
}

std::vector<PointIndex> PathFinder::findPathPointIndex(PointIndex fromIndex, PointIndex toIndex, const ConnectionMap& map) const {
  std::vector<PointIndex> reversedPath;
  reversedPath.reserve(32);

  PointIndex pointIndex = toIndex;

  while (pointIndex != fromIndex) {
    pointIndex = minLastPointIndexes[pointIndex];
    SIAAssert(pointIndex != map.invalidPointIndex());
    reversedPath.push_back(pointIndex);
  }

  std::reverse(reversedPath.begin(), reversedPath.end());
  return reversedPath;
}

void PathFinder::fillPointsData(PointIndex beginPointIndex, const ConnectionMap& map) {
  const size_t pointCount = map.getPointCount();

  std::vector<char> pointVisited;
  pointVisited.resize(pointCount, false);

  pointWeight.clear();
  pointWeight.resize(pointCount, DBL_MAX);

  minLastPointIndexes.clear();
  minLastPointIndexes.resize(pointCount, map.invalidPointIndex());

  pointWeight[beginPointIndex] = 0;

  bool* pPointVisited = (bool*)pointVisited.data();
  double* pPointWeight = pointWeight.data();
  PointIndex* pPointLast = minLastPointIndexes.data();

  PointIndex minPointIndex = beginPointIndex;
  double minWeight = 0;
  while (minPointIndex != map.invalidPointIndex()) {
    pPointVisited[minPointIndex] = true;

    const auto& pointData = map.getConnectionPointByIndex(minPointIndex);
    const size_t pointDataJoinsSize = pointData.joins.size();

    for (size_t i = 0; i < pointDataJoinsSize; ++i) {
      const auto& join = pointData.joins[i];

      if (!pPointVisited[join.index]) {
        SIAAssert(NULL != join.data);

        double newPointWeight = minWeight + calculatePointWeight(*join.data);
        if (newPointWeight < pPointWeight[join.index]) {
          pPointWeight[join.index] = newPointWeight;
          pPointLast[join.index] = minPointIndex;
        }
      }
    }

    minPointIndex = map.invalidPointIndex();
    minWeight = DBL_MAX;

    for (size_t i = 0; i < pointCount; i++) {
      if (pPointWeight[i] < minWeight && !pPointVisited[i]) {
        minWeight = pPointWeight[i];
        minPointIndex = PointIndex(i);
      }
    }
  }
}

double PathFinder::calculatePointWeight(const ConnectionJoin& join) const {
  const double backwardWeight = (NULL != join.userInfo) ? *join.userInfo : 0;
  return MAX(0.1, join.length - join.weight * sWeightMult + backwardWeight);
}

#ifdef ENABLE_VISUALIZATOR
void PathFinder::visualizationPath(const Visualizator& visualizator, int32_t color) const {
  const size_t pathSize = path.size();

  for (size_t i = 1; i < pathSize; ++i) {
    visualizator.line(path[i - 1].x, path[i - 1].y, path[i].x, path[i].y, color);
  }
}
#endif