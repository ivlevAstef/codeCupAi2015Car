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

#ifdef ENABLE_VISUALIZATOR
#include "Visualizator/Visualizator.h"
#endif

struct ConnectionJoin {
  size_t pointIndex;
  double weight;
  double length;
};

struct ConnectionPointData {
  SIA::Vector pos;
  std::vector<ConnectionJoin> joins;
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

  const ConnectionPointData& getJoinsByTileAndDir(int x, int y, int dx, int dy);
  const ConnectionPointData& getJoinsByIndex(size_t pointIndex);
  const size_t getPointCount();

#ifdef ENABLE_VISUALIZATOR
  void visualizationConnectionPoints(const Visualizator& visualizator, int32_t color) const;
  void visualizationConnectionJoins(const Visualizator& visualizator, int32_t color) const;
#endif

private:
  void createConnectionData(const model::World& world);
  void fillConnectionDataForTile(const model::World& world, size_t x, size_t y);
  ///methods worked only for Unknown tiles
  void removeSingleConnections();
  bool checkConnection(size_t fromIndex, size_t toIndex) const;
  ///end methods

  const std::vector<SIA::Position>& directionsByTileType(const model::TileType& type);

private:
  SIA::Vector toRealPoint(int x, int y, int dx, int dy) const;
  size_t connectionPointIndex(int x, int y, int dx, int dy) const;
  int countConnectionPointsBySize(size_t width, size_t heigth) const;

  std::vector<ConnectionPointData> data;

};

#endif