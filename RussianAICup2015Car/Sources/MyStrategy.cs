using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Logger log = new Logger();
    private Car self = null;
    private World world = null;
    private Game game = null;

    private Path path = null;
    private OutStuck outStuck = new OutStuck();

    public MyStrategy() {
    }

    public void Move(Car self, World world, Game game, Move move) {
      this.self = self;
      this.world = world;
      this.game = game;

      if (null == path) {
        path = new Path(world, log);        
      }

      if (world.Tick < game.InitialFreezeDurationTicks) {
        move.EnginePower = 1.0;
        return;
      }

      outStuck.update(self);

      PointInt[] wayPoints = path.wayPoints(self, world, game, 2);
      log.Assert(3 == wayPoints.Length, "incorrect calculate way points.");

      PointInt dirSelfToNext = dirFor(wayPoints[0], wayPoints[1]);
      PointInt dirNextToNextNext = dirFor(wayPoints[1], wayPoints[2]);
      bool oneDir = dirSelfToNext.X == dirNextToNextNext.X && dirSelfToNext.Y == dirNextToNextNext.Y;

      if (outStuck.needRunOutStuck()) {
        outStuck.updateUseOutStuck(self, dirSelfToNext, game, move);
      } else {
        double idealAngle = self.GetAngleTo(self.X + dirNextToNextNext.X, self.Y + dirNextToNextNext.Y);
        double speed = hypot(self.SpeedX, self.SpeedY);
        double nSpeed = speed * Math.Abs(idealAngle / (Math.PI * 0.5));

        double procent = procentToWay(wayPoints[1]);
        if (!oneDir && procent < 0.55) {
          move.IsSpillOil = true;
        }

        double procentToSpeed = Math.Min(2.0f, nSpeed / (game.TrackTileSize / 140));
        procent = Math.Sin(Math.PI * 0.5 * procent) * (4.0 - procentToSpeed * procentToSpeed);

        procent = Math.Min(1.0, Math.Max(0.0, procent));
        double xMoved = dirSelfToNext.X * procent + dirNextToNextNext.X * (1.0 - procent);
        double yMoved = dirSelfToNext.Y * procent + dirNextToNextNext.Y * (1.0 - procent);

        double needAngle = self.GetAngleTo(self.X + xMoved, self.Y + yMoved);
        move.EnginePower = 1.0f - Math.Min(0.2f, Math.Abs(needAngle / (Math.PI * 0.5)));

        if (nSpeed > game.TrackTileSize / 40) {
          move.IsBrake = true;
        }

        double bonusMagnited = magniteToBonus(dirSelfToNext);
        double centerMagnited = magniteToCenter(dirSelfToNext);

        if (oneDir && Math.Abs(bonusMagnited) > 1.0e-3) {
          needAngle += bonusMagnited;
        } else {
          needAngle += centerMagnited* 0.1;
        }

        needAngle *= 25;
        needAngle -= 15 * self.AngularSpeed;
        move.WheelTurn = (needAngle / Math.PI);

        if (isStraight() && Math.Abs(needAngle) < 0.1) {
          move.IsUseNitro = true;
        }

      }

      if (enemyAhead()) {
        move.IsThrowProjectile = true;
      }
    }

    private bool enemyAhead() {
      foreach (Car car in world.Cars) {
        if (car.IsTeammate || car.IsFinishedTrack || 0 == car.Durability) {
          continue;
        }

        double distance = self.GetDistanceTo(car);
        if (distance > game.TrackTileSize) {
          continue;
        }

        double angle = self.GetAngleTo(car);
        if (Math.Abs(angle) < Math.PI / 18) {
          return true;
        }
      }

      return false;
    }


    private double magniteToPoint(double x, double y, PointInt dir) {
      return (dir.Y * (self.X - x) - dir.X * (self.Y - y)) / game.TrackTileSize;
    }

    private double magniteToCenter(PointInt dir) {
      double moveX = (Math.Floor(self.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double moveY = (Math.Floor(self.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      return magniteToPoint(moveX, moveY, dir);
    }

    private double magniteToBonus(PointInt dir) {
      Dictionary<BonusType, int> priority = new Dictionary<BonusType,int> {
        { BonusType.AmmoCrate , Math.Min(10, 50 - 10 * self.ProjectileCount) },
        { BonusType.NitroBoost , Math.Min(10, 80 - 10 * self.NitroChargeCount) },
        { BonusType.OilCanister , Math.Min(10, 50 - 10 * self.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , (int)(150 * (1.0 - self.Durability)) }
      };

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = self.GetDistanceTo(bonus);
        if (distance > game.TrackTileSize*2.0) {
          continue;
        }

        double angle = self.GetAngleTo(bonus);
        if (Math.Abs(angle) > Math.PI / 9) {
          continue;
        }

        PointInt selfTile = convert(self.X, self.Y);
        PointInt bonusTile = convert(bonus.X, bonus.Y);
        if (!selfTile.Equals(bonusTile) && !selfTile.Add(dir).Equals(bonusTile)) {
          continue;
        }


        if (null == priorityBonus || priority[priorityBonus.Type] < priority[bonus.Type]) {
          priorityBonus = bonus;
        }
      }

      if (null == priorityBonus) {
        return 0;
      }

      return magniteToPoint(priorityBonus.X, priorityBonus.Y, dir);
    }

    private bool isStraight() {
      PointInt[] wayPoints = path.wayPoints(self, world, game, 4);
      log.Assert(3 <= wayPoints.Length, "incorrect calculate way points.");

      int moveX = 0;
      int moveY = 0;
      for (int index = 1; index < wayPoints.Length; index++) {
        moveX += Math.Abs(wayPoints[index].X - wayPoints[index - 1].X);
        moveY += Math.Abs(wayPoints[index].Y - wayPoints[index - 1].Y);
      }

      return (0 == moveX) || (0 == moveY);
    }

    private PointInt convert(double x, double y) {
      return new PointInt((int)(x / game.TrackTileSize), (int)(y / game.TrackTileSize));
    }

    private PointDouble convert(PointInt point) {
      log.Assert(null != game, "zero game");

      double nextWaypointX = (point.X + 0.5D) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5D) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private double pixelsToWay(PointInt way) {
      log.Assert(null != way, "zero way");
      log.Assert(null != game, "zero game");

      PointDouble wayPos = convert(way);
      return self.GetDistanceTo(wayPos.X, wayPos.Y);
    }

    private double procentToWay(PointInt way) {
      log.Assert(null != way, "zero way");
      log.Assert(null != game, "zero game");

      return pixelsToWay(way) / game.TrackTileSize;
    }

    private PointInt dirFor(PointInt way, PointInt nextWay) {
      PointInt dir = new PointInt(nextWay.X - way.X, nextWay.Y - way.Y);

      log.Assert(dir.Equals(Path.DirLeft) ||
                 dir.Equals(Path.DirRight) ||
                 dir.Equals(Path.DirUp) ||
                 dir.Equals(Path.DirDown), "incorrect dir");

      return dir;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}