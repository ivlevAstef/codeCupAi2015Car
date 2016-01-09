//
//File: NeuralIn.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:41 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralIn.h"
#include "Common/Constants.h"
#include "Common/SIALogger.h"

using namespace Neural;

const size_t NeuralIn::sInputValuesCount = 
6 * 2 + 3 * 3 * 2 + ///paths
4 * (4/*speed, angle*/ + 2/*wheelturn, engine power*/ + 2/*hp, type*/ + 2/*times*/) + ///all cars
3 * (2 + 1) +///car position + car is teammate
19 +///passages around
3 * 2 +///oil positions
3 * (2 + 1) + ///bonus position + bonus type
2 * (2 + 2) + ///tire position + tire speed
6 * (2 + 2) ///washer position + washer speed
;

NeuralIn::NeuralIn(const model::World& world, const std::vector<Map::PathFinder>& paths) {
  calculateInputValue(world, paths);
}

const std::vector<double>& NeuralIn::values() const {
  return inputValues;
}

void NeuralIn::calculateInputValue(const model::World& world, const std::vector<Map::PathFinder>& paths) {
  static const double projectileCooldown = double(Constants::instance().game.getThrowProjectileCooldownTicks());
  static const double oiledOnWheel = 60;

  SIAAssert(6 == Constants::pathSelfDepth);
  SIAAssert(3 == Constants::pathOtherCarDepth);

  SIAAssert(4 == paths.size());

  const SIA::Vector translate(paths[0].getCar().getX(), paths[0].getCar().getY());

  inputValues.resize(sInputValuesCount, 0);

  auto* pValuesIter = inputValues.data();
  for (size_t i = 0; i < paths.size(); i++) {
    const auto& path = paths[i];

    SIAAssert(path.getPath().size() == (0 == i ? Constants::pathSelfDepth : Constants::pathOtherCarDepth));
    for (const auto& point : path.getPath()) {
      *(pValuesIter++) = point.x - translate.x;
      *(pValuesIter++) = point.y - translate.y;
    }
   
    const model::Car& car = path.getCar();

    *(pValuesIter++) = car.getSpeedX();
    *(pValuesIter++) = car.getSpeedY();

    *(pValuesIter++) = cos(car.getAngle());
    *(pValuesIter++) = sin(car.getAngle());

    *(pValuesIter++) = car.getWheelTurn();
    *(pValuesIter++) = car.getEnginePower();

    *(pValuesIter++) = car.getDurability();
    *(pValuesIter++) = double(car.getType()) / double(model::_CAR_TYPE_COUNT_);

    *(pValuesIter++) = (0 == car.getProjectileCount()) ? 0.0 : 
      ((projectileCooldown - car.getRemainingProjectileCooldownTicks()) / projectileCooldown);

    *(pValuesIter++) = car.getRemainingOiledTicks() / oiledOnWheel;

    if (0 != i) {
      *(pValuesIter++) = car.getX() - translate.x;
      *(pValuesIter++) = car.getY() - translate.y;
      *(pValuesIter++) = car.isTeammate() ? 0.0 : 1.0;
    }

    ///TODO: passages around
    ///TODO: bonuses
    ///TODO: tires
    ///TODO: washers
  }
}
