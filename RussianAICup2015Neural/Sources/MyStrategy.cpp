#include "MyStrategy.h"

#define PI 3.14159265358979323846
#define _USE_MATH_DEFINES

#include "Visualizator/Visualizator.h"
#include "Common/Constants.h"
#include "Map/ConnectionMap.h"

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

  visualizator.beginPost();
  map.visualizationConnectionPoints(visualizator, 0x000000);
  visualizator.endPost();
}

MyStrategy::MyStrategy() { }
