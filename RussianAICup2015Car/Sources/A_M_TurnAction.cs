using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_TurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      if (!path.WayCells[0].DirIn.Equals(path.WayCells[0].DirOut)) {
        return false;
      }

      if (null != path.WayCells[2].DirOut && !path.WayCells[2].DirIn.Equals(path.WayCells[2].DirOut)) {
        return false;
      }

      PointInt dirIn = path.WayCells[1].DirIn;
      PointInt dirOut = path.WayCells[1].DirOut;

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirSelfToNext = path.WayCells[0].DirOut;
      PointInt dirNextToNextNext = path.WayCells[1].DirOut;

      double idealAngle = car.GetAngleTo(car.X + dirNextToNextNext.X, car.Y + dirNextToNextNext.Y);
      double nIdealAngle = Math.Abs(Math.Sin(idealAngle));
      nIdealAngle = (idealAngle < Math.PI / 2) ? nIdealAngle : (2 - nIdealAngle);
      double speed = car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY;
      double nSpeed = speed * nIdealAngle;

      double procent = procentToWay(path.WayCells[1].Pos, dirSelfToNext);

      double procentToSpeed = Math.Min(2.0f, nSpeed / (game.TrackTileSize / 80));
      procent = procent * ((4.0 - procentToSpeed * procentToSpeed) / 2.5);

      procent = Math.Min(1.0, Math.Max(0.0, procent));
      double xMoved = dirSelfToNext.X * procent + dirNextToNextNext.X * (1.0 - procent);
      double yMoved = dirSelfToNext.Y * procent + dirNextToNextNext.Y * (1.0 - procent);

      double needAngle = car.GetAngleTo(car.X + xMoved, car.Y + yMoved);
      move.EnginePower = 1.0f - Math.Min(0.2f, Math.Abs(needAngle / (Math.PI * 0.5)));

      move.WheelTurn = 25 * needAngle;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }

    private PointDouble convert(PointInt point) {
      double nextWaypointX = (point.X + 0.5) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private double pixelsToWay(PointInt way, PointInt dir) {
      Logger.instance.Assert(null != way, "zero way");
      Logger.instance.Assert(null != game, "zero game");

      PointDouble wayPos = convert(way);
      wayPos.X = car.X * Math.Abs(dir.Y) + wayPos.X * Math.Abs(dir.X);
      wayPos.Y = car.Y * Math.Abs(dir.X) + wayPos.Y * Math.Abs(dir.Y);
      return car.GetDistanceTo(wayPos.X, wayPos.Y);
    }

    private double procentToWay(PointInt way, PointInt dir) {
      Logger.instance.Assert(null != way, "zero way");
      Logger.instance.Assert(null != game, "zero game");

      return pixelsToWay(way, dir) / game.TrackTileSize;
    }
  }
}
