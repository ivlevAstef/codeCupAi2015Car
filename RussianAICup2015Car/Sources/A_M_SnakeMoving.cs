using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class SnakeMoving : MovingBase {
    private int offset = 0;

    public override bool valid() {
      for (offset = 0; offset <= 1; offset++) {
        if (PathCheckResult.Yes == checkSnakeWithOffset(offset)) {
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

      Vector endPos = GetWayEnd(path[1 + offset].Pos, TileDir.Zero) - new Vector(dirMove.X, dirMove.Y) * game.TrackTileSize * 0.5;
      calculator.setupDefaultAction(endPos);

      Vector endDir = new Vector(dirEnd.X + dirMove.X, dirEnd.Y + dirMove.Y).Normalize();

      calculator.setupAngleReach(endDir);
      calculator.setupPassageLine(endPos, new Vector(dirMove.X - dirEnd.X, dirMove.Y - dirEnd.Y).Normalize(), 0.45);

      Dictionary<TilePos, TileDir[]> selfMap = new Dictionary<TilePos, TileDir[]>();
      for (int i = 0; i <= offset + 1; i++) {
        if (path[i].DirIn == path[i].DirOut) {
          selfMap.Add(path[i].Pos, new TileDir[2] { path[i].DirIn.PerpendicularLeft(), path[i].DirIn.PerpendicularRight() });
        } else {
          selfMap.Add(path[i].Pos, new TileDir[3] { path[i].DirIn, path[i].DirOut.Negative(), path[i].DirIn.Negative() + path[i].DirOut });
        }
      }

      calculator.setupSelfMapCrash(selfMap);
      calculator.setupAdditionalPoints(this.additionalPoints);

      Move needMove = calculator.calculateTurn(endDir);
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

      if (PathCheckResult.Yes == checkSnakeWithOffset(1) && PathCheckResult.Yes == checkSnakeWithOffset(2) && angleDiff < maxAngle) {
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
  }
}
