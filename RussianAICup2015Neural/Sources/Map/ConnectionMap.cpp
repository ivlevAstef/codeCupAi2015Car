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

size_t ConnectionMap::sConnectionPointsCount = 0;
size_t ConnectionMap::sMapWidth = 0;
size_t ConnectionMap::sMapHeight = 0;

std::vector<ConnectionJoin> ConnectionMap::joinsMemory;
ConnectionJoin* ConnectionMap::pJoinsMemory = NULL;

std::vector<ConnectionPointData> ConnectionMap::data;
ConnectionPointData* ConnectionMap::pData = NULL;

void ConnectionMap::reMemory() {
  sMapWidth = Constants::instance().game.getWorldWidth();
  sMapHeight = Constants::instance().game.getWorldHeight();

  sConnectionPointsCount = connectionPointIndex(sMapWidth - 1, sMapHeight - 1, 1, 0);

  if (joinsMemory.size() != sConnectionPointsCount * sConnectionPointsCount) {
    joinsMemory.resize(sConnectionPointsCount * sConnectionPointsCount);
    pJoinsMemory = joinsMemory.data();
  }
  
  if (data.size() != sConnectionPointsCount) {
    data.resize(sConnectionPointsCount);
    pData = data.data();
  }

  for (size_t i = 0; i < sConnectionPointsCount; ++i) {
    auto& pointData = pData[i];
    pointData.joins.clear();
    pointData.joins.reserve(sMaxConnectionJoinsInTile);
  }
}

void ConnectionMap::update(const model::World& world) {
  createConnectionData(world);
  removeSingleConnections();
}

void ConnectionMap::updateWeightForCar(const model::Car& car, const model::World& world) {
  const size_t joinsMemorySize = joinsMemory.size();
  for (size_t i = 0; i < joinsMemorySize; i++) {
    pJoinsMemory[i].weight = 0;
  }

  const auto& tiles = world.getTilesXY();

  const auto& bonuses = world.getBonuses();
  const size_t bonusesSize = bonuses.size();

  for (size_t i = 0; i < bonusesSize; ++i) {
    const auto& bonus = bonuses[i];

    const SIA::Position tile = tilePosition(bonus.getX(), bonus.getY());
    const model::TileType& tileType = tiles[tile.x][tile.y];

    auto& directions = directionsByTileType(tileType);
    const size_t directionsSize = directions.size();

    for (size_t i1 = 0; i1 < directionsSize; ++i1) {
      const auto& dir1 = directions[i1];

      for (size_t i2 = i1 + 1; i2 < directionsSize; ++i2) {
        const auto& dir2 = directions[i2];

        if (checkBelongsBonusToJoin(bonus, tile, dir1, dir2)) {
          const PointIndex index1 = connectionPointIndex(tile.x, tile.y, dir1.x, dir1.y);
          const PointIndex index2 = connectionPointIndex(tile.x, tile.y, dir2.x, dir2.y);

          ConnectionJoin& join = joinByPointIndexes(index1, index2);
          join.weight += Constants::bonusPriorityForCar(bonus.getType(), car);
        }
      }
    }
  }
}

bool ConnectionMap::checkBelongsBonusToJoin(const model::Bonus& bonus, const SIA::Position& tile, const SIA::Position& dir1, const SIA::Position& dir2) const {
  static const double sTileSize = Constants::instance().game.getTrackTileSize();
  static const double sTileMargin = Constants::instance().game.getTrackTileMargin();

  SIA::Vector p1 = toRealPoint(tile.x, tile.y, dir1.x, dir1.y);
  SIA::Vector normal(dir1.y - dir2.y, - (dir1.x - dir2.x));
  normal /= normal.length();

  SIA::Vector bonusPos(bonus.getX(), bonus.getY());

  const double maxDistance = (sTileSize * 0.5 - sTileMargin) / (abs(normal.x) + abs(normal.y));

  return abs((bonusPos - p1).dot(normal)) < maxDistance;
}

PointIndex ConnectionMap::getPointIndexByTileAndDir(int x, int y, int dx, int dy) const {
  return connectionPointIndex(x, y, dx, dy);
}

bool ConnectionMap::validPointIndex(PointIndex index) const {
  return (0 <= index && index < data.size()) && !pData[index].joins.empty();
}

PointIndex ConnectionMap::invalidPointIndex() const {
  return PointIndex(data.size());
}

const ConnectionPointData& ConnectionMap::getConnectionPointByIndex(PointIndex index) const {
  SIAAssert(index <= data.size());
  return pData[index];
}

const std::vector<SIA::Position> ConnectionMap::getTiles(PointIndex index) const {
  SIAAssert(index <= data.size());

  const SIA::Vector pos = pData[index].pos;
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
  for (size_t x = 0; x < sMapWidth; x++) {
    for (size_t y = 0; y < sMapHeight; y++) {
      fillJoinsMemoryForTile(world, x, y);
      fillConnectionDataForTile(world, x, y);
    }
  }
}

void ConnectionMap::fillJoinsMemoryForTile(const model::World& world, size_t x, size_t y) {
  const auto& tiles = world.getTilesXY();
  const model::TileType& tileType = tiles[x][y];

  auto& directions = directionsByTileType(tileType);
  const size_t directionsSize = directions.size();

  for (size_t i1 = 0; i1 < directionsSize; ++i1) {
    const auto& dir1 = directions[i1];
    const PointIndex index1 = connectionPointIndex(x, y, dir1.x, dir1.y);

    for (size_t i2 = i1 + 1; i2 < directionsSize; ++i2) {
      const auto& dir2 = directions[i2];
      const PointIndex index2 = connectionPointIndex(x, y, dir2.x, dir2.y);

      if (index1 != index2) {
        ConnectionJoin& join = joinByPointIndexes(index1, index2);
        join.index1 = MIN(index1, index2);
        join.index2 = MAX(index1, index2);

        join.length = (dir1 - dir2).length();
        join.weight = 0;
        join.userInfo = NULL;
      }
    }
  }
}

void ConnectionMap::fillConnectionDataForTile(const model::World& world, size_t x, size_t y) {
  const auto& tiles = world.getTilesXY();
  const model::TileType& tileType = tiles[x][y];

  const auto& directions = directionsByTileType(tileType);
  const size_t directionsSize = directions.size();

  for (size_t i1 = 0; i1 < directionsSize; ++i1) {
    const auto& dir1 = directions[i1];
    const PointIndex index1 = connectionPointIndex(x, y, dir1.x, dir1.y);

    auto& pointData = pData[index1];
    pointData.pos = toRealPoint(x, y, dir1.x, dir1.y);

    for (size_t i2 = 0; i2 < directionsSize; ++i2) {
      const auto& dir2 = directions[i2];
      const PointIndex index2 = connectionPointIndex(x, y, dir2.x, dir2.y);

      if (index1 != index2) {
        ConnectionJoinData joinData = {index2, &joinByPointIndexes(index1, index2)};
        pointData.joins.push_back(joinData);
      }
    }
  }
}

void ConnectionMap::removeSingleConnections() {
  const size_t dataSize = data.size();

  for (size_t index = 0; index < dataSize; ++index) {
    auto& pointData = pData[index];
    const size_t joinsSize = pointData.joins.size();

    for (size_t i = 0; i < joinsSize; ++i) {
      if (!checkConnection(pointData.joins[i].index, index)) {
        pointData.joins.erase(pointData.joins.begin() + i);
        i--;
      }
    }
  }
}

bool ConnectionMap::checkConnection(size_t fromIndex, size_t toIndex) const {
  const auto& joins = pData[fromIndex].joins;
  const size_t joinsSize = joins.size();

  for (size_t i = 0; i < joinsSize; ++i) {
    if (joins[i].index == toIndex) {
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

SIA::Vector ConnectionMap::toRealPoint(int x, int y, int dx, int dy) {
  return vectorByAnchor(x, y, (double)(dx + 1) * 0.5, (double)(dy + 1) * 0.5);
}

PointIndex ConnectionMap::connectionPointIndex(int x, int y, int dx, int dy) {
  return PointIndex(2 * (x + y * sMapWidth) + dx + 2 * ((-1 == dy) ? -(int(sMapWidth) - 1) : dy) - 1);
}

ConnectionJoin& ConnectionMap::joinByPointIndexes(PointIndex index1, PointIndex index2) {
  PointIndex minIndex = MIN(index1, index2);
  PointIndex maxIndex = MAX(index1, index2);

  auto key = uint32_t(minIndex) + uint32_t(maxIndex) * sConnectionPointsCount;
  return pJoinsMemory[key];
}

#ifdef ENABLE_VISUALIZATOR
void ConnectionMap::visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) const {
  static const double pointR = 10;

  const size_t dataSize = data.size();
  const auto* pData = data.data();

  for (size_t i = 0; i < dataSize; ++i) {
    const auto& pointData = pData[i];

    if (!pointData.joins.empty()) {
      visualizator.fillCircle(pointData.pos.x, pointData.pos.y, pointR, color);
    }
  }
}

void ConnectionMap::visualizationConnectionJoins(const Visualizator& visualizator, int32_t color) const {
  const size_t dataSize = data.size();
  const auto* pData = data.data();

  for (size_t i = 0; i < dataSize; ++i) {
    const auto& pointData = pData[i];

    const auto& joins = pointData.joins;
    const size_t joinsSize = joins.size();

    for (size_t j = 0; j < joinsSize; ++j) {
      const auto& join = joins[j];

      if (join.index > i) /*for don't show twice*/ {

        const auto& p1 = pointData.pos;
        const auto& p2 = pData[join.index].pos;

        const auto& center = p1 + (p2 - p1) * 0.5;

        SIAAssert(NULL != join.data);
        visualizator.text(center.x, center.y, join.data->length, color);
        visualizator.text(center.x, center.y + 35, join.data->weight, 0x000000);
        visualizator.line(p1.x, p1.y, p2.x, p2.y, color);
      }
    }
  }
}

#endif