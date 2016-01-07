#include "MyStrategy.h"

#include "Common/SIALogger.h"
#include "Common/Constants.h"

#include "Map/ConnectionMap.h"
#include "Map/PathFinder.h"

using namespace model;
using namespace std;

void MyStrategy::move(const Car& car, const World& world, const Game& game, Move& move) {
  volatile Constants constants = Constants(game);
  ConnectionMap::reMemory();

  //move.setThrowProjectile(true);
  move.setSpillOil(true);

  if (world.getTick() > game.getInitialFreezeDurationTicks()) {
    move.setUseNitro(true);
  }

  ConnectionMap map;
  map.update(world);
  map.updateWeightForCar(car, world);

  PathFinder path;
  path.findPath(car, world, map, 6);

  double angleToWaypoint = car.getAngleTo(path.getPath()[1].x, path.getPath()[1].y);

  move.setWheelTurn(angleToWaypoint * 32.0 / 3.14);
  move.setEnginePower(0.75);

  double speedModule = SIA::Vector(car.getSpeedX(), car.getSpeedY()).length();
  if (speedModule * speedModule * abs(angleToWaypoint) > 10 * 3.14) {
    move.setBrake(true);
  }

#ifdef ENABLE_VISUALIZATOR
  Visualizator::setWindowCenter(car.getX(), car.getY(), world.getWidth() * game.getTrackTileSize(), world.getHeight() * game.getTrackTileSize());
  visualizator.beginPost();

  map.visualizationConnectionJoins(visualizator, 0x000077);
  map.visualizationConnectionPoints(visualizator, 0x000000);
  path.visualizationPath(visualizator, 0xFF0000);
  path.visualizationPointWeight(visualizator, 0xFF00FF, map);

  visualizator.endPost();
#endif
}

MyStrategy::MyStrategy() { }
