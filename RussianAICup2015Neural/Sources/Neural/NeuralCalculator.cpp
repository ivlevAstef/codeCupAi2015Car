//
//File: NeuralCalculator.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:53 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralCalculator.h"
#include "Common/SIALogger.h"
#include <vector>

using namespace Neural;

NeuralCalculator::NeuralCalculator(const NeuralNet& net) : net(net) {

} 

const NeuralOut NeuralCalculator::calculate(const NeuralIn& input) const {
  const auto& weights = net.getWeights();
  const size_t layerCount = weights.size();
  const auto& inputValues = input.values();

  std::vector<std::vector<double>> neurons;

  neurons.resize(layerCount + 1/*for input*/);

  neurons[0] = inputValues;

  for (size_t layerIndex = 1; layerIndex < layerCount + 1; ++layerIndex) {
    const auto* pLayerWeights = weights[layerIndex - 1].data();
    const size_t lastNeuronsCount = neurons[layerIndex - 1].size();
    const size_t currentNeuronsCount = weights[layerIndex - 1].size() / lastNeuronsCount;

    neurons[layerIndex].resize(currentNeuronsCount);
    const auto& lastLayerNeurons = neurons[layerIndex - 1];
    const auto& pLayerNeurons = neurons[layerIndex].data();

    for (size_t neuronIndex = 0; neuronIndex < currentNeuronsCount; ++neuronIndex) {
      const double* beginWeight = pLayerWeights + neuronIndex * lastNeuronsCount;
      const double* lastWeight = beginWeight + lastNeuronsCount;
      pLayerNeurons[neuronIndex] = calculateNeuronExcitation(lastLayerNeurons, beginWeight, lastWeight);
    }
  }

  return NeuralOut(neurons[layerCount]);
}

double NeuralCalculator::calculateNeuronExcitation(const std::vector<double>& lastNeurons, const double* beginWeight, const double* lastWeight) const {
  SIAAssert(lastNeurons.size() == (lastWeight - beginWeight));

  const auto* pNeuronIter = lastNeurons.data();
  const double* pWeightIter = beginWeight;

  double result = 0;

  while (pWeightIter != lastWeight) {
    result += (*pNeuronIter) * (*pWeightIter);

    ++pNeuronIter;
    ++pWeightIter;
  }

  return result;
}