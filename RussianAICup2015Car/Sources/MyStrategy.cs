using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Path path = new Path();

    private Dictionary<ActionType, A_IAction> actions = new Dictionary<ActionType, A_IAction> {
      { ActionType.InitialFreeze, new A_InitialFreeze()},

      { ActionType.Forward, new A_M_ForwardAction()},
      { ActionType.Backward, new A_M_BackwardAction()},
      { ActionType.PreTurn, new A_M_PreTurnAction()},
      { ActionType.Turn, new A_M_TurnAction()},
      //{ ActionType.Snake, new A_M_SnakeAction()},
      //{ ActionType.Around, new A_M_AroundAction()},
      { ActionType.StuckOut, new A_M_StuckOutAction()},

      { ActionType.MoveToBonus, new A_MoveToBonusAction()},
      { ActionType.Overtake, new A_OvertakeAction()},
      //{ ActionType.AvoidSideHit, new ()},
      //{ ActionType.AvoidWindShieldHit, new ()},

      { ActionType.Shooting, new A_ShootingAction()},
      { ActionType.OilSpill, new A_OilSpillAction()},
      { ActionType.UseNitro, new A_UseNitroAction()},
    };

    public void Move(Car self, World world, Game game, Move move) {
      path.update(self, world, game);

      foreach (A_IAction action in actions.Values) {
        action.setupEnvironment(self, world, game, path);
      }

      HashSet<ActionType> validActions = new HashSet<ActionType>();

      foreach (KeyValuePair<ActionType, A_IAction> actionInfo in actions) {
        if (actionInfo.Value.valid()) {
          validActions.Add(actionInfo.Key);
        }
      }

      move.WheelTurn = 0;
      move.EnginePower = 0;
      foreach (ActionType actionType in validActions) {
        A_IAction action = actions[actionType];

        HashSet<ActionType> blocked = new HashSet<ActionType>();
        foreach (ActionType subActionType in validActions) {
          if (action.blockers.Contains(subActionType)) {
            blocked.Add(subActionType);
          }
        }

        if (blocked.Count > 0) {
          action.blockedBy(blocked);
          continue;
        }

        action.execute(move);
      }


      /*path.update(self, world, game);

      outStuck.update(self);

      PathCell[] wayCells = path.wayCells;

      PointInt dirSelfToNext = dirFor(wayCells[0].Pos, wayCells[1].Pos);
      PointInt dirNextToNextNext = dirFor(wayCells[1].Pos, wayCells[2].Pos);
      bool oneDir = dirSelfToNext.X == dirNextToNextNext.X && dirSelfToNext.Y == dirNextToNextNext.Y;

      if (dirNextToNextNext.X == -dirSelfToNext.X || dirNextToNextNext.Y == -dirSelfToNext.Y) {
        dirNextToNextNext = dirSelfToNext;
      }

      if (outStuck.needRunOutStuck()) {
        outStuck.updateUseOutStuck(self, dirSelfToNext, game, move);
      } else {
        double idealAngle = self.GetAngleTo(self.X + dirNextToNextNext.X, self.Y + dirNextToNextNext.Y);
        double nIdealAngle = Math.Abs(Math.Sin(idealAngle));
        nIdealAngle = (idealAngle < Math.PI/2) ? nIdealAngle : (2 - nIdealAngle);
        double speed = hypot(self.SpeedX, self.SpeedY);
        double nSpeed = speed * nIdealAngle;

        double procent = procentToWay(wayCells[1].Pos, dirSelfToNext);

        double procentToSpeed = Math.Min(2.0f, nSpeed / (game.TrackTileSize / 80));
        procent = procent * ((4.0 - procentToSpeed * procentToSpeed)/2.5);

        procent = Math.Min(1.0, Math.Max(0.0, procent));
        double xMoved = dirSelfToNext.X * procent + dirNextToNextNext.X * (1.0 - procent);
        double yMoved = dirSelfToNext.Y * procent + dirNextToNextNext.Y * (1.0 - procent);

        double needAngle = self.GetAngleTo(self.X + xMoved, self.Y + yMoved);
        move.EnginePower = 1.0f - Math.Min(0.2f, Math.Abs(needAngle / (Math.PI * 0.5)));

        if (!isStraight() && speed > game.TrackTileSize / 40) {
          needAngle *= 0.4;
          move.IsBrake = true;
        }

        double bonusMagnited = magniteToBonus(dirSelfToNext);
        double centerMagnited = magniteToCenter(dirSelfToNext);

        if (isStraight() && Math.Abs(bonusMagnited) > 1.0e-3) {
          needAngle += bonusMagnited;
        } else {
          needAngle += centerMagnited * 0.5;
        }

        needAngle *= 25;
        //needAngle -= 15 * self.AngularSpeed;
        move.WheelTurn = (needAngle / Math.PI);

        if (isStraight() && Math.Abs(needAngle) < 0.1) {
          move.IsUseNitro = true;
        }
        
      }

      if (null != useOilOn && useOilOn.Equals(wayCells[0].Pos)) {
        move.IsSpillOil = true;
        useOilOn = null;
      }

      if (!oneDir) {
        useOilOn = wayCells[1].Pos;
       }

      if (enemyAhead()) {
        move.IsThrowProjectile = true;
      }
       */
    }

    /*private bool enemyAhead() {
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
        { BonusType.AmmoCrate , Math.Min(10, 70 - 10 * self.ProjectileCount) },
        { BonusType.NitroBoost , Math.Min(10, 80 - 10 * self.NitroChargeCount) },
        { BonusType.OilCanister , Math.Min(10, 50 - 10 * self.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , (int)(150 * (1.0 - self.Durability)) }
      };

      double speed = hypot(self.SpeedX, self.SpeedY);
      double maxAngle = (Math.PI / 6) / Math.Min(0.75, speed / (game.TrackTileSize/80));

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = self.GetDistanceTo(bonus);
        if (distance > game.TrackTileSize*2.0) {
          continue;
        }

        double angle = self.GetAngleTo(bonus);
        if (Math.Abs(angle) > maxAngle) {
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
      for (int index = 0; index < path.wayCells.Length; index++) {
        if (null == path.wayCells[index].DirOut ||
          path.wayCells[index].DirOut.X != path.wayCells[index].DirIn.X ||
          path.wayCells[index].DirOut.Y != path.wayCells[index].DirIn.Y) {
          return false;
        }
      }

      return true;
    }

    private PointInt convert(double x, double y) {
      return new PointInt((int)(x / game.TrackTileSize), (int)(y / game.TrackTileSize));
    }

    private PointDouble convert(PointInt point) {
      Logger.instance.Assert(null != game, "zero game");

      double nextWaypointX = (point.X + 0.5) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private double pixelsToWay(PointInt way, PointInt dir) {
      Logger.instance.Assert(null != way, "zero way");
      Logger.instance.Assert(null != game, "zero game");

      PointDouble wayPos = convert(way);
      wayPos.X = self.X * Math.Abs(dir.Y) + wayPos.X * Math.Abs(dir.X);
      wayPos.Y = self.Y * Math.Abs(dir.X) + wayPos.Y * Math.Abs(dir.Y);
      return self.GetDistanceTo(wayPos.X, wayPos.Y);
    }

    private double procentToWay(PointInt way, PointInt dir) {
      Logger.instance.Assert(null != way, "zero way");
      Logger.instance.Assert(null != game, "zero game");

      return pixelsToWay(way, dir) / game.TrackTileSize;
    }

    private PointInt dirFor(PointInt way, PointInt nextWay) {
      PointInt dir = new PointInt(nextWay.X - way.X, nextWay.Y - way.Y);

      Logger.instance.Assert(dir.Equals(Path.DirLeft) ||
                 dir.Equals(Path.DirRight) ||
                 dir.Equals(Path.DirUp) ||
                 dir.Equals(Path.DirDown), "incorrect dir");

      return dir;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }
    */
  }
}