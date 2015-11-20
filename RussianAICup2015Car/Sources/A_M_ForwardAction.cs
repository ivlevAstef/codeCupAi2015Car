using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_ForwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      /*PathCell cell = path.ShortWayCells[0];
      if (null != cell.DirOut && !cell.DirOut.Equals(cell.DirIn)) {
        return false;
      }*/

      /*foreach(PathCell cell in path.ShortWayCells) {
        if (null != cell.DirOut && !cell.DirOut.Equals(cell.DirIn)) {
          return false;
        }
      }*/

      return true;
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;

      double magnitedAngle = magniteToCenter(path.FirstWayCell.DirOut);
      double magnitedForce = 0.75 * car.WheelTurnForAngle(magnitedAngle, game);

      if (Math.Abs(magnitedAngle) > Math.PI / (3 * car.Speed() / 25)) {
        magnitedForce *= 2;
        move.IsBrake = true;
      }

      move.WheelTurn = magnitedForce;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { 
      ActionType.InitialFreeze, 
      ActionType.Backward,
      ActionType.Turn,
      ActionType.PreTurn,
      ActionType.Snake,
      ActionType.Around,
      ActionType.MoveToBonus, 
      ActionType.StuckOut }; 
    } }

    private double magniteToCenter(PointInt dir) {
      double powerTilt = game.TrackTileSize * 3;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      double x = car.X * Math.Abs(dir.X) + centerX * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + centerY * Math.Abs(dir.X) + powerTilt * dir.Y;

      double ticks = car.GetDistanceTo(x, y) / car.Speed();

      x -= ticks * car.SpeedX * Math.Abs(dir.Y);
      y -= ticks * car.SpeedY * Math.Abs(dir.X);

      return car.GetAngleTo(new PointDouble(centerX,centerY), dir, powerTilt);
    }
  }
}
