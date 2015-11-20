using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakeAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      PointInt posIn = path.WayCells[1].Pos;
      PointInt posOut = path.WayCells[2].Pos;

      PointInt dirIn = path.WayCells[1].DirIn;
      PointInt dirOut = path.WayCells[2].DirOut;

      if (null == dirOut || dirOut.Equals(new PointInt(0))) {
        return false;
      }

      PointInt dir = new PointInt(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.Perpendicular()) || dir.Equals(dirIn.Perpendicular().Negative()));
    }

    public override void execute(Move move) {
      PointInt dirMove = path.FirstWayCell.DirOut;
      PointInt dirEnd = path.ShortWayCells[0].DirOut;

      double magnitedAngle = magniteToCenter(dirMove, dirEnd);
      double magnitedForce = 0.25 * car.WheelTurnForAngle(magnitedAngle, game);

      Logger.instance.Debug("Angle: {0} Speed: {1}", magnitedAngle, car.Speed());

      if (Math.Abs(magnitedAngle) > Math.PI / (18 * car.Speed() / 20) && car.Speed() > 10) {
        move.IsBrake = true;
      }

      move.EnginePower = 1.0;

      move.WheelTurn = magnitedForce;
    }

    public override HashSet<ActionType> GetBlocks() {
      return new HashSet<ActionType>() { 
        ActionType.Backward,
        ActionType.Forward,
        ActionType.Turn,
        ActionType.Overtake,
        ActionType.UseNitro, //dynamic
        ActionType.MoveToBonus, // dynamic
      };
    } 

    private double magniteToCenter(PointInt dir1, PointInt dir2) {
      double powerTilt = game.TrackTileSize * 0.5;
      PointDouble dir = new PointDouble((dir1.X + dir2.X) / Math.Sqrt(2), (dir1.Y + dir2.Y) / Math.Sqrt(2));

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5 + 0.5 * dir1.X) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5 + 0.5 * dir1.Y) * game.TrackTileSize;

      return car.GetAngleTo(new PointDouble(centerX, centerY), dir, powerTilt);
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

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }
  }
}
