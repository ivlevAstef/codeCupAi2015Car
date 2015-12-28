//
//File: Constants.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 10:18 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "model/Game.h"
#include "SIASingleton.h"

class Constants : public SIA::Singleton <Constants> {
public:
  Constants(const model::Game& game) : SIA::Singleton <Constants>(true), game(game) {
  }

  const model::Game& game;
};