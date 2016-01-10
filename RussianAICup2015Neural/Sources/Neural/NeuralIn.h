//
//File: NeuralIn.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 19:37 9/1/2016
//Copyright (c) SIA 2015. All Right Reserved.
//


#pragma once
#ifndef _NEURAL_IN_H__
#define _NEURAL_IN_H__

#include "model/World.h"
#include "Map/PathFinder.h"
  
namespace Neural
{
  class NeuralIn {
  public:
    static const size_t sInputValuesCount;

  public:
    NeuralIn(const model::World& world, const std::vector<Map::PathFinder>& paths);

    const std::vector<double>& values() const;

  private:
    void calculateInputValue(const model::World& world, const std::vector<Map::PathFinder>& paths);

    std::vector<double> inputValues;
  };

};

#endif