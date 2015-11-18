using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_ForwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      for (int i = 1; i < path.wayCells.Length; i++) {
        PathCell cell = path.wayCells[i];
        if (null != cell.DirOut && !cell.DirOut.Equals(cell.DirIn)) {
          return false;
        }
      }

      return true;
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;

      
      double magnitedForce = magniteToCenter(path.wayCells[0].DirIn);

      move.WheelTurn = magnitedForce / (Math.PI * 0.5);
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { 
      ActionType.InitialFreeze, 
      ActionType.Turn,
      ActionType.PreTurn,
      ActionType.Snake,
      ActionType.Around,
      ActionType.MoveToBonus, 
      ActionType.StuckOut }; 
    } }

    private double magniteToCenter(PointInt dir) {
      double powerTilt = game.TrackTileSize * 1;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      double x = car.X * Math.Abs(dir.X) + centerX * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + centerY * Math.Abs(dir.X) + powerTilt * dir.Y;

      return car.GetAngleTo(x, y);
    }

    private double magniteToPoint(double x, double y, PointInt dir) {
      return car.GetAngleTo(x,y);
    }
  }
}
