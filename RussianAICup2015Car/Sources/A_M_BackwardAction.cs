using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_BackwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      PointInt dir = path.FirstWayCell.DirOut;

      double angle = car.GetAngleTo(car.X + dir.X, car.Y + dir.Y);

      return Math.Abs(angle) > ((Math.PI / 2) + (Math.PI / 18) );///100 degrees
    }

    public override void execute(Move move) {
      move.EnginePower = -1.0;

      PointInt dirIn = path.FirstWayCell.DirIn;
      PointInt dir = path.FirstWayCell.DirOut;

      if (dirIn.Equals(dir) || dirIn.Equals(dir.Negative())) {
        dir = dir.Negative();
      } else {
        move.EnginePower = -0.1;
      }

      double magnitedAngle = magniteToCenter(dir);
      double magnitedForce = 0.5 * car.WheelTurnForAngle(magnitedAngle, game);

      move.WheelTurn = -magnitedForce;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }

    private double magniteToCenter(PointInt dir) {
      double powerTilt = -game.TrackTileSize * 3;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      double x = car.X * Math.Abs(dir.X) + centerX * Math.Abs(dir.Y) + powerTilt * dir.X;
      double y = car.Y * Math.Abs(dir.Y) + centerY * Math.Abs(dir.X) + powerTilt * dir.Y;

      double ticks = car.GetDistanceTo(x, y) / car.Speed();

      x -= ticks * car.SpeedX * Math.Abs(dir.Y);
      y -= ticks * car.SpeedY * Math.Abs(dir.X);

      return car.GetAngleTo(new PointDouble(centerX, centerY), dir, powerTilt);
    }
  }
}
