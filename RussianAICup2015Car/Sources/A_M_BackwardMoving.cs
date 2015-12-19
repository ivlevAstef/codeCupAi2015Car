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

      double minAngle = (path[offset].DirOut == path[offset].DirIn) ? (11 * Math.PI / 18) : (16 * Math.PI / 18);

      double angle = car.GetAngleTo(car.X + dir.X, car.Y + dir.Y);
      if (Math.Abs(angle) > minAngle) {
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
      endPos.X = car.X * Math.Abs(dir.X) + endPos.X * Math.Abs(dir.Y);
      endPos.Y = car.Y * Math.Abs(dir.Y) + endPos.Y * Math.Abs(dir.X);
      
      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);
      calculator.useBackward();

      if (0 == offset) {
        TileDir nextDirOut = path[1].DirOut;
        Vector nextDir = new Vector(nextDirOut.X, nextDirOut.Y);

        bool needBrake = false;
        bool needChangeEnginePower = false;
        double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

        if (speedSign > 0 || nextDirOut == dirMove) {
          endPos -= dir * 2 * game.TrackTileSize;
          needBrake = speedSign > 0;
        } else if (nextDirOut.Negative() == dirMove) {
          endPos -= dir * 2 * game.TrackTileSize;
          needChangeEnginePower = true;
          needBrake = car.Speed() > 8;
        } else {
          endPos -= dir * 2 * game.TrackTileSize;
          endPos += nextDir * car.Height;
          needBrake = car.Speed() > 10;
        }

        calculator.setupAngleReach(nextDir);
        calculator.setupDefaultAction(endPos);

        Move needMove = calculator.calculateMove();
        move.IsBrake = needMove.IsBrake || needBrake;
        move.EnginePower = needMove.EnginePower;
        move.WheelTurn = needMove.WheelTurn;

        if (needChangeEnginePower) {
          double distance = (GetWayEnd(path[1].Pos, dirMove.Negative()) - carPos).Dot(dir);
          move.EnginePower = -game.CarEnginePowerChangePerTick - distance / game.TrackTileSize;
        }
      } else {
        endPos -= dir * 2 * game.TrackTileSize;
        double distance = (carPos - GetWayEnd(path[offset].Pos, dirMove)).Dot(dir);

        calculator.setupAngleReach(dir);
        calculator.setupDefaultAction(endPos);
        Move needMove = calculator.calculateMove();
        move.IsBrake = needMove.IsBrake || car.Speed() > 8;
        move.EnginePower = game.CarEnginePowerChangePerTick + distance / game.TrackTileSize;
        move.WheelTurn = needMove.WheelTurn;
      }
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
      };
    }
  }
}
