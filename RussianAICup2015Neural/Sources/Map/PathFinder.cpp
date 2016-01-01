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
#include <math.h>

void PathFinder::findPath(const model::Car& car, const model::World& world, const ConnectionMap& map, const size_t maxDepth) {
  const PointIndex beginPointIndex = pointIndexByCar(car, map);
  SIAAssert(map.invalidPointIndex() != beginPointIndex);

  int wayPointIndex = car.getNextWaypointIndex();
  PointIndex pointIndex = beginPointIndex;

  while (path.size() < maxDepth) {
    fillPointsData(pointIndex, map);

    SIA::Position endPos = positionByWayPointIndex(wayPointIndex, world);
    PointIndex toIndex = minPointIndexInPos(endPos, map);

    const auto& newPath = findPath(pointIndex, toIndex, map);
    path.insert(path.end(), newPath.begin(), newPath.end());

    wayPointIndex++;
    pointIndex = toIndex;
  }
}

PointIndex PathFinder::pointIndexByCar(const model::Car& car, const ConnectionMap& map) const {
  static const size_t dirsCount = 4;

  const SIA::Vector carPos = SIA::Vector(car.getX(), car.getY());
  const SIA::Position tilePos = tilePosition(carPos.x, carPos.y);

  const SIA::Position dirs[dirsCount] = {
    SIA::Position(1, 0),
    SIA::Position(-1, 0),
    SIA::Position(0, 1),
    SIA::Position(0, -1)
  };

  PointIndex minIndex = map.invalidPointIndex();
  double minDistance = DBL_MAX;

  for (size_t i = 0; i < dirsCount; i++) {
    int dx = dirs[i].x;
    int dy = dirs[i].y;

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
  static const size_t dirsCount = 4;
  const SIA::Position dirs[dirsCount] = {
    SIA::Position( 1,  0),
    SIA::Position(-1,  0),
    SIA::Position( 0,  1),
    SIA::Position( 0, -1)
  };

  PointIndex minIndex = map.invalidPointIndex();
  double weight = DBL_MAX;
  for (int i = 0; i < dirsCount; i++) {
    PointIndex index = map.getPointIndexByTileAndDir(pos.x, pos.y, dirs[i].x, dirs[i].y);
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

std::vector<PathFinder::PathPoint> PathFinder::findPath(PointIndex fromIndex, PointIndex toIndex, const ConnectionMap& map) const {
  std::vector<SIA::Vector> reversedPath;

  PointIndex pointIndex = toIndex;

  while (pointIndex != fromIndex) {
    const auto& pointData = map.getConnectionPointByIndex(pointIndex);

    PointIndex savePointIndex = pointIndex;
    double currentWeight = pointWeight[pointIndex];
    for (const auto& join : pointData.joins) {
      if (abs(currentWeight - pointWeight[join.index] - calculatePointWeight(join)) < 1.0e-9) {
        pointIndex = join.index;

        const auto& pointData = map.getConnectionPointByIndex(pointIndex);
        reversedPath.push_back(pointData.pos);
        break;
      }
    }

    SIAAssert(savePointIndex != pointIndex);
  }

  std::reverse(reversedPath.begin(), reversedPath.end());
  return reversedPath;
}

void PathFinder::fillPointsData(PointIndex beginPointIndex, const ConnectionMap& map) {
  const size_t pointCount = map.getPointCount();
  pointWeight.clear();
  pointVisited.clear();
  pointWeight.resize(pointCount, DBL_MAX);
  pointVisited.resize(pointCount, false);

  pointWeight[beginPointIndex] = 0;

  while (true) {
    PointIndex minPointIndex = map.invalidPointIndex();
    double minWeight = DBL_MAX;
    for (PointIndex i = 0; i < pointCount; i++) {
      if (pointWeight[i] < minWeight && !pointVisited[i]) {
        minWeight = pointWeight[i];
        minPointIndex = i;
      }
    }

    if (minPointIndex == map.invalidPointIndex()) {
      break;
    }

    pointVisited[minPointIndex] = true;

    const auto& pointData = map.getConnectionPointByIndex(minPointIndex);
    for (const auto& join : pointData.joins) {
      if (!pointVisited[join.index]) {
        pointWeight[join.index] = min(pointWeight[join.index], minWeight + calculatePointWeight(join));
      }
    }    

  }

}

double PathFinder::calculatePointWeight(const ConnectionJoin& join) const {
  return join.length;
}

#ifdef ENABLE_VISUALIZATOR
void PathFinder::visualizationPath(const Visualizator& visualizator, int32_t color) const {
  for (size_t i = 1; i < path.size(); i++) {
    visualizator.line(path[i - 1].x, path[i - 1].y, path[i].x, path[i].y, color);
  }
}
#endif