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

      PointInt posIn = path[1 + offset].Pos;
      PointInt posOut = path[2 + offset].Pos;

      PointInt dirIn = path[1 + offset].DirIn;
      PointInt dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut.Equals(new PointInt(0))) {
        return false;
      }

      PointInt dir = new PointInt(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.PerpendicularLeft()) || dir.Equals(dirIn.PerpendicularRight()));
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0 + offset].DirOut;
      PointInt dirEnd = path[1 + offset].DirOut;

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
