//
//File: NeuralCalculator.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:51 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _NEURAL_CALCULATOR_H__
#define _NEURAL_CALCULATOR_H__

#include "NeuralIn.h"
#include "NeuralNet.h"
#include "NeuralOut.h"

namespace Neural
{
  class NeuralCalculator {
  public:
    NeuralCalculator(const NeuralNet& net);

    const NeuralOut calculate(const NeuralIn& input) const;

  private:
    const NeuralNet& net;
  };

};

#endif