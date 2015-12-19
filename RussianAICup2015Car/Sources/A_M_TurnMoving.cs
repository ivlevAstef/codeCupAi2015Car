using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class TurnMoving : MovingBase {
    private int offset = 0;

    public override bool valid() {
      for (offset = 0; offset <= 1; offset++) {
        if (PathCheckResult.Yes == checkTurn(offset)) {
          return true;
        }
      }

      return false;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      MovingCalculator calculator = new MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, path[1 + offset].Pos);
      calculator.setupDefaultAction(GetWayEnd(path[1 + offset].Pos, dirEnd, 0.25));

      Vector endDir = new Vector(dirEnd.X, dirEnd.Y).Normalize();

      calculator.setupAngleReach(endDir);
      calculator.setupPassageLine(GetWayEnd(path[1 + offset].Pos, dirEnd), new Vector(dirMove.X, dirMove.Y), 1.1);

      Dictionary<TilePos, TileDir[]> selfMap = new Dictionary<TilePos, TileDir[]>();
      for (int i = 0; i <= offset; i++) {
        selfMap.Add(path[i].Pos, new TileDir[2] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() });
      }
      selfMap.Add(path[1 + offset].Pos, new TileDir[1] { dirMove.Negative() + dirEnd });

      calculator.setupSelfMapCrash(selfMap);
      calculator.setupAdditionalPoints(this.additionalPoints);

      Move needMove = calculator.calculateTurn(endDir);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill,
      };
    }
  }
}
