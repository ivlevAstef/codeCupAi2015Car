using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_AroundAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      PointInt posIn = path.WayCells[1].Pos;
      PointInt posOut = path.WayCells[2].Pos;

      PointInt dirIn = path.WayCells[1].DirIn;
      PointInt dirOut = path.WayCells[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.Negative()) && !posIn.Equals(posOut);
    }

    public override void execute(Move move) {
      PointInt dirMove = path.FirstWayCell.DirOut;
      PointInt dirEnd = path.ShortWayCells[0].DirOut;

      PointDouble wayEnd = GetWayEnd(path.FirstWayCell.Pos, dirMove);

      PointDouble endPoint1 = GetWaySideEnd(path.FirstWayCell.Pos, dirMove, dirEnd.Negative());
      PointDouble endPoint2 = GetWaySideEnd(path.FirstWayCell.Pos, dirMove, dirEnd);

      double endAngle1 = car.GetAngleTo(endPoint1.X, endPoint1.Y);
      double endAngle2 = car.GetAngleTo(endPoint2.X, endPoint2.Y);

      double distanceToEnd = car.GetDistanceTo(wayEnd, dirMove);
      double procentToEnd = distanceToEnd / game.TrackTileSize;

      double angleMult = (procentToEnd < 0.4) ? 0 : procentToEnd;
      double angle = angleMult * endAngle1 + (1.0 - angleMult) * endAngle2;
      move.WheelTurn = car.WheelTurnForAngle(angle, game);

      double sign = GetSign(dirMove, dirEnd);
      double normalAngle = car.GetAngleTo(car.X + dirMove.X + dirEnd.X, car.Y + dirMove.Y + dirEnd.Y);

      if (distanceToEnd < (game.CarWidth * 0.5 + 5 * car.SpeedN(dirMove)) && sign * normalAngle > 0) {
        angle = car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y);
        normalAngle = angle;
      }


      double diffAngle = sign * (normalAngle - angle);
      if (diffAngle > 0 && car.Speed() * diffAngle > 2.2 / (1 - procentToEnd)) {
        move.IsBrake = true;
      }

      double speedWithEnginePower = car.Speed() + car.EnginePower * car.EnginePower - 1;
      if (speedWithEnginePower > 15) {
        move.IsBrake = true;
      }

      move.EnginePower = 1.0;
    }

     public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill
      };
     }    

    private double GetSign(PointInt dir1, PointInt dir2) {
      double changedSign = Math.Abs(dir1.X + dir1.Y + dir2.X + dir2.Y) - 1;
      if (0 == dir2.X) {
        return changedSign;
      }
      return -changedSign;
    }

    private double AngleToWheelTurn(double angle) {
      double scalar = car.SpeedX * Math.Sin(car.Angle) + car.SpeedY * Math.Cos(car.Angle);

      return angle / (game.CarAngularSpeedFactor * Math.Abs(scalar));
    }

    private PointDouble GetWaySideEnd(PointInt pos, PointInt dir, PointInt normal) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.5;
      double endSideDistance = game.TrackTileSize * 0.5 - sideDistance;

      PointDouble wayEnd = GetWayEnd(pos, dir);

      double endX = wayEnd.X + normal.X * endSideDistance + dir.X * sideDistance;
      double endY = wayEnd.Y + normal.Y * endSideDistance + dir.Y * sideDistance;

      return new PointDouble(endX, endY);
    }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }
  }
}
