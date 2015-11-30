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

    public static double GetDistanceTo(this Car car, Vector point, PointInt dir) {
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

    public static double GetAngleTo(this Car car, Vector point, PointInt dir, double powerTilt) {
      double x = car.X * Math.Abs(dir.X) + point.X * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + point.Y * Math.Abs(dir.X) + powerTilt * dir.Y;

      return car.GetAngleTo(x, y);
    }

    public static double GetAngleTo(this Car car, Vector point, Vector dir, double powerTilt) {
      double projectT = (car.X - point.X) * dir.X + (car.Y - point.Y) * dir.Y;
      double x = point.X + (projectT + powerTilt) * dir.X;
      double y = point.Y + (projectT + powerTilt) * dir.Y;

      return car.GetAngleTo(x, y);
    }

    public static double AngularFactor(this Car car, Game game) {
      double scalar = car.SpeedX * Math.Cos(car.Angle) + car.SpeedY * Math.Sin(car.Angle);
      return game.CarAngularSpeedFactor * scalar;
    }

    public static double WheelTurnForAngle(this Car car, double angleDt, Game game) {
      const double dt = 1;

      double angularFactor = Math.Abs(car.AngularFactor(game));

      if (angularFactor < 1.0e-5) {
        return 0;
      }

      double aFrictionMult = Math.Pow(1 - game.CarRotationFrictionFactor, dt);
      double angularFriction = game.CarRotationFrictionFactor * dt;

      double baseAngularSpeed = car.WheelTurn * angularFactor;
      double angularSpeed = car.AngularSpeed;

      angularSpeed -= Extensions.Limit(angularSpeed - baseAngularSpeed, angularFriction);

      double wheelTurn = car.WheelTurn;
      int n = (int)Math.Abs(Math.Floor(car.WheelTurn / game.CarWheelTurnChangePerTick));

      double angle = car.Angle;
      for (int i = 0; i < n; i++) {
        baseAngularSpeed = wheelTurn * angularFactor;

        angle += angularSpeed * dt;
        angularSpeed = baseAngularSpeed - (angularSpeed - baseAngularSpeed) * aFrictionMult;
        wheelTurn -= game.CarWheelTurnChangePerTick * Math.Sign(wheelTurn);
      }

      if (Math.Abs(angle - car.Angle + angleDt) < 1.0e-5) {
        return 0;
      }

      if (angle > car.Angle + angleDt) {
        return car.WheelTurn - game.CarWheelTurnChangePerTick;
      } else {
        return car.WheelTurn + game.CarWheelTurnChangePerTick;
      }
    }

    public static int TicksForAngle(this Car car, double angleDt, Game game) {
      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setWheelTurn(Math.Sign(angleDt));

      double finalAngle = car.Angle + angleDt;
      int ticks = 0;
      for (ticks = 0; ticks < 50; ticks++) {
        physicCar.Iteration(1);
        if (Math.Abs(physicCar.Angle - finalAngle) <= Math.PI/90) {
          break;
        }
      }

      return ticks;
      /*
      const double dt = 1;

      double angularFactor = Math.Abs(car.AngularFactor(game));

      if (angularFactor < 1.0e-5) {
        return 0;
      }

      double aFrictionMult = Math.Pow(1 - game.CarRotationFrictionFactor, dt);
      double angularFriction = game.CarRotationFrictionFactor * dt;

      double baseAngularSpeed = car.WheelTurn * angularFactor;
      double angularSpeed = car.AngularSpeed;

      angularSpeed -= Extensions.Limit(angularSpeed - baseAngularSpeed, angularFriction);

      double wheelTurn = car.WheelTurn;

      double angle = car.Angle;
      int ticks = 0;

      int beginSign = Math.Sign(angle - (car.Angle + angleDt));
      while (0 != beginSign && beginSign == Math.Sign(angle - (car.Angle + angleDt))) {
        baseAngularSpeed = wheelTurn * angularFactor;
        angle += angularSpeed * dt;
        angularSpeed = baseAngularSpeed - (angularSpeed - baseAngularSpeed) * aFrictionMult;

        if (angle > car.Angle + angleDt) {
          wheelTurn -= game.CarWheelTurnChangePerTick;
        } else {
          wheelTurn += game.CarWheelTurnChangePerTick;
        }

        wheelTurn = Extensions.Limit(wheelTurn, 1);

        ticks++;
      }


      return ticks - (int)Math.Abs(wheelTurn / game.CarWheelTurnChangePerTick);*/
    }

    public static double AngleForZeroWheelTurn(this Car car, Game game) {
      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setWheelTurn(0);

      for (int i = 0; i < 25; i++) {
        physicCar.Iteration(2);
        if (Math.Abs(physicCar.WheelTurn) <= 1.0e-3) {
          break;
        }
      }

      return physicCar.Angle;
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
