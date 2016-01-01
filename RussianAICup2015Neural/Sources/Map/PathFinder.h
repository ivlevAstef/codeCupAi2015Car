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
  bool findPath(const model::Car& car, const model::World& world, const ConnectionMap& map);

  inline const std::vector<PathPoint>& getPath() const { return path; }

#ifdef ENABLE_VISUALIZATOR
  void visualizationPath(const Visualizator& visualizator, int32_t color) const;
#endif

private:
  std::vector<PathPoint> path;
};

#endif