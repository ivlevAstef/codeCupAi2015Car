//
//File: NeuralNet.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:20 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//


#pragma once
#ifndef _NEURAL_NET_H__
#define _NEURAL_NET_H__

#include <vector>

namespace Neural
{
  class NeuralNet {
  public:
    typedef std::vector<double> Weights;
    typedef std::vector<Weights> WeightsBeforeLayers;

  public:
    NeuralNet() = default;

    const WeightsBeforeLayers& getWeights() const;

#ifdef ENABLE_NET_LEARNING
    NeuralNet(std::string fileName);
    void setWeights(const WeightsBeforeLayers& weights);
#endif

  private:
    std::vector<Weights> weightsBeforeLayers;
    static std::vector<Weights> sDefaultWeightsBeforeLayers;
  };

};

#endif