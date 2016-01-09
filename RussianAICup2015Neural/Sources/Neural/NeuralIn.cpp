//
//File: NeuralIn.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:41 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralIn.h"

using namespace Neural;

NeuralIn::NeuralIn(const model::Car& car, const model::World& world, const Map::PathFinder& path) {
  calculateInputValue(car, world, path);
}

const std::vector<double>& NeuralIn::input() const {
  return inputValues;
}

void NeuralIn::calculateInputValue(const model::Car& car, const model::World& world, const Map::PathFinder& path) {

}
