//
//File: NeuralNet.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:20 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralNet.h"

using namespace Neural;

const NeuralNet::WeightsBeforeLayers& NeuralNet::getWeights() const {
#ifdef ENABLE_NET_LEARNING
  if (!weightsBeforeLayers.empty()) {
    return weightsBeforeLayers;
  }
#endif
  return sDefaultWeightsBeforeLayers;
}

#ifdef ENABLE_NET_LEARNING
NeuralNet::NeuralNet(std::string fileName) {
  //weightsBeforeLayers; from file
}

void NeuralNet::setWeights(const WeightsBeforeLayers& weights) {

}

#endif

///default neural net
std::vector<NeuralNet::Weights> NeuralNet::sDefaultWeightsBeforeLayers = {

};