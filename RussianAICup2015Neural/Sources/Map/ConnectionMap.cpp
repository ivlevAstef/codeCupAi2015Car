//
//File: ConnectionMap.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 9:44 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//


#include "ConnectionMap.h"
#include "Common/Extensions.h"
#include "Common/Constants.h"
#include "Common/SIALogger.h"

const SIA::Position ConnectionMap::sDirUp = SIA::Position(0, -1);
const SIA::Position ConnectionMap::sDirDown = SIA::Position(0, 1);
const SIA::Position ConnectionMap::sDirLeft = SIA::Position(-1, 0);
const SIA::Position ConnectionMap::sDirRight = SIA::Position(1, 0);

void ConnectionMap::update(const model::World& world) {
  createConnectionPoints(world);
}

int ConnectionMap::connectionPointsBySize(int width, int heigth) {
  return connectionPointIndex(width - 1, heigth - 1, 1, 0);//last inadmissible index
}

void ConnectionMap::createConnectionPoints(const model::World& world) {
  points.clear();
  points.resize(connectionPointsBySize(world.getWidth(), world.getHeight()));

  for (int x = 0; x < world.getWidth(); x++) {
    for (int y = 0; y < world.getHeight(); y++) {
      fillConnectionPointsByTile(world, x, y);
    }
  }
}

void ConnectionMap::fillConnectionPointsByTile(const model::World& world, int x, int y) {
  const auto tiles = world.getTilesXY();
  const model::TileType tileType = tiles[x][y];

  auto directions = directionsByTileType(tileType);

  for (const SIA::Position& dir : directions) {
    size_t index = connectionPointIndex(x, y, dir.x, dir.y);
    SIAAssert(0 <= index && index < points.size());

    points[index] = toConnectionPoint(x, y, dir.x, dir.y);
  }
}

const std::vector<SIA::Position>& ConnectionMap::directionsByTileType(const model::TileType& type) {
  static const std::vector<SIA::Position> empty;
  static const std::vector<SIA::Position> vertical {sDirUp, sDirDown};
  static const std::vector<SIA::Position> horizontal {sDirLeft, sDirRight};
  static const std::vector<SIA::Position> leftTop {sDirRight, sDirDown};
  static const std::vector<SIA::Position> rightTop {sDirLeft, sDirDown};
  static const std::vector<SIA::Position> leftBottom {sDirRight, sDirUp};
  static const std::vector<SIA::Position> rightBottom {sDirLeft, sDirUp};
  static const std::vector<SIA::Position> leftT {sDirLeft, sDirUp, sDirDown};
  static const std::vector<SIA::Position> rightT {sDirRight, sDirUp, sDirDown};
  static const std::vector<SIA::Position> topT {sDirUp, sDirLeft, sDirRight};
  static const std::vector<SIA::Position> bottomT {sDirDown, sDirLeft, sDirRight};
  static const std::vector<SIA::Position> cross {sDirUp, sDirDown, sDirLeft, sDirRight};

  switch (type) {
    case model::EMPTY:
      return empty;
    case model::VERTICAL:
      return vertical;
    case model::HORIZONTAL:
      return horizontal;
    case model::LEFT_TOP_CORNER:
      return leftTop;
    case model::RIGHT_TOP_CORNER:
      return rightTop;
    case model::LEFT_BOTTOM_CORNER:
      return leftBottom;
    case model::RIGHT_BOTTOM_CORNER:
      return rightBottom;
    case model::LEFT_HEADED_T:
      return leftT;
    case model::RIGHT_HEADED_T:
      return rightT;
    case model::TOP_HEADED_T:
      return topT;
    case model::BOTTOM_HEADED_T:
      return bottomT;
    case model::CROSSROADS:
    case model::UNKNOWN:
      return cross;
  }

  SIAAssertMsg(false, "Unknown type:%d", type);
  return empty;
}

void ConnectionMap::visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) {
  static const double pointR = 10;
  for (ConnectionPoint point : points) {
    visualizator.fillCircle(point.x, point.y, pointR, color);
  }
}

ConnectionMap::ConnectionPoint ConnectionMap::toConnectionPoint(int x, int y, int dx, int dy) const {
  return vectorByAnchor(x, y, (double)(dx + 1) * 0.5, (double)(dy + 1) * 0.5);
}

size_t ConnectionMap::connectionPointIndex(int x, int y, int dx, int dy) const {
  const int width = Constants::instance().game.getWorldWidth();
  return 2 * (x + y * width) + dx + 2 * ((-1 == dy) ? -(width - 1) : dy) - 1;
}