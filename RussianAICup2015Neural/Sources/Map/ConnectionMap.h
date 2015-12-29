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
  static const SIA::Position sDirUp;
  static const SIA::Position sDirDown;
  static const SIA::Position sDirLeft;
  static const SIA::Position sDirRight;

public:
  void update(const model::World& world);

  void visualizationConnectionPoints(const Visualizator& visualizator, int32_t color);

private:
  int connectionPointsBySize(int width, int heigth);
  void createConnectionPoints(const model::World& world);
  void fillConnectionPointsByTile(const model::World& world, int x, int y);
  const std::vector<SIA::Position>& directionsByTileType(const model::TileType& type);

private:
  typedef SIA::Vector ConnectionPoint;

  ConnectionPoint toConnectionPoint(int x, int y, int dx, int dy) const;
  size_t connectionPointIndex(int x, int y, int dx, int dy) const;

  std::vector<ConnectionPoint> points;

};