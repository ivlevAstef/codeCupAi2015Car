using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class AroundMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      TilePos posIn = path[1].Pos;
      TilePos posOut = path[2].Pos;

      TileDir dirIn = path[1].DirIn;
      TileDir dirOut = path[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      bool isLine = path[1].DirIn.Equals(path[1].DirOut) || path[2].DirIn.Equals(path[2].DirOut);

      return !isLine && dirIn.Equals(dirOut.Negative()) && !posIn.Equals(posOut);
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;
      TileDir dirEnd = path[1].DirOut;

      Vector endPos = GetWayEnd(path[1].Pos, new TileDir(0));
      //endPos = endPos + new Vector(dirMove.X, dirMove.Y) * game.TrackTileSize * 0.1;

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

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
