//
//File: NeuralOut.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:46 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralOut.h"

using namespace Neural;

NeuralOut::NeuralOut(const std::vector<double>& output) {
  calculateMove(output);
}

void NeuralOut::calculateMove(const std::vector<double>& output) {

}

model::Move NeuralOut::getMove() const {
  return move;
}

void NeuralOut::fillMove(model::Move& moveForFill) const {
  moveForFill.setEnginePower(move.getEnginePower());
  moveForFill.setWheelTurn(move.getWheelTurn());
  moveForFill.setBrake(move.isBrake());

  moveForFill.setUseNitro(move.isUseNitro());
  moveForFill.setThrowProjectile(move.isThrowProjectile());
  moveForFill.setSpillOil(move.isSpillOil());
}