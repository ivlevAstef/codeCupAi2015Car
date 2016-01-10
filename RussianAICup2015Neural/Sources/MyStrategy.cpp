#include "MyStrategy.h"

#include "Common/SIALogger.h"
#include "Common/Constants.h"

#include "Map/ConnectionMap.h"
#include "Map/PathFinder.h"

#include "Neural/NeuralTraining.h"
#include "Neural/NeuralNet.h"
#include "Neural/NeuralIn.h"
#include "Neural/NeuralCalculator.h"

using namespace model;
using namespace std;
using namespace Map;
using namespace Neural;

void MyStrategy::move(const Car& car, const World& world, const Game& game, Move& move) {
  volatile Constants constants = Constants(game);
  ConnectionMap::reMemory();

  ///create map
  ConnectionMap map;
  map.update(world);
  map.updateWeightForCar(car, world);

  ///create paths
  std::vector<PathFinder> paths;
  paths.push_back(PathFinder());
  paths[0].findPath(car, world, map, constants.pathSelfDepth);

  for (const Car& otherCar : world.getCars()) {
    if (otherCar.getId() != car.getId()) {
      paths.push_back(PathFinder());
      paths[paths.size() - 1].findPath(otherCar, world, map, constants.pathOtherCarDepth);
    }
  }


  ///neural
#ifdef ENABLE_NET_LEARNING
  const NeuralNet& neuralNet = NeuralTraining::instance().getNet();
#else
  NeuralNet neuralNet;
#endif

  NeuralIn neuralIn(world, paths);

  NeuralCalculator moveCalculator(neuralNet);
  NeuralOut neuralOut = moveCalculator.calculate(neuralIn);
  
  neuralOut.fillMove(move);

#ifdef ENABLE_NET_LEARNING
  NeuralTraining::instance().update(neuralIn, neuralOut);
#endif

  ///simple move
  /*move.setSpillOil(true);

  if (world.getTick() > game.getInitialFreezeDurationTicks()) {
    move.setUseNitro(true);
  }

  double angleToWaypoint = car.getAngleTo(paths[0].getPath()[1].x, paths[0].getPath()[1].y);
  move.setWheelTurn(angleToWaypoint * 32.0 / 3.14);
  move.setEnginePower(0.75);

  double speedModule = SIA::Vector(car.getSpeedX(), car.getSpeedY()).length();
  if (speedModule * speedModule * abs(angleToWaypoint) > 10 * 3.14) {
    move.setBrake(true);
  }*/

  ///visualization
#ifdef ENABLE_VISUALIZATOR
  Visualizator::setWindowCenter(car.getX(), car.getY(), world.getWidth() * game.getTrackTileSize(), world.getHeight() * game.getTrackTileSize());
  visualizator.beginPost();

  map.visualizationConnectionJoins(visualizator, 0x000077);
  map.visualizationConnectionPoints(visualizator, 0x000000);

  for (size_t i = 1; i < paths.size(); i++) {
    paths[i].visualizationPath(visualizator, 0x770077);
  }

  paths[0].visualizationPath(visualizator, 0xFF0000);
  paths[0].visualizationPointWeight(visualizator, 0xFF00FF, map);

  visualizator.endPost();
#endif
}

MyStrategy::MyStrategy() { }
