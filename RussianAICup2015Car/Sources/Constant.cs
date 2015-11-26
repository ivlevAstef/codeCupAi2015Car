using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

namespace RussianAICup2015Car.Sources {
  class Constant {
    //0 < angle < 1.0 for 0 to 180 degrees
    public static double MaxTurnSpeed(Car car, double angle = 0.5) {
      return 20 - (angle * 12) - Math.Min(5, car.RemainingNitroTicks * 0.2);
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
