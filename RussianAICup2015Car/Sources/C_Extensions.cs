using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Common {
  public static class Extensions {
    public static Vector CenterTile(this Car car, Game game) {
      double x = Math.Floor(car.X / game.TrackTileSize);
      double y = Math.Floor(car.Y / game.TrackTileSize);

      return new Vector(x + 0.5, y + 0.5) * game.TrackTileSize;
    }

    public static double AngleNormalize(this double angle) {
      while (angle > Math.PI) {
        angle -= 2.0D * Math.PI;
      }

      while (angle < -Math.PI) {
        angle += 2.0D * Math.PI;
      }

      return angle;
    }
    public static double AngleDeviation(this double angle1, double angle2) {
      return (angle1 - angle2).AngleNormalize();
    }

    public static double Limit(double v, double limit) {
      return Math.Max(-limit, Math.Min(v, limit));
    }


    public static double Speed(this Car car) {
      return Math.Sqrt(car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY);
    }

    public static double Speed2(this Car car) {
      return car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY;
    }

    public static double AngleForZeroWheelTurn(this Car car, Game game) {
      Physic.PCar physicCar = new Physic.PCar(car, game);
      physicCar.setWheelTurn(0);

      for (int i = 0; i < 25; i++) {
        physicCar.Iteration(2);
        if (Math.Abs(physicCar.WheelTurn) <= 1.0e-3) {
          break;
        }
      }

      return physicCar.Angle;
    }

    public static double GetAbsoluteAngleTo(this Car car, double x, double y) {
      double absoluteAngleTo = Math.Atan2(y - car.Y, x - car.X);

      while (absoluteAngleTo > Math.PI) {
        absoluteAngleTo -= 2.0D * Math.PI;
      }

      while (absoluteAngleTo < -Math.PI) {
        absoluteAngleTo += 2.0D * Math.PI;
      }

      return absoluteAngleTo;
    }
  }
}
