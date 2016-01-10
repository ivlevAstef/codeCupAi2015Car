//
//File: NeuralTraining.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 15:02 10/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _NEURAL_TRAINING_H__
#define _NEURAL_TRAINING_H__

#include "Common/SIASingleton.h"
#include "NeuralNet.h"
#include "NeuralIn.h"
#include "NeuralOut.h"

namespace Neural
{
  class NeuralTraining : public SIA::Singleton<NeuralTraining> {
  private:
    static NeuralTraining singleton;
  public:
    NeuralTraining();

    void update(const NeuralIn& input, const NeuralOut& output);

    inline const NeuralNet& getNet() const { return net; }

  private:
    NeuralNet net;
  };

};

#endif