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

namespace Map
{

  class PathFinder {
  public:
    typedef SIA::Vector PathPoint;
  public:
    void findPath(const model::Car& car, const model::World& world, const ConnectionMap& map, const size_t maxDepth);

    inline const std::vector<PathPoint>& getPath() const {
      return path;
    }

#ifdef ENABLE_VISUALIZATOR
    void visualizationPath(const Visualizator& visualizator, int32_t color) const;
    void visualizationPointWeight(const Visualizator& visualizator, int32_t color, const ConnectionMap& map) const;
#endif

  private:
    void fillPointsData(PointIndex lastPointIndex, PointIndex beginIndex, const ConnectionMap& map);

    PointIndex pointIndexByCar(const model::Car& car, const ConnectionMap& map, double moveLength, PointIndex ignoredIndex) const;

    SIA::Position positionByWayPointIndex(int wayPointIndex, const model::World& world) const;

    PointIndex minPointIndexInPos(SIA::Position pos, const ConnectionMap& map) const;

    std::vector<PointIndex> findPathPointIndex(PointIndex fromIndex, PointIndex toIndex, const ConnectionMap& map) const;
    void fillPathByPointIndex(const std::vector<PointIndex>& points, const ConnectionMap& map);

    double calculatePointWeight(const ConnectionJoin& join, const ConnectionJoin* lastJoin, const PointIndex& commonPointIndex) const;

  private:
    static const double sWeightMult;
    static const double sAngleWeightMult;

    std::vector<double> pointWeight;
    std::vector<PointIndex> minLastPointIndexes;

    std::vector<PathPoint> path;
  };

};

#endif