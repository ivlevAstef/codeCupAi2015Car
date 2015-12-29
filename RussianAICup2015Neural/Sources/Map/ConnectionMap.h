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
  static const size_t sMaxConnectionJoinsInTile;
  static const SIA::Position sDirUp;
  static const SIA::Position sDirDown;
  static const SIA::Position sDirLeft;
  static const SIA::Position sDirRight;

public:
  void update(const model::World& world);

  void visualizationConnectionPoints(const Visualizator& visualizator, int32_t color);
  void visualizationConnectionJoins(const Visualizator& visualizator, int32_t color);

private:
  int connectionPointsBySize(int width, int heigth);

  void createConnectionPoints(const model::World& world);
  void fillConnectionPointsByTile(const model::World& world, int x, int y);

  void createConnectionJoins(const model::World& world);
  void fillConnectionJoinsInTile(const model::World& world, int x, int y);

  const std::vector<SIA::Position>& directionsByTileType(const model::TileType& type);

private:
  typedef SIA::Vector ConnectionPoint;

  ConnectionPoint toConnectionPoint(int x, int y, int dx, int dy) const;
  size_t connectionPointIndex(int x, int y, int dx, int dy) const;

  std::vector<ConnectionPoint> points;

  struct ConnectionJoin {
    static const ConnectionPoint sDefaultConnectionPoint;
    double length;
    double weight;

    ConnectionJoin();
    ConnectionJoin(const ConnectionPoint& p1, const ConnectionPoint& p2);

    inline const ConnectionPoint& getP1() const { return *p1; }
    inline const ConnectionPoint& getP2() const { return *p2; }

  private:
    const ConnectionPoint* p1;
    const ConnectionPoint* p2;
  };

  typedef std::vector<ConnectionJoin> ConnectionJoins;

  std::vector<std::vector<ConnectionJoins>> joinsByTiles;

};