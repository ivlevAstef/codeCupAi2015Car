//
//File: Extensions.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 10:16 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//


#include "Extensions.h"
#include "Constants.h"

namespace Extensions
{
  double constPI() {
    static const double PI = std::atan(1) * 4;
    return PI;
  }

  SIA::Vector vectorByAnchor(int x, int y, double ax, double ay) {
    static const double tileSize = Constants::instance().game.getTrackTileSize();
    return SIA::Vector((x + ax)* tileSize, (y + ay)* tileSize);
  }

  SIA::Position tilePosition(double x, double y) {
    static const double tileSize = Constants::instance().game.getTrackTileSize();
    return SIA::Position(int(x / tileSize), int(y / tileSize));
  }

  SIA::Position tilePosition(SIA::Vector pos) {
    static const double tileSize = Constants::instance().game.getTrackTileSize();
    return SIA::Position(int(pos.x / tileSize), int(pos.y / tileSize));
  }

  double angleDiff(double angle1, double angle2) {
    double angle = angle1 - angle2;

    while (angle > SIA_PI) {
      angle -= 2.0 * SIA_PI;
    }

    while (angle < -SIA_PI) {
      angle += 2.0 * SIA_PI;
    }

    return angle;
  }

}