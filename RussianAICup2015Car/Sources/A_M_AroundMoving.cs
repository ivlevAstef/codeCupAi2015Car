using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class AroundMoving : MovingBase {
    private int offset = 1;

    public override bool valid() {
      for (offset = 0; offset <= 1; offset++) {
        if (PathCheckResult.Yes == checkAround(offset)) {
          return true;
        }
      }

      return false;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;
      TilePos endTile = path[1 + offset].Pos;

      Vector endPos = GetWayEnd(path[1 + offset].Pos, dirMove.Negative(), 1.0);

      MovingCalculator calculator = new MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, path[1 + offset].Pos);
      calculator.setupDefaultAction(GetWayEnd(endTile, dirEnd.Negative() + dirMove.Negative(), 1.0));

      Vector endDir = new Vector(dirEnd.X, dirEnd.Y).Normalize();

      calculator.setupAngleReach(endDir);
      calculator.setupPassageLine(GetWayEnd(endTile, dirEnd), new Vector(dirMove.X, dirMove.Y).Normalize(), 0.75);

      Dictionary<TilePos, TileDir[]> selfMap = new Dictionary<TilePos, TileDir[]>();
      for (int i = 0; i <= offset; i++) {
        selfMap.Add(path[i].Pos, new TileDir[2] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() });
      }
      selfMap.Add(endTile, new TileDir[2] { dirEnd.Negative() , dirEnd + dirMove.Negative() });

      calculator.setupSelfMapCrash(selfMap);
      
      Move needMove = calculator.calculateTurn(endDir);

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
