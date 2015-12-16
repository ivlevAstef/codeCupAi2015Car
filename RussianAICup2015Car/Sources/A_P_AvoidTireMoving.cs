using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;
using RussianAICup2015Car.Sources.Actions;

namespace RussianAICup2015Car.Sources.Actions {
  class AvoidTireMoving : AdditionalPoints {
    private const int MaxCheckTicks = 50;

    public override List<Vector> GetPoints() {
      List<Tuple<PCar, PTire>> hitInforamtions = hitInformation();

      if (null == hitInforamtions || 0 == hitInforamtions.Count) {
        return null;
      }

      List<Vector> result = new List<Vector>();
      foreach (Tuple<PCar, PTire> hitInfo in hitInforamtions) {
        PCar self = hitInfo.Item1;
        PTire tire = hitInfo.Item2;

        Vector selfPos = new Vector(self.Car.X, self.Car.Y);
        Vector tirePos = new Vector(tire.Tire.X, tire.Tire.Y);

        double angle = self.Car.Angle.AngleDeviation((tirePos - selfPos).Angle);
        double sign = Math.Sign(angle);

        Vector center = selfPos + (tire.Pos - selfPos) * 0.5;
        Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
        Vector endPos = center + dir.PerpendicularRight() * sign * car.Height;

        result.Add(endPos);
      }

      return result;
    }

    private List<Tuple<PCar, PTire>> hitInformation() {
      List<Tuple<PCar, PTire>> result = new List<Tuple<PCar, PTire>>();

      foreach (Projectile iter in world.Projectiles) {
        if (iter.Type == ProjectileType.Tire) {
          PCar self = new PCar(car, game);
          PTire tire = new PTire(iter, game);
          if (checkHit(self, tire)) {
            result.Add(new Tuple<PCar, PTire>(self, tire));
          }
        }
      }

      return result;
    }

    private bool checkHit(PCar self, PTire tire) {
      double selfRadius = 0.5 * Math.Sqrt(game.CarWidth * game.CarWidth + game.CarHeight * game.CarHeight);
      double checkRadius = selfRadius + game.TireRadius;

      self.setEnginePower(1);
      MoveToAngleFunction mover = new MoveToAngleFunction(new Vector(path[0].DirOut.X, path[0].DirOut.Y).Angle);

      for (int i = 0; i < MaxCheckTicks; i++) {
        Vector lastDistance = tire.LastPos - self.LastPos;
        Vector distance = tire.Pos - self.Pos;

        if (lastDistance.Length > checkRadius && distance.Length < checkRadius) {
          return true;
        }

        tire.Iteration(1);
        mover.Iteration(self, 1);
      }

      return false;
    }
  }
}
