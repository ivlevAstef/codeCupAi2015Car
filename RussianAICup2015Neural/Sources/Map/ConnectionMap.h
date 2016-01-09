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
#include "Common/SIAPoint2D.h"
#include <stdint.h>

#ifdef ENABLE_VISUALIZATOR
#include "Visualizator/Visualizator.h"
#endif

namespace Map
{

  typedef uint16_t PointIndex;

  struct ConnectionJoin {
    PointIndex index1;
    PointIndex index2;

    double length;
    double weight;
    double angle;
  };

  struct ConnectionJoinData {
    PointIndex index;
    ConnectionJoin* data;

    ConnectionJoinData(PointIndex index, ConnectionJoin* data) : index(index), data(data) {
    }
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
    static void reMemory();

    void update(const model::World& world);
    void updateWeightForCar(const model::Car& car, const model::World& world);

    PointIndex getPointIndexByTileAndDir(int x, int y, int dx, int dy) const;
    bool validPointIndex(PointIndex index) const;
    PointIndex invalidPointIndex() const;
    const ConnectionPointData& getConnectionPointByIndex(PointIndex index) const;
    const ConnectionJoinData& getConnectionJoinByIndexes(PointIndex index1, PointIndex index2) const;
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

    bool checkConnection(size_t fromIndex, size_t toIndex) const;

    bool checkBelongsBonusToJoin(const model::Bonus& bonus, const SIA::Position& tile, const SIA::Position& dir1, const SIA::Position& dir2) const;

    static const std::vector<SIA::Position>& directionsByTileType(model::TileType type, const model::World& world, size_t x, size_t y);

  private:
    static SIA::Vector toRealPoint(int x, int y, int dx, int dy);
    static PointIndex connectionPointIndex(int x, int y, int dx, int dy);

    ConnectionJoin& joinByPointIndexes(PointIndex index1, PointIndex index2);

  private:
    static size_t sMapWidth;
    static size_t sMapHeight;
    static size_t sConnectionPointsCount;

    static std::vector<ConnectionJoin> joinsMemory;
    static ConnectionJoin* pJoinsMemory;//fast
    static std::vector<ConnectionPointData> data;
    static ConnectionPointData* pData;//fast

  };

};

#endif