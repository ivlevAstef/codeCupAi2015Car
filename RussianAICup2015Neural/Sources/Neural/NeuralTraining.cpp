//
//File: NeuralTraining.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 15:05 10/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralTraining.h"

using namespace Neural;

NeuralTraining NeuralTraining::singleton;

NeuralTraining::NeuralTraining() {
  net.load();
  if (net.getWeights().empty()) {
    net.rand();
    net.save();
  }
}

void NeuralTraining::update(const NeuralIn& input, const NeuralOut& output) {

}