//
//File: Extensions.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 10:16 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//


#include "Extensions.h"
#include "Constants.h"

SIA::Vector vectorByAnchor(int x, int y, double ax, double ay) {
  const double tileSize = Constants::instance().game.getTrackTileSize();
  return SIA::Vector((x + ax)* tileSize, (y + ay)* tileSize);
}