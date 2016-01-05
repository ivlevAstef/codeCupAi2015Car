//
//File: ConnectionMap.h
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 9:35 28/12/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#pragma once
#ifndef _CONNECTION_MAP_H__
#define _CONNECTION_MAP_H__

#include "model/World.h"
#include "model/Game.h"
#include "Common/SIASingleton.h"
#include "Common/SIAPoint2D.h"
#include <unordered_map>

#ifdef ENABLE_VISUALIZATOR
#include "Visualizator/Visualizator.h"
#endif

typedef uint16_t PointIndex;

struct ConnectionJoin {
  PointIndex index1;
  PointIndex index2;

  double length;
  double weight;
  double* userInfo;
};

struct ConnectionJoinData {
  PointIndex index;
  ConnectionJoin* data;
};

struct ConnectionPointData {
  SIA::Vector pos;
  std::vector<ConnectionJoinData> joins;
};

class ConnectionMap {
private:
  static const size_t sMaxConnectionJoinsInTile;
  static const SIA::Position sDirUp;
  static const SIA::Position sDirDown;
  static const SIA::Position sDirLeft;
  static const SIA::Position sDirRight;

public:
  void update(const model::World& world);

  PointIndex getPointIndexByTileAndDir(int x, int y, int dx, int dy) const;
  bool validPointIndex(PointIndex index) const;
  PointIndex invalidPointIndex() const;
  const ConnectionPointData& getConnectionPointByIndex(PointIndex index) const;
  const std::vector<SIA::Position> getTiles(PointIndex index) const;
  const size_t getPointCount() const;

#ifdef ENABLE_VISUALIZATOR
  void visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) const;
  void visualizationConnectionJoins(const Visualizator& visualizator, int32_t color) const;
#endif

private:
  void createConnectionData(const model::World& world);
  void fillConnectionDataForTile(const model::World& world, size_t x, size_t y);
  void fillJoinsMemoryForTile(const model::World& world, size_t x, size_t y);

  void removeSingleConnections();
  bool checkConnection(size_t fromIndex, size_t toIndex) const;

  void createBonusesByTiles(const model::World& world);
  double weightForTileAndDir(const model::World& world, size_t x, size_t y, const SIA::Position& dir1, const SIA::Position& dir2) const;

  static const std::vector<SIA::Position>& directionsByTileType(const model::TileType& type);

private:
  SIA::Vector toRealPoint(int x, int y, int dx, int dy) const;
  PointIndex connectionPointIndex(int x, int y, int dx, int dy) const;
  int countConnectionPointsBySize(size_t width, size_t heigth) const;

  ConnectionJoin& joinByPointIndexes(PointIndex index1, PointIndex index2);

  std::vector<ConnectionPointData> data;
  std::unordered_map<uint32_t, ConnectionJoin> joinsMemory;

  std::vector<std::vector<std::vector<model::Bonus>>> bonusesByTiles;

};

#endif