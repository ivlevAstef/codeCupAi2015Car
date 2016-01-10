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

#ifdef ENABLE_NET_LEARNING
    WeightsBeforeLayers& getWeights() const;
    void rand();
    void load();
    void save();
#else
    const WeightsBeforeLayers& getWeights() const;
#endif

  private:
    static std::vector<Weights> sWeightsBeforeLayers;
  };

};

#endif