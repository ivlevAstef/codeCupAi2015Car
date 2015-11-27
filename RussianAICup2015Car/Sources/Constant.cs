using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

namespace RussianAICup2015Car.Sources {
  class Constant {
    public static double MaxTurnSpeed(Car car, double angle = 0.5) {
      double enginePower = 4 * Math.Max(0.0, Math.Abs(car.EnginePower) - 1.0);
      double oilOnWheel = (car.RemainingOiledTicks > 0) ? 7 : 0;

      return 20 - Math.Min(15, (angle * 12) + enginePower + oilOnWheel);
    }

    public static bool isExceedMaxTurnSpeed(Car car, Vector dir, double angle = 0.5) {
      dir = dir.Normalize();
      double len = dir.Dot(new Vector(car.SpeedX, car.SpeedY));

      return Math.Abs(len) > MaxTurnSpeed(car, angle);
    }

    public static bool isExceedMaxTurnSpeed(Car car, PointInt dir, double angle = 0.5) {
      return isExceedMaxTurnSpeed(car, new Vector(dir.X, dir.Y), angle);
    }
  }
}
