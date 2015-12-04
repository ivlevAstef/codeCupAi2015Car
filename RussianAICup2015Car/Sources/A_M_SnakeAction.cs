﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakeAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      return validSnakeWithOffset(0);
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      Vector endPos = GetWayEnd(path[1].Pos, dirEnd);
      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y).Normalize();

      PhysicMoveCalculator calculator = new PhysicMoveCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, new Vector(dirMove.X, dirMove.Y), dir, 0.5);
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
      double maxAngle = Math.Cos(Math.PI / 18);

      if (validSnakeWithOffset(1) && validSnakeWithOffset(2) && Math.Abs(dir.Cross(Vector.sincos(car.Angle))) < maxAngle) {
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
  }
}
