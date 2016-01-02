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

#include <algorithm>

const size_t ConnectionMap::sMaxConnectionJoinsInTile = 6;

const SIA::Position ConnectionMap::sDirUp = SIA::Position(0, -1);
const SIA::Position ConnectionMap::sDirDown = SIA::Position(0, 1);
const SIA::Position ConnectionMap::sDirLeft = SIA::Position(-1, 0);
const SIA::Position ConnectionMap::sDirRight = SIA::Position(1, 0);

void ConnectionMap::update(const model::World& world) {
  createConnectionData(world);
  removeSingleConnections();
}

PointIndex ConnectionMap::getPointIndexByTileAndDir(int x, int y, int dx, int dy) const {
  return connectionPointIndex(x, y, dx, dy);
}

bool ConnectionMap::validPointIndex(PointIndex index) const {
  return (0 <= index && index < data.size()) && !data[index].joins.empty();
}

PointIndex ConnectionMap::invalidPointIndex() const {
  return data.size();
}

const ConnectionPointData& ConnectionMap::getConnectionPointByIndex(PointIndex index) const {
  SIAAssert(index <= data.size());
  return data[index];
}

const std::vector<SIA::Position> ConnectionMap::getTiles(PointIndex index) const {
  SIAAssert(index <= data.size());

  const SIA::Vector pos = data[index].pos;
  const double dist = Constants::instance().game.getTrackTileSize() * 0.25;

  std::vector<SIA::Position> result;
  result.resize(2);

  result[0] = tilePosition(pos.x + dist, pos.y + dist);
  result[1] = tilePosition(pos.x - dist, pos.y - dist);

  return result;
}

const size_t ConnectionMap::getPointCount() const {
  return data.size();
}


void ConnectionMap::createConnectionData(const model::World& world) {
  data.clear();
  data.resize(countConnectionPointsBySize(world.getWidth(), world.getHeight()));

  for (auto& pointData : data) {
    pointData.joins.reserve(sMaxConnectionJoinsInTile);
  }

  for (int x = 0; x < world.getWidth(); x++) {
    for (int y = 0; y < world.getHeight(); y++) {
      fillConnectionDataForTile(world, x, y);
    }
  }
}

void ConnectionMap::fillConnectionDataForTile(const model::World& world, size_t x, size_t y) {
  const auto& tiles = world.getTilesXY();
  const model::TileType& tileType = tiles[x][y];

  auto& directions = directionsByTileType(tileType);

  for (size_t i = 0; i < directions.size(); i++) {
    const SIA::Position& dir1 = directions[i];
    auto& pointData = data[connectionPointIndex(x, y, dir1.x, dir1.y)];
    pointData.pos = toRealPoint(x, y, dir1.x, dir1.y);

    for (size_t j = 0; j < directions.size(); j++) {
      if (i == j) {
        continue;
      }
      const SIA::Position& dir2 = directions[j];

      ConnectionJoin join = {0};
      join.index = connectionPointIndex(x, y, dir2.x, dir2.y);
      join.length = (dir1 - dir2).length();
      join.weight = 0;//TODO: set weight
      pointData.joins.push_back(join);
    }
  }
}

void ConnectionMap::removeSingleConnections() {
  for (size_t index = 0; index < data.size(); index++) {
    auto& pointData = data[index];

    pointData.joins.erase(std::remove_if(pointData.joins.begin(), pointData.joins.end(), [index, this] (const ConnectionJoin& cell) {
      return !checkConnection(cell.index, index);
    }), pointData.joins.end());
  }
}

bool ConnectionMap::checkConnection(size_t fromIndex, size_t toIndex) const {
  for (const auto& join : data[fromIndex].joins) {
    if (join.index == toIndex) {
      return true;
    }
  }
  return false;
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

SIA::Vector ConnectionMap::toRealPoint(int x, int y, int dx, int dy) const {
  return vectorByAnchor(x, y, (double)(dx + 1) * 0.5, (double)(dy + 1) * 0.5);
}

PointIndex ConnectionMap::connectionPointIndex(int x, int y, int dx, int dy) const {
  const int width = Constants::instance().game.getWorldWidth();
  return 2 * (x + y * width) + dx + 2 * ((-1 == dy) ? -(width - 1) : dy) - 1;
}

int ConnectionMap::countConnectionPointsBySize(size_t width, size_t heigth) const {
  return connectionPointIndex(width - 1, heigth - 1, 1, 0);//last inadmissible index
}

#ifdef ENABLE_VISUALIZATOR
void ConnectionMap::visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) const {
  static const double pointR = 10;

  for (const auto& pointData : data) {
    if (!pointData.joins.empty()) {
      visualizator.fillCircle(pointData.pos.x, pointData.pos.y, pointR, color);
    }
  }
}

void ConnectionMap::visualizationConnectionJoins(const Visualizator& visualizator, int32_t color) const {
  for (const auto& pointData : data) {
    for (const auto& join : pointData.joins) {
      const auto& p1 = pointData.pos;
      const auto& p2 = data[join.index].pos;

      const auto& center = p1 + (p2 - p1) * 0.5;

      visualizator.text(center.x, center.y, join.length, color);
      visualizator.line(p1.x, p1.y, p2.x, p2.y, color);
    }
  }
}

#endif