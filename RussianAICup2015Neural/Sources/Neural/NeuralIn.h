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
    NeuralIn(const model::Car& car, const model::World& world, const Map::PathFinder& path);

    const std::vector<double>& input() const;

  private:
    void calculateInputValue(const model::Car& car, const model::World& world, const Map::PathFinder& path);


  private:
    std::vector<double> inputValues;
  };

};

#endif