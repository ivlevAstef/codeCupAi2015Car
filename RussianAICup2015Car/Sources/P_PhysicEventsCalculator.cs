using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public static class PhysicEventsCalculator {
    public delegate bool CheckCalculateEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick);

    private static readonly int maxSaveIterationCount = 500;

    private static Game game;
    private static World world;

    public static void setupEnvironment(Game lGame, World lWorld) {
      game = lGame;
      world = lWorld;
    }

    public static void calculateEvents(PCar physicCar, IPhysicMoveFunction moveFunc, HashSet<IPhysicEvent> pEvents, CheckCalculateEnd checkEnd) {
      for (int tick = 0; tick < maxSaveIterationCount; tick++) {
        foreach (IPhysicEvent pEvent in pEvents) {
          if (!pEvent.IsCome && pEvent.Check(physicCar)) {
            pEvent.SetCome(physicCar, tick, pEvent.InfoForCheck);
          }
        }

        if (checkEnd(physicCar, pEvents, tick)) {
          return;
        }

        moveFunc.Iteration(physicCar, 1);
      }

      Logger.instance.Assert(false, "Please check delegate: CheckCalculateEnd.");
    }
  }
}
