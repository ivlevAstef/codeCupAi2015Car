using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_AroundAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      PointInt posIn = path[1].Pos;
      PointInt posOut = path[2].Pos;

      PointInt dirIn = path[1].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.Negative()) && !posIn.Equals(posOut);
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      Vector endPos = GetWayEnd(path[1].Pos, new PointInt(0));
      //endPos = endPos + new Vector(dirMove.X, dirMove.Y) * game.TrackTileSize * 0.1;

      PhysicMoveCalculator calculator = new PhysicMoveCalculator();
      calculator.setupEnvironment(car, map, game);

      Vector dir = new Vector(dirEnd.X, dirEnd.Y);
      Move needMove = calculator.calculateMove(endPos, new Vector(dirMove.X, dirMove.Y), dir, 0.03);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill
      };
    }
  }
}
