using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class PreTurnMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (!path[1].DirIn.Equals(path[1].DirOut)) {
        return false;
      }

      return PathCheckResult.Yes == checkTurn(1);
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;
      TileDir dirEnd = path[2].DirOut;

      MovingCalculator calculator = new MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, path[2].Pos);
      calculator.setupDefaultAction(GetWayEnd(path[2].Pos, dirEnd.Negative()));

      Vector endDir = new Vector(dirEnd.X + dirMove.X, dirEnd.Y + dirMove.Y).Normalize();

      calculator.setupAngleReach(endDir);
      calculator.setupPassageLine(GetWayEnd(path[2].Pos, TileDir.Zero), new Vector(dirMove.X, dirMove.Y), 1.0);

      Dictionary<TilePos, TileDir[]> selfMap = new Dictionary<TilePos, TileDir[]>();
      for (int i = 0; i < 2; i++) {
        selfMap.Add(path[i].Pos, new TileDir[2] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() });
      }
      selfMap.Add(path[2].Pos, new TileDir[2] { dirEnd + dirMove.Negative(), dirEnd });
      
      calculator.setupSelfMapCrash(selfMap);
      calculator.setupAdditionalPoints(this.additionalPoints);

      Move needMove = calculator.calculateTurn(endDir);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }
  }
}
