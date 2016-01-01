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

  PointIndex pointIndexByCar(const model::Car& car, const ConnectionMap& map) const;
  SIA::Position positionByWayPointIndex(int wayPointIndex, const model::World& world) const;

  PointIndex minPointIndexInPos(SIA::Position pos, const ConnectionMap& map) const;

  std::vector<PathPoint> findPath(PointIndex fromIndex, PointIndex toIndex, const ConnectionMap& map) const;

  double calculatePointWeight(const ConnectionJoin& join) const;

private:
  std::vector<double> pointWeight;
  std::vector<bool> pointVisited;

  std::vector<PathPoint> path;
};

#endif