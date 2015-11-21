using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public static class Extensions {
    public static T[] RemoveFirst<T>(this T[] source) {
      T[] destination = new T[source.Length - 1];
      
      Array.Copy(source,  1, destination, 0, source.Length - 1);

      return destination;
    }

    public static double GetDistanceTo(this Car car, PointDouble point, PointInt dir) {
      double x = car.X * Math.Abs(dir.Y) + point.X * Math.Abs(dir.X);
      double y = car.Y * Math.Abs(dir.X) + point.Y * Math.Abs(dir.Y);
      return car.GetDistanceTo(x, y);
    }

    public static double Speed(this Car car) {
      return Math.Sqrt(car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY);
    }

    public static double SpeedN(this Car car, PointInt dir) {
      return Math.Sqrt(car.SpeedX * car.SpeedX * Math.Abs(dir.X) + car.SpeedY * car.SpeedY * Math.Abs(dir.Y));
    }
    public static double SpeedN2(this Car car, PointInt dir) {
      return car.SpeedX * car.SpeedX * Math.Sign(car.SpeedX) * dir.X + car.SpeedY * car.SpeedY * Math.Sign(car.SpeedY) * dir.Y;
    }

    public static double Speed2(this Car car) {
      return car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY;
    }

    public static double GetAngleTo(this Car car, PointDouble point, PointInt dir, double powerTilt) {
      double x = car.X * Math.Abs(dir.X) + point.X * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + point.Y * Math.Abs(dir.X) + powerTilt * dir.Y;

      return car.GetAngleTo(x, y);
    }

    public static double GetAngleTo(this Car car, PointDouble point, PointDouble dir, double powerTilt) {
      double projectT = (car.X - point.X) * dir.X + (car.Y - point.Y) * dir.Y;
      double x = point.X + (projectT + powerTilt) * dir.X;
      double y = point.Y + (projectT + powerTilt) * dir.Y;

      return car.GetAngleTo(x, y);
    }

    public static double AngularFactor(this Car car, Game game) {
      double scalar = car.SpeedX * Math.Cos(car.Angle) + car.SpeedY * Math.Sin(car.Angle);
      return game.CarAngularSpeedFactor * scalar;
    }

    public static double WheelTurnForAngle(this Car car, double angle, Game game) {
      double angularFactor = car.AngularFactor(game);
      angularFactor = Math.Abs(angularFactor);
      double angularChanged = car.WheelTurn * angularFactor;
      double angularSpeed = car.AngularSpeed;

      if (Math.Abs(angularFactor) < 1.0e-3) {
        return 0;
      }

      double sign = Math.Sign(angle);

      double n = 0;
      double iterAngle = -angularChanged;

      while (sign * iterAngle < sign * angle) {
        iterAngle += angularSpeed;
        angularSpeed += sign * game.CarWheelTurnChangePerTick * angularFactor;
        n++;
      }

      if (0 == n) {
        return 0;
      }

      return (angle / n) / angularFactor;
    }

    public static double GetAbsoluteAngleTo(this Car car, double x, double y, double dirX, double dirY) {
      double absoluteAngleTo = Math.Atan2(y - car.Y, x - car.X);
      absoluteAngleTo -= Math.Atan2(dirY, dirX);

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
