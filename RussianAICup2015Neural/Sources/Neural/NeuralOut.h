//
//File: NeuralOut.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:42 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _NEURAL_OUT_H__
#define _NEURAL_OUT_H__

#include "model/Move.h"
#include <vector>

namespace Neural
{
  class NeuralOut {
  public:
    NeuralOut(const std::vector<double>& output);

    model::Move getMove() const;
    void fillMove(model::Move& moveForFill) const;

  private:
    void calculateMove(const std::vector<double>& output);

  private:
    model::Move move;
  };

};

#endif