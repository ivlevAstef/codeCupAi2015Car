using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class SnakeMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      return validSnakeWithOffset(0);
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;
      TileDir dirEnd = path[1].DirOut;

      Vector endPos = GetWayEnd(path[1].Pos, dirEnd);
      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y).Normalize();

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, dirMove, dir, 0.5);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      List<ActionType> result = new List<ActionType>() {
        ActionType.OilSpill,
        ActionType.Shooting,
        ActionType.SnakePreEnd
      };

      Vector dir = new Vector(path[0].DirOut.X + path[1].DirOut.X, path[0].DirOut.Y + path[1].DirOut.Y);
      double maxAngle = Math.Sin(Math.PI / 18);
      double angleDiff = Math.Abs(dir.Cross(Vector.sincos(car.Angle)));

      if (validSnakeWithOffset(1) && validSnakeWithOffset(2) && angleDiff < maxAngle) {
        result.Add(ActionType.UseNitro);
      }

      if (nearCrossRoadOrT()) {
        result.Add(ActionType.AvoidSideHit);
      }

      return result;
    }

    private bool nearCrossRoadOrT() {
      for (int i = 1; i < Math.Min(3, path.Count); i++) {
        if (path[i].DirOuts.Length >= 2) {//crosroad or T.
          return true;
        }
      }

      return false;
    }

    private bool validSnakeWithOffset(int offset) {
      if (3 + offset >= path.Count) {
        return true;
      }

      TilePos posIn = path[1 + offset].Pos;
      TilePos posOut = path[2 + offset].Pos;

      TileDir dirIn = path[1 + offset].DirIn;
      TileDir dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut.Equals(new TileDir(0))) {
        return false;
      }

      TileDir dir = new TileDir(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.PerpendicularLeft()) || dir.Equals(dirIn.PerpendicularRight()));
    }
  }
}
