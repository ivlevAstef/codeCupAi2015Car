using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public class PhysicMoveCalculator {
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
    private Map map;

    public void setupEnvironment(Car car, Map map, Game game) {
      this.car = car;
      this.game = game;
      this.map = map;
      oneLineWidth = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;
    }

    public Move calculateMove(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> events = calculateEvents(idealPos, dirMove, idealDir);

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X));

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      //Vector dirMovePerpendicular = dirMove.Perpendicular() * Math.Sign(dirMove.Perpendicular().Dot(idealDir));

      if (events.ContainsKey(MovedEvent.PassageLine)) {
        Vector posSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Pos : null;

        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dirMove) > 1.25 * oneLineWidth) {
          result.IsBrake = car.Speed() > 8;
        }

        result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, idealAngle);
      }

      if (events.ContainsKey(MovedEvent.SideCrash)) {
        Vector pos = events[MovedEvent.SideCrash].Pos;
        Vector sideDir = (pos - new Vector(car.X, car.Y));
        Vector improvedSideDir = (pos + dirMove - new Vector(car.X, car.Y));
        double angle = improvedSideDir.Angle.AngleDeviation(sideDir.Angle);

        result.WheelTurn = car.WheelTurn + Math.Sign(angle) * game.CarWheelTurnChangePerTick;
      }

      return result;
    }

    private Dictionary<MovedEvent, PhysicCar> calculateEvents(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> result = new Dictionary<MovedEvent, PhysicCar>();

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setEnginePower(1.0);

      for (int i = 0; i < maxIterationCount; i++) {
        if (!result.ContainsKey(MovedEvent.PassageLine) && checkPassageLine(physicCar, idealPos, dirMove)) {
          result[MovedEvent.PassageLine] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.AngleReach) && checkAngleReach(physicCar, idealAngle)) {
          result[MovedEvent.AngleReach] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.SpeedReach) && checkAngleReach(physicCar, idealAngle) && checkSpeedReach(physicCar)) {
          result[MovedEvent.SpeedReach] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.PassageLine) && !result.ContainsKey(MovedEvent.SideCrash) && checkSideCrash(physicCar, dirMove, idealDir)) {
          result[MovedEvent.SideCrash] = new PhysicCar(physicCar);
        }

        if (result.ContainsKey(MovedEvent.PassageLine) && result.ContainsKey(MovedEvent.AngleReach) && result.ContainsKey(MovedEvent.SpeedReach)) {
          break;
        }

        //move
        PhysicCar zeroWheelTurn = PhysicCarForZeroWheelTurn(physicCar, game);
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

    private bool checkPassageLine(PhysicCar car, Vector idealPos, Vector dirMove) {
      return Math.Abs((car.Pos - idealPos).Dot(dirMove)) < oneLineWidth * 0.25;
    }

    private bool checkAngleReach(PhysicCar car, double idealAngle) {
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      
      return Math.Abs(angleDeviation) < game.CarRotationFrictionFactor &&
             Math.Abs(car.WheelTurn)  < game.CarWheelTurnChangePerTick;
    }

    private bool checkSpeedReach(PhysicCar car) {
      return (car.Speed.Normalize() - car.Dir).Length < 1.0e-3;
    }

    private bool checkSideCrash(PhysicCar car, Vector dirMove, Vector idealDir) {
      int xTile = (int)(car.Pos.X / game.TrackTileSize);
      int yTile = (int)(car.Pos.Y / game.TrackTileSize);
      PointInt[] dirs = map.reverseDirsByPos(xTile, yTile);
      if (null == dirs || 4 == dirs.Length/*undefined or empty*/) {
        return false;
      }

      double sideRadius = game.TrackTileMargin;
      double distanceToSide = game.TrackTileSize * 0.5 - game.TrackTileMargin;
      double minDistanceToSide = distanceToSide - game.CarWidth * 0.5;

      Vector center = new Vector(xTile + 0.5, yTile + 0.5) * game.TrackTileSize;
      Vector distanceFromCenter = car.Pos - center;

      //car near the center tile
      if (Math.Abs(distanceFromCenter.X) < minDistanceToSide && Math.Abs(distanceFromCenter.Y) < minDistanceToSide) {
        return false;
      }

      //edge
      if (Math.Abs(distanceFromCenter.X) > minDistanceToSide && Math.Abs(distanceFromCenter.Y) > minDistanceToSide) {
        Vector edge = center;
        edge.X += Math.Sign(distanceFromCenter.X) * game.TrackTileSize * 0.5;
        edge.Y += Math.Sign(distanceFromCenter.Y) * game.TrackTileSize * 0.5;

        if (intersectCarWithCircle(car.Pos, car.Dir, edge, sideRadius)) {
          return true;
        }
      }

      foreach(PointInt dirInt in dirs) {
        Vector dir = new Vector(dirInt.X, dirInt.Y);
       
        if (intersectCarWithSide(car.Pos, car.Dir, dir, center + dir * distanceToSide)) {
          return true;
        }
      }

      return false;
    }

    private static double WheelTurnForEndZeroWheelTurn(Car car, Game game, double finalAngle) {
      PhysicCar physicCar = new PhysicCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      double angleDeviation = finalAngle.AngleDeviation(physicCar.Angle);

      if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
        return 0;
      }

      return car.WheelTurn + game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation);
    }

    private static PhysicCar PhysicCarForZeroWheelTurn(PhysicCar car, Game game) {
      PhysicCar physicCar = new PhysicCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }

    ///Math
    private double DistanceToLine(Vector point, Vector p1, Vector p2) {
      Vector delta = p2 - p1;
      Vector distanceP1 = point - p1;
      double t = delta.Normalize().Dot(distanceP1);
      t = Math.Max(0, Math.Min(t, 1));

      Vector res = delta * t + p1;
      return (res - point).Length;
    } 

    private bool intersectCarWithCircle(Vector pos, Vector dir, Vector center, double radius) {
      Vector p1 = (dir * car.Width + dir.Perpendicular() * car.Height) * 0.5;
      Vector p2 = (dir * car.Width + dir.Perpendicular() * -car.Height) * 0.5;
      Vector p3 = (dir * -car.Width + dir.Perpendicular() * -car.Height) * 0.5;
      Vector p4 = (dir * -car.Width + dir.Perpendicular() * car.Height) * 0.5;

      double d = DistanceToLine(center, pos + p1, pos + p2);
      d = Math.Min(d, DistanceToLine(center, pos + p2, pos + p3));
      d = Math.Min(d, DistanceToLine(center, pos + p3, pos + p4));
      d = Math.Min(d, DistanceToLine(center, pos + p4, pos + p1));

      return d < radius;
    }

    private bool intersectCarWithSide(Vector pos, Vector dir, Vector sidePerp, Vector sidePos) {
      Vector p1 = (dir * car.Width + dir.Perpendicular() * car.Height) * 0.5;
      Vector p2 = (dir * car.Width + dir.Perpendicular() * -car.Height) * 0.5;
      Vector p3 = (dir * -car.Width + dir.Perpendicular() * -car.Height) * 0.5;
      Vector p4 = (dir * -car.Width + dir.Perpendicular() * car.Height) * 0.5;

      double p1Sign = (p1 + pos - sidePos).Dot(sidePerp);
      double p2Sign = (p2 + pos - sidePos).Dot(sidePerp);
      double p3Sign = (p3 + pos - sidePos).Dot(sidePerp);
      double p4Sign = (p4 + pos - sidePos).Dot(sidePerp);

      return !(p1Sign < 0 && p2Sign < 0 && p3Sign < 0 && p4Sign < 0);

    }
  }
}
