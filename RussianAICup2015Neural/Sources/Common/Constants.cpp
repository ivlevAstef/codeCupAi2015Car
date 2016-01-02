//
//File: Constants.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 20:58 2/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "Constants.h"

const size_t Constants::dirsCount = 4;
const SIA::Position Constants::dirs[dirsCount] = {
  SIA::Position(1, 0),
  SIA::Position(-1, 0),
  SIA::Position(0, 1),
  SIA::Position(0, -1)
};