//
//File: Constants.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 20:58 2/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "Constants.h"
#include "Extensions.h"

const size_t Constants::dirsCount = 4;
const SIA::Position Constants::dirs[dirsCount] = {
  SIA::Position(1, 0),
  SIA::Position(-1, 0),
  SIA::Position(0, 1),
  SIA::Position(0, -1)
};

double Constants::bonusPriorityForCar(const model::BonusType& bonusType, const model::Car& car) {
  switch (bonusType) {
    case model::REPAIR_KIT:
      return 1.0 - car.getDurability() * car.getDurability();

    case model::PURE_SCORE:
      return 1.0;

    case model::NITRO_BOOST:
      return MAX(0, 0.8 - 0.3 * car.getNitroChargeCount());

    case model::AMMO_CRATE:
      return MAX(0, 0.7 - 0.2 * car.getProjectileCount());

    case model::OIL_CANISTER:
      return MAX(0, 0.4 - 0.2 * car.getOilCanisterCount());
  }

  return 0;
}