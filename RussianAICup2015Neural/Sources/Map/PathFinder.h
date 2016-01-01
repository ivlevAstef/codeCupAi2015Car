//
//File: PathFinder.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 13:49 1/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "ConnectionMap.h"

class PathFinder {
public:
  typedef SIA::Vector PathPoint;
public:
  bool findPath(const model::Car& car, const model::World& world, const ConnectionMap& map);

  inline const std::vector<PathPoint>& getPath() const { return path; }

  void visualizationPath(const Visualizator& visualizator, int32_t color) const;

private:
  std::vector<PathPoint> path;
};