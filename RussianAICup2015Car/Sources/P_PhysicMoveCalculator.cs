using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class MovingCalculator {
    private const int maxIterationCount = 250;
    private double oneLineWidth;

    private enum MovedEvent {
      PassageLine,
      AngleReach,
      SpeedReach,
      SideCrash,
    };

    private Car car;
    private Game game;
    private World world;

    public void setupEnvironment(Car car, Game game, World world) {
      this.car = car;
      this.game = game;
      this.world = world;
      oneLineWidth = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;
    }

    public Move calculateMove(Vector idealPos, Vector dirMove, Vector idealDir, double lineCount = 1.25) {
      Dictionary<MovedEvent, Tuple<PCar, int, object>> events = calculateEvents(idealPos, dirMove, idealDir);

      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X), speedSign);

      if (events.ContainsKey(MovedEvent.PassageLine)) {
        Vector posSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Item1.Pos : null;

        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dirMove) > lineCount * oneLineWidth) {
          result.IsBrake = car.Speed() > 9;
        }

        int tickToPassageLine = events[MovedEvent.PassageLine].Item2;
        int tickToSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Item2 : maxIterationCount;
        bool magnifiedPos = tickToSpeedReach < tickToPassageLine;

        double idealAngle = magnifiedPos ? car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y) : Math.Atan2(idealDir.Y, idealDir.X);
        result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, idealAngle, speedSign);
      }

      if (events.ContainsKey(MovedEvent.SideCrash)) {
        PCar physicCar = events[MovedEvent.SideCrash].Item1;
        Vector pos = physicCar.Pos;
        Vector sideNormal = events[MovedEvent.SideCrash].Item3 as Vector;
        Logger.instance.Assert(null != sideNormal, "Can't get side normal");

        double angle = dirMove.Angle.AngleDeviation(sideNormal.Angle);
        if (Math.PI / 4 < Math.Abs(angle) && Math.Abs(angle) < 3 * Math.PI / 4) {
          result.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;

          bool isParallel = Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI/9);//20 degrees

          if (!isParallel && events[MovedEvent.SideCrash].Item2 < 20 && speedSign > 0) {
            result.IsBrake = car.Speed() > 8;
          }
        }
      }

      return result;
    }

    private Dictionary<MovedEvent, Tuple<PCar, int, object>> calculateEvents(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, Tuple<PCar, int, object>> result = new Dictionary<MovedEvent, Tuple<PCar, int, object>>();

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);

      for (int i = 0; i < maxIterationCount; i++) {
        double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
        double speedSign = Math.Sign(physicCar.Speed.Dot(physicCar.Dir));
        if (speedSign < 0) {
          idealAngle = Math.Atan2(dirMove.Y, dirMove.X);
        }

        bool passageLine = checkPassageLine(physicCar, idealPos, dirMove);
        bool angleReach = checkAngleReach(physicCar, idealAngle);
        bool speedReach = checkSpeedReach(physicCar);
        Vector sideCrashNormal = checkSideCrashRetNormal(physicCar, idealPos, new TileDir((int)Math.Round(dirMove.X), (int)Math.Round(dirMove.Y)));
        bool sideCrash = null != sideCrashNormal;

        if (!result.ContainsKey(MovedEvent.PassageLine) && passageLine) {
          result[MovedEvent.PassageLine] = new Tuple<PCar, int, object>(new PCar(physicCar), i, null);
        }
        if (!result.ContainsKey(MovedEvent.AngleReach) && angleReach) {
          result[MovedEvent.AngleReach] = new Tuple<PCar, int, object>(new PCar(physicCar), i, null);
        }
        if (!result.ContainsKey(MovedEvent.SpeedReach) && angleReach && speedReach) {
          result[MovedEvent.SpeedReach] = new Tuple<PCar, int, object>(new PCar(physicCar), i, null);
        }
        if (!result.ContainsKey(MovedEvent.PassageLine) && !result.ContainsKey(MovedEvent.SideCrash) && sideCrash) {
          result[MovedEvent.SideCrash] = new Tuple<PCar, int, object>(new PCar(physicCar), i, sideCrashNormal);
        }

        if (result.ContainsKey(MovedEvent.PassageLine) && result.ContainsKey(MovedEvent.SpeedReach)) {
          break;
        }

        //move

        intersectOilStick(physicCar);

        PCar zeroWheelTurn = PhysicCarForZeroWheelTurn(physicCar, game);
        double angleDeviation = idealAngle.AngleDeviation(zeroWheelTurn.Angle);

        if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
          physicCar.setWheelTurn(0);
        } else {
          physicCar.setWheelTurn(Math.Sign(angleDeviation));
        }

        physicCar.Iteration(1);
      }

      return result;
    }

    private bool checkPassageLine(PCar car, Vector idealPos, Vector dirMove) {
      return Math.Sign((car.Pos - idealPos).Dot(dirMove)) != Math.Sign((car.LastPos - idealPos).Dot(dirMove));
    }

    private bool checkAngleReach(PCar car, double idealAngle) {
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      
      return Math.Abs(angleDeviation) < game.CarRotationFrictionFactor &&
             Math.Abs(car.WheelTurn)  < game.CarWheelTurnChangePerTick;
    }

    private bool checkSpeedReach(PCar car) {
      return (car.Speed.Normalize() - car.Dir).Length < 1.0e-3;
    }

    private Vector checkSideCrashRetNormal(PCar car, Vector idealPos, TileDir dirMove) {
      TileDir carPos = new TileDir((int)(car.Pos.X/game.TrackTileSize),(int)(car.Pos.Y/game.TrackTileSize));
      TileDir endPos = new TileDir((int)(idealPos.X/game.TrackTileSize),(int)(idealPos.Y/game.TrackTileSize));
      bool isEndPos = (carPos * dirMove).Equals(endPos * dirMove);

      TileDir[] additionalDirs = isEndPos ? null : new TileDir[] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() };
      Vector normal = CollisionDetector.instance.IntersectCarWithMap(car.Pos, car.Dir, additionalDirs);
      if (null != normal && car.Speed.Dot(normal) < 0) {
        return normal;
      }
      return null;
    }

    private void intersectOilStick(PCar car) {
      foreach (OilSlick stick in world.OilSlicks) {
        if ((car.Pos - new Vector(stick.X, stick.Y)).Length < stick.Radius) {
          car.traveledOnOil(stick);
          return;
        }
      }
    }

    private static double WheelTurnForEndZeroWheelTurn(Car car, Game game, double finalAngle, double sign) {
      PCar physicCar = new PCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      double angleDeviation = finalAngle.AngleDeviation(physicCar.Angle);

      if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
        return 0;
      }

      return car.WheelTurn + sign * game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation);
    }

    private static PCar PhysicCarForZeroWheelTurn(PCar car, Game game) {
      PCar physicCar = new PCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }
  }
}
