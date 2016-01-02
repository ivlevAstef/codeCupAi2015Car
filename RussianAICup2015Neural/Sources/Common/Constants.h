//
//File: Constants.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 10:18 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _CONSTANTS_H__
#define _CONSTANTS_H__

#include "model/Game.h"
#include "SIASingleton.h"
#include "SIAPoint2D.h"

class Constants : public SIA::Singleton <Constants> {
public:
  Constants(const model::Game& game) : SIA::Singleton <Constants>(true), game(game) {
  }

  const model::Game& game;

  static const size_t dirsCount;
  static const SIA::Position dirs[];
};

#endif