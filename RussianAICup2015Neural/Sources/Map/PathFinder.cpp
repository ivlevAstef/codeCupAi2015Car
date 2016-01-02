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

const double PathFinder::sBackwardWeight = 4;

void PathFinder::findPath(const model::Car& car, const model::World& world, const ConnectionMap& map, const size_t maxDepth) {
  const PointIndex beginPointIndex = pointIndexByCar(car, map, Constants::instance().game.getTrackTileSize());
  setBackwardIndexes(beginPointIndex, nextPositionForCar(beginPointIndex, car, map), map);

  std::vector<PointIndex> pointIndexPath;

  int wayPointIndex = car.getNextWaypointIndex();
  PointIndex pointIndex = beginPointIndex;
  std::vector<PointIndex> visited;

  while (pointIndexPath.size() < maxDepth) {
    fillPointsData(pointIndex, visited, map);

    const SIA::Position endPos = positionByWayPointIndex(wayPointIndex, world);
    const PointIndex toIndex = minPointIndexInPos(endPos, map);

    const auto& newPath = findPathPointIndex(pointIndex, toIndex, map);
    pointIndexPath.insert(pointIndexPath.end(), newPath.begin(), newPath.end());

    wayPointIndex++;
    pointIndex = toIndex;

    setBackwardIndexes(pointIndex, endPos, map);
  }

  fillPathByPointIndex(pointIndexPath, map);
}

void PathFinder::setBackwardIndexes(PointIndex pointIndex, const SIA::Position pos, const ConnectionMap& map) {
  backwardPointIndexes.clear();

  for (const ConnectionJoin& join : map.getConnectionPointByIndex(pointIndex).joins) {
    bool found = false;
    for (size_t i = 0; i < Constants::dirsCount; i++) {
      PointIndex index = map.getPointIndexByTileAndDir(pos.x, pos.y, Constants::dirs[i].x, Constants::dirs[i].y);
      found |= (join.index == index);
    }

    if (!found) {
      backwardPointIndexes.insert(join.index);
    }
  }
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

  PointIndex pointIndex = toIndex;

  while (pointIndex != fromIndex) {
    const auto& pointData = map.getConnectionPointByIndex(pointIndex);

    PointIndex savePointIndex = pointIndex;
    double currentWeight = pointWeight[pointIndex];
    for (const auto& join : pointData.joins) {
      if (abs(currentWeight - pointWeight[join.index] - calculatePointWeight(join)) < 1.0e-9) {
        pointIndex = join.index;
        reversedPath.push_back(pointIndex);
        break;
      }
    }

    SIAAssert(savePointIndex != pointIndex);
  }

  std::reverse(reversedPath.begin(), reversedPath.end());
  return reversedPath;
}

void PathFinder::fillPointsData(PointIndex beginPointIndex, const std::vector<PointIndex>& visited, const ConnectionMap& map) {
  const size_t pointCount = map.getPointCount();
  pointWeight.clear();
  pointVisited.clear();
  pointWeight.resize(pointCount, DBL_MAX);
  pointVisited.resize(pointCount, false);

  pointWeight[beginPointIndex] = 0;
  for (const PointIndex& i : visited) {
    pointVisited[i] = true;
  }

  std::unordered_set<PointIndex> validIndex;
  validIndex.reserve(pointCount);

  PointIndex minPointIndex = beginPointIndex;
  double minWeight = 0;
  while (minPointIndex != map.invalidPointIndex()) {
    pointVisited[minPointIndex] = true;
    validIndex.erase(minPointIndex);

    const auto& pointData = map.getConnectionPointByIndex(minPointIndex);
    for (const auto& join : pointData.joins) {
      if (!pointVisited[join.index]) {
        pointWeight[join.index] = min(pointWeight[join.index], minWeight + calculatePointWeight(join));
        validIndex.insert(join.index);
      }
    }


    minPointIndex = map.invalidPointIndex();
    minWeight = DBL_MAX;
    for (const PointIndex& i : validIndex) {
      if (pointWeight[i] < minWeight && !pointVisited[i]) {
        minWeight = pointWeight[i];
        minPointIndex = i;
      }
    }
  }
}

double PathFinder::calculatePointWeight(const ConnectionJoin& join) const {
  const double backwardWeight = (0 != backwardPointIndexes.count(join.index)) ? sBackwardWeight : 0;
  return join.length + backwardWeight;
}

#ifdef ENABLE_VISUALIZATOR
void PathFinder::visualizationPath(const Visualizator& visualizator, int32_t color) const {
  for (size_t i = 1; i < path.size(); i++) {
    visualizator.line(path[i - 1].x, path[i - 1].y, path[i].x, path[i].y, color);
  }
}
#endif