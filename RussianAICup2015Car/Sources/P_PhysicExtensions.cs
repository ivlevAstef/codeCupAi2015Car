using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

using RussianAICup2015Car.Sources.Common;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources.Physic {
  public static class PhysicExtensions {
    private static Game game = null;

    public static void setupEnvironment(Game lGame) {
      game = lGame;
    }

    public static PCar GetZeroWheelTurnCar(this PCar car) {
      PCar physicCar = new PCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }


    public static bool HasCollision(this List<CollisionInfo> collisions) {
      foreach (CollisionInfo info in collisions) {
        if (info.CollisionDeletected) {
          return true;
        }
      }
      return false;
    }

    public static Vector AverageNormalObj1(this List<CollisionInfo> collisions) {
      if (!collisions.HasCollision()) {
        return null;
      }

      Vector normal = new Vector(0, 0);
      foreach (CollisionInfo info in collisions) {
        if (info.CollisionDeletected && !info.Inside) {
          normal = normal + info.NormalObj1;
        }
      }

      return normal.Normalize();
    }

    public static Vector AverageNormalObj2(this List<CollisionInfo> collisions) {
      if (!collisions.HasCollision()) {
        return null;
      }

      Vector normal = new Vector(0, 0);
      foreach (CollisionInfo info in collisions) {
        if (info.CollisionDeletected && !info.Inside) {
          normal = normal + info.NormalObj2;
        }
      }

      return normal.Normalize();
    }
  }
}
