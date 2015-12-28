//
//File: ConnectionMap.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 9:35 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "model/World.h"
#include "model/Game.h"
#include "Common/SIASingleton.h"
#include "Common/SIAPoint2D.h"
#include "Visualizator/Visualizator.h"

class ConnectionMap {
private:
  static const int sMaxConnectionPointsInTile;

public:
  void update(const model::World& world);

  void visualizationConnectionPoints(const Visualizator& visualizator, int32_t color);

private:
  int connectionPointsBySize(int width, int heigth);
  void createConnectionPoints(const model::World& world);
  void fillConnectionPointsByTile(const model::World& world, int x, int y);
  size_t anchorsByTileTyle(const model::TileType& type, SIA::Vector* data);

private:
  typedef SIA::Vector ConnectionPoint;

  std::vector<ConnectionPoint> points;

};