using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class BackwardMoving : MovingBase {
    private int offset = 0;

    public override bool valid() {
      offset = 0;
      TileDir dir = path[offset].DirOut;

      double angle = car.GetAngleTo(car.X + dir.X, car.Y + dir.Y);
      if (Math.Abs(angle) > (2 * Math.PI / 3)) {
        return true;
      }

      offset = 1;
      TileDir dirIn = path[offset].DirIn;
      TileDir dirOut = path[offset].DirOut;

      if (dirIn == dirOut.Negative()) {
        return true;
      }

      return false;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;

      Vector dir = new Vector(dirMove.X, dirMove.Y);

      Vector carPos = new Vector(car.X, car.Y);
      Vector endPos = GetWayEnd(path[offset].Pos, TileDir.Zero);
      endPos.set(car.X * Math.Abs(dir.X) + endPos.X * Math.Abs(dir.Y), car.Y * Math.Abs(dir.Y) + endPos.Y * Math.Abs(dir.X));
      

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      if (0 == offset) {
        TileDir nextDirOut = path[1 + offset].DirOut;
        Vector nextDir = new Vector(nextDirOut.X, nextDirOut.Y);

        bool needBrake = false;
        double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
        if (speedSign > 0 || nextDirOut == dirMove) {
          endPos -= dir * game.TrackTileSize;
          needBrake = (nextDirOut != dirMove && speedSign > 0);
        } else {
          Vector tileEnd = path[offset].Pos.ToVector(0.5 * (1 + dir.X), 0.5 * (1 + dir.Y));
          endPos = GetWayEnd(path[1 + offset].Pos, nextDirOut.Negative() - dirMove);
          endPos += nextDir * (tileEnd - carPos).Dot(dir);
          endPos = carPos + (carPos - endPos);
          needBrake = car.Speed() > 8;
        }

        Move needMove = calculator.calculateBackMove(endPos, dirMove, nextDir);
        move.IsBrake = needMove.IsBrake || needBrake;
        move.EnginePower = needMove.EnginePower;
        move.WheelTurn = needMove.WheelTurn;
      } else {
        endPos -= dir * game.TrackTileSize;
        double distance = (carPos - GetWayEnd(path[offset].Pos, dirMove)).Dot(dir);

        Move needMove = calculator.calculateMove(endPos, dirMove, dir);
        move.IsBrake = needMove.IsBrake;
        move.EnginePower = game.CarEnginePowerChangePerTick + distance / game.TrackTileSize;
        move.WheelTurn = needMove.WheelTurn;
      }
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting
      };
    }
  }
}
