﻿using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;
using RussianAICup2015Car.Sources.Actions.Moving;

namespace RussianAICup2015Car.Sources {
  class BlockCarHitMoving : MovingBase {
    private const int MaxCheckTicks = 30;

    private Tuple<PCar, PCar> hitInfo = null;

    public override bool valid() {
      if (car.Durability < 0.25) {
        return false;
      }

      hitInfo = hitInformation();

      return null != hitInfo;
    }

    public override void execute(Move move) {
      PCar self = hitInfo.Item1;
      PCar enemy = hitInfo.Item2;

      Vector distance = self.Pos - enemy.Pos;
      Vector endPos = enemy.Pos + enemy.Dir * distance.Length;

      TileDir dirMove = path[0].DirOut;
      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);

      calculator.setupAngleReach(new Vector(dirMove.X, dirMove.Y));
      calculator.setupDefaultAction(endPos);

      Move needMove = calculator.calculateMove();
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    private Tuple<PCar, PCar> hitInformation() {
      PCar self = new PCar(car, game);
      List<PCar> enemies = new List<PCar>();
      foreach (Car iter in world.Cars) {
        Vector distance = new Vector(iter.X, iter.Y) - self.Pos;
        if (!iter.IsTeammate && 0 == iter.ProjectileCount && distance.Dot(self.Dir) < 0 && !iter.IsFinishedTrack) {
          enemies.Add(new PCar(iter, game));
        }
      }

      return hitInformation(self, enemies);
    }

    private Tuple<PCar, PCar> hitInformation(PCar self, List<PCar> enemies) {
      double minAngle = 0.5 * Math.Atan2(car.Height, car.Width);
      double maxAngle = Math.PI/ 2.57;

      self.setEnginePower(1);
      MoveToAngleFunction mover = new MoveToAngleFunction(new Vector(path[0].DirOut.X, path[0].DirOut.Y).Angle);

      for (int i = 0; i < MaxCheckTicks; i++) {
        foreach (PCar enemy in enemies) {
          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(Math.Acos(distance.Normalize().Dot(self.Dir.Negative())));
          if (distance.Length < game.CarWidth && minAngle < angle && angle < maxAngle) {
            return new Tuple<PCar, PCar>(self, enemy);
          }

          enemy.Iteration(1);
        }
        mover.Iteration(self, 1);
      }

      return null;
    }
  }
}
