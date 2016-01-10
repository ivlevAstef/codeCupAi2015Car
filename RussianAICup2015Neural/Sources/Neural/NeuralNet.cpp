//
//File: NeuralNet.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:20 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "NeuralNet.h"
#include "Common/SIALogger.h"

#ifdef ENABLE_NET_LEARNING
#include "NeuralIn.h"
#include "NeuralOut.h"
#include <iostream>
#include <iomanip>
#include <fstream>
#endif

using namespace Neural;

#ifdef ENABLE_NET_LEARNING
static const char* filePath = "D:/Projects/RussianAICup/2015/Project/RussianAICup2015Neural/Sources/Neural/DefaultNeuralNet.cpp";

NeuralNet::WeightsBeforeLayers& NeuralNet::getWeights() const {
  return sWeightsBeforeLayers;
}

void NeuralNet::rand() {
  static std::vector<size_t> weightsCountData = {700 * NeuralIn::sInputValuesCount, 700 * 100, 100 * 50, 50 * NeuralOut::sOutputValuesCount};

  srand(unsigned int(time(0)));

  sWeightsBeforeLayers.clear();
  sWeightsBeforeLayers.resize(weightsCountData.size());

  for (size_t layer = 0; layer < weightsCountData.size(); ++layer) {
    auto& weights = sWeightsBeforeLayers[layer];
    const size_t& weightsCount = weightsCountData[layer];

    weights.resize(weightsCount);

    for (size_t i = 0; i < weightsCount; i++) {
      weights[i] = double(::rand()) / double(RAND_MAX);
    }
  }

}

void NeuralNet::load() {
  std::ifstream file(filePath);

  SIAAssert(file.is_open());

  file.ignore(1024, '{');///static std::vector<NeuralNet::Weights> GetWeightsBeforeLayers() {

  sWeightsBeforeLayers.clear();
  while (!file.eof()) {
    do {
      file.ignore(1024, '{');///static const double dataI[] = {
    } while (!file.eof() && file.peek() == '\n');

    Weights weights;
    while (!file.eof() && file.peek() != '}') {
      double value = 0;
      file.ignore();
      file >> value;
      weights.push_back(value);
    }

    if (!weights.empty()) {
      sWeightsBeforeLayers.push_back(weights);
    }
  }
}

void NeuralNet::save() {
  std::ofstream file(filePath);

  SIAAssert(file.is_open());

  file << "#include \"NeuralNet.h\"" << std::endl;
  file << "using namespace Neural;" << std::endl;
  file << std::endl;
  file << "static std::vector<NeuralNet::Weights> GetWeightsBeforeLayers() {" << std::endl;
  for (size_t weightsIter = 0; weightsIter < sWeightsBeforeLayers.size(); ++weightsIter) {
    size_t index = weightsIter + 1;
    file << "  static const double data" << index << "[] = {";

    const auto* pWeights = sWeightsBeforeLayers[weightsIter].data();
    const size_t weightsSize = sWeightsBeforeLayers[weightsIter].size();

    char separator = ' ';
    for (size_t i = 0; i < weightsSize; ++i) {
      file << separator << std::setprecision(5) << pWeights[i];
      separator = ',';
    }

    file << "};" << std::endl;
  }

  file << "  static std::vector<NeuralNet::Weights> result;" << std::endl;
  file << "  if (result.empty()) {" << std::endl;
  for (size_t weightsIter = 0; weightsIter < sWeightsBeforeLayers.size(); ++weightsIter) {
    size_t index = weightsIter + 1;
    file << "    result.push_back(NeuralNet::Weights(data" << index << ", data" << index << " + sizeof(data" << index << ")/sizeof(double)));" << std::endl;
  }

  file << "  }" << std::endl;
  file << "  return result;" << std::endl;
  file << "}" << std::endl;
  file << std::endl;
  file << "std::vector<NeuralNet::Weights> NeuralNet::sWeightsBeforeLayers = GetWeightsBeforeLayers();" << std::endl;
}

#else
const NeuralNet::WeightsBeforeLayers& NeuralNet::getWeights() const {
  return sWeightsBeforeLayers;
}
#endif