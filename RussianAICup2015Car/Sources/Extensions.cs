﻿using System;
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

    public static double GetAbsoluteAngleTo(this Car car, PointDouble point, PointInt dir, double powerTilt) {
      double x = car.X * Math.Abs(dir.X) + point.X * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + point.Y * Math.Abs(dir.X) + powerTilt * dir.Y;

      return car.GetAbsoluteAngleTo(x, y, dir.X, dir.Y);
    }

    public static double WheelTurnFactor(this Car car, Game game) {
      double scalar = 1;
      double speed = car.Speed();

      if (speed < 1) {
         scalar = (car.SpeedX / speed) * Math.Cos(car.Angle) + (car.SpeedY / speed) * Math.Sin(car.Angle);
      } else {
         scalar = car.SpeedX * Math.Cos(car.Angle) + car.SpeedY * Math.Sin(car.Angle);
      }

      return 1.0 / (game.CarAngularSpeedFactor * scalar);
    }

    public static double WheelTurnForAngle(this Car car, double angle, Game game) {
      return angle * car.WheelTurnFactor(game);
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