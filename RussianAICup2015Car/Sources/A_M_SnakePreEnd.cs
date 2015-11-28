using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakePreEnd : A_M_BaseMoveAction {
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
      PointInt dirOut = path[1 + offset].DirOut;
      Vector dir = new Vector(dirOut.X, dirOut.Y);

      if (Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.25) > 0) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.5) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }
    }
  }
}
