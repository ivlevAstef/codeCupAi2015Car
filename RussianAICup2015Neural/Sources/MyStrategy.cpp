#include "MyStrategy.h"

#include "Common/SIALogger.h"
#include "Common/Constants.h"

#include "Map/ConnectionMap.h"
#include "Map/PathFinder.h"

using namespace model;
using namespace std;

void MyStrategy::move(const Car& car, const World& world, const Game& game, Move& move) {
  volatile Constants constants = Constants(game);

  move.setEnginePower(1.0);
  move.setThrowProjectile(true);
  move.setSpillOil(true);

  if (world.getTick() > game.getInitialFreezeDurationTicks()) {
    move.setUseNitro(true);
  }

  ConnectionMap map;
  map.update(world);

  PathFinder path;
  bool found = path.findPath(car, world, map);
  SIAAssertMsg(found, "Can't found path.");

#ifdef ENABLE_VISUALIZATOR
  visualizator.beginPost();

  map.visualizationConnectionJoins(visualizator, 0x000077);
  map.visualizationConnectionPoints(visualizator, 0x000000);
  path.visualizationPath(visualizator, 0xFF0000);

  visualizator.endPost();
#endif
}

MyStrategy::MyStrategy() { }
