//
//File: ConnectionMap.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 9:44 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//


#include "ConnectionMap.h"
#include "Common/Extensions.h"

const int ConnectionMap::sMaxConnectionPointsInTile = 4;

void ConnectionMap::update(const model::World& world) {
  createConnectionPoints(world);
}

int ConnectionMap::connectionPointsBySize(int width, int heigth) {
  const int pT = sMaxConnectionPointsInTile;
  return pT + (pT - 1) * (width + heigth - 2) + (pT - 2) * (width - 1) * (heigth - 1);
}

void ConnectionMap::createConnectionPoints(const model::World& world) {
  points.clear();
  points.reserve(connectionPointsBySize(world.getWidth(), world.getHeight()));

  for (int x = 0; x < world.getWidth(); x++) {
    for (int y = 0; y < world.getHeight(); y++) {
      fillConnectionPointsByTile(world, x, y);
    }
  }
}

void ConnectionMap::fillConnectionPointsByTile(const model::World& world, int x, int y) {
  const auto tiles = world.getTilesXY();
  const model::TileType tileType = tiles[x][y];

  SIA::Vector anchorPoints[sMaxConnectionPointsInTile];
  const size_t anchorPointsCount = anchorsByTileTyle(tileType, anchorPoints);

  for (size_t i = 0; i < anchorPointsCount; i++) {
    ConnectionPoint point = vectorByAnchor(x, y, anchorPoints[i].x, anchorPoints[i].y);
    points.push_back(point);
  }
}

#define UP 0.5, 0
#define DOWN 0.5, 1
#define LEFT 0, 0.5
#define RIGHT 1, 0.5

size_t ConnectionMap::anchorsByTileTyle(const model::TileType& type, SIA::Vector* data) {
  switch (type) {
    case model::EMPTY:
      return 0;
    case model::VERTICAL:
      data[0].set(UP);
      data[1].set(DOWN);
      return 2;
    case model::HORIZONTAL:
      data[0].set(LEFT);
      data[1].set(RIGHT);
      return 2;
    case model::LEFT_TOP_CORNER:
      data[0].set(RIGHT);
      data[1].set(DOWN);
      return 2;
    case model::RIGHT_TOP_CORNER:
      data[0].set(LEFT);
      data[1].set(DOWN);
      return 2;
    case model::LEFT_BOTTOM_CORNER:
      data[0].set(RIGHT);
      data[1].set(UP);
      return 2;
    case model::RIGHT_BOTTOM_CORNER:
      data[0].set(LEFT);
      data[1].set(UP);
      return 2;
    case model::LEFT_HEADED_T:
      data[0].set(LEFT);
      data[1].set(UP);
      data[2].set(DOWN);
      return 3;
    case model::RIGHT_HEADED_T:
      data[0].set(RIGHT);
      data[1].set(UP);
      data[2].set(DOWN);
      return 3;
    case model::TOP_HEADED_T:
      data[0].set(UP);
      data[1].set(LEFT);
      data[2].set(RIGHT);
      return 3;
    case model::BOTTOM_HEADED_T:
      data[0].set(DOWN);
      data[1].set(LEFT);
      data[2].set(RIGHT);
      return 3;
    case model::CROSSROADS:
    case model::UNKNOWN:
      data[0].set(LEFT);
      data[1].set(RIGHT);
      data[2].set(UP);
      data[3].set(DOWN);
      return 4;
  }

  return 0;
}

#undef UP
#undef DOWN
#undef LEFT
#undef RIGHT


void ConnectionMap::visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) {
  static const double pointR = 10;
  for (ConnectionPoint point : points) {
    visualizator.fillCircle(point.x, point.y, pointR, color);
  }
}
