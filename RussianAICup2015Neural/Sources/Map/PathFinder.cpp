//
//File: PathFinder.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 13:49 1/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "PathFinder.h"
#include "Common/SIALogger.h"

bool PathFinder::findPath(const model::Car& car, const model::World& world, const ConnectionMap& map) {
  return true;
}

#ifdef ENABLE_VISUALIZATOR
void PathFinder::visualizationPath(const Visualizator& visualizator, int32_t color) const {
  for (size_t i = 1; i < path.size(); i++) {
    visualizator.line(path[i - 1].x, path[i - 1].y, path[i].x, path[i].y, color);
  }
}
#endif