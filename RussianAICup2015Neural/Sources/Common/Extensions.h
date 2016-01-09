//
//File: Extensions.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 10:16 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _EXTENSIONS_H__
#define _EXTENSIONS_H__

#include "SIAPoint2D.h"

namespace Extensions
{
  SIA::Vector vectorByAnchor(int x, int y, double aX, double aY);
  SIA::Position tilePosition(double x, double y);
  SIA::Position tilePosition(SIA::Vector pos);

  double constPI();

  double angleDiff(double angle1, double angle2);

  template <typename T> int sign(T val) {
    return (T(0) < val) - (val < T(0));
  }

#define MAX(a,b) (((a) > (b)) ? (a) : (b))
#define MIN(a,b) (((a) < (b)) ? (a) : (b))

#define SIA_PI constPI()

};

#endif