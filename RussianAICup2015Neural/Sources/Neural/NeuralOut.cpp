//
//File: NeuralOut.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:46 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralOut.h"
#include "Common/SIALogger.h"

using namespace Neural;

const size_t NeuralOut::sOutputValuesCount = 6;

NeuralOut::NeuralOut(const std::vector<double>& neurons) {
  calculateMove(neurons);
}

void NeuralOut::calculateMove(const std::vector<double>& neurons) {
  SIAAssert(sOutputValuesCount == neurons.size());

  move.setEnginePower(neurons[0]);
  move.setWheelTurn(neurons[1]);
  move.setBrake(neurons[2] > 0);

  move.setUseNitro(neurons[3] > 0);
  move.setThrowProjectile(neurons[4] > 0);
  move.setSpillOil(neurons[5] > 0);
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