//
//File: PathFinder.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 13:49 1/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _PATH_FINDER_H__
#define _PATH_FINDER_H__

#include "ConnectionMap.h"
#include <unordered_set>

class PathFinder {
public:
  typedef SIA::Vector PathPoint;
public:
  void findPath(const model::Car& car, const model::World& world, const ConnectionMap& map, const size_t maxDepth);

  inline const std::vector<PathPoint>& getPath() const { return path; }

#ifdef ENABLE_VISUALIZATOR
  void visualizationPath(const Visualizator& visualizator, int32_t color) const;
#endif

private:
  void fillPointsData(PointIndex beginIndex, const ConnectionMap& map);

  PointIndex pointIndexByCar(const model::Car& car, const ConnectionMap& map, double moveLength) const;
  SIA::Position nextPositionForCar(PointIndex pointIndex, const model::Car& car, const ConnectionMap& map) const;

  SIA::Position positionByWayPointIndex(int wayPointIndex, const model::World& world) const;

  PointIndex minPointIndexInPos(SIA::Position pos, const ConnectionMap& map) const;

  std::vector<PointIndex> findPathPointIndex(PointIndex fromIndex, PointIndex toIndex, const ConnectionMap& map) const;
  void fillPathByPointIndex(const std::vector<PointIndex>& points, const ConnectionMap& map);

  void setBackwardIndexes(PointIndex pointIndex, const SIA::Position pos, const ConnectionMap& map);

  double calculatePointWeight(const ConnectionJoin& join, const PointIndex& from, bool reverse = false) const;

private:
  static const double sBackwardWeight;

  std::vector<double> pointWeight;
  std::vector<bool> pointVisited;
  std::unordered_set<PointIndex> backwardPointIndexes;
  PointIndex backwardFromPointIndex;

  std::vector<PathPoint> path;
};

#endif