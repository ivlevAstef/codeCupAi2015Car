using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class SnakePreEndMoving : MovingBase {
    private const int offset = 1;

    public override bool valid() {
      return !validSnakeWithOffset(offset);
    }

    private bool validSnakeWithOffset(int offset) {
      if (3 + offset >= path.Count) {
        return true;
      }

      TileDir posIn = path[1 + offset].Pos;
      TileDir posOut = path[2 + offset].Pos;

      TileDir dirIn = path[1 + offset].DirIn;
      TileDir dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut.Equals(new TileDir(0))) {
        return false;
      }

      TileDir dir = new TileDir(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.PerpendicularLeft()) || dir.Equals(dirIn.PerpendicularRight()));
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0 + offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      Vector endPos = GetWayEnd(path[1 + offset].Pos, dirEnd);
      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y).Normalize();
      dirMove = path[0].DirOut;

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, new Vector(dirMove.X, dirMove.Y), dir, 0.5);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      //move.WheelTurn = needMove.WheelTurn;
    }
  }
}
