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
  static std::vector<size_t> weightsCountData = {1400 * NeuralIn::sInputValuesCount, 1400 * 700, 700 * 300, 300 * NeuralOut::sOutputValuesCount};

  srand(time(0));

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

  char buffer[1024] = {0};

  file.getline(buffer, 1024);///#include "NeuralNet.h"
  file.getline(buffer, 1024);///using namespace Neural;
  file.getline(buffer, 1024);///\n
  file.getline(buffer, 1024);///std::vector<NeuralNet::Weights> NeuralNet::sWeightsBeforeLayers = {
  
  sWeightsBeforeLayers.clear();

  while (!file.eof()) {
    Weights weights;
    while (file.peek() != '}') {
      double value = 0;
      file >> value;
      weights.push_back(value);
    }
    file.getline(buffer, 1024);

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
  file << "std::vector<NeuralNet::Weights> NeuralNet::sWeightsBeforeLayers = {" << std::endl;

  for (size_t weightsIter = 0; weightsIter < sWeightsBeforeLayers.size(); ++weightsIter) {
    file << "{";

    const auto* pWeights = sWeightsBeforeLayers[weightsIter].data();
    const size_t weightsSize = sWeightsBeforeLayers[weightsIter].size();

    char separator = ' ';
    for (size_t i = 0; i < weightsSize; ++i) {
      file << separator << std::setprecision(5) << pWeights[i];
      separator = ',';
    }

    if (weightsIter + 1 == sWeightsBeforeLayers.size()) {
      file << "}" << std::endl;
    } else {
      file << "}," << std::endl;
    }
    
  }

  file << "};" << std::endl;
}

#else
const NeuralNet::WeightsBeforeLayers& NeuralNet::getWeights() const {
  return sWeightsBeforeLayers;
}
#endif