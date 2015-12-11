using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class ForwardMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      return true;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);

      calculator.setupAngleReach(new Vector(dirMove.X, dirMove.Y));
      calculator.setupDefaultAction(EndSidePos());

      Move needMove = calculator.calculateMove();
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      List<ActionType> result = new List<ActionType>() {
        ActionType.PreTurn,
        ActionType.Shooting,
        ActionType.MoveToBonus,
        ActionType.DodgeHit
      };

      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
      double maxAngle = Math.Sin(Math.PI / 9);

      if (isStraight() && Math.Abs(dir.Cross(Vector.sincos(car.Angle))) < maxAngle) {
        result.Add(ActionType.UseNitro);
      }

      if (nearCrossRoad()) {
        result.Add(ActionType.AvoidSideHit);
      }

      return result;
    }

    private bool nearCrossRoad() {
      for (int i = 1; i < Math.Min(3, path.Count); i++) {
        if (path[i].DirOuts.Length == 3) {//because one dir it's IN
          return true;
        }
      }

      return false;
    }

    private Vector EndSidePos() {
      TilePos pos = path[0].Pos;
      TileDir dir = path[0].DirOut;
      TileDir normal = new TileDir(0);

      for (int i = 1; i < Math.Min(5, path.Count); i++) {
        if (null == path[i].DirOut) {
          pos = path[0].Pos + dir * 4;
          break;
        }

        if (!path[i].DirIn.Equals(path[i].DirOut)) {
          normal = path[i].DirOut;
          break;
        }
        pos = path[i].Pos;
        dir = path[i].DirIn;
      }

      return EndSidePos(pos, dir, normal.Negative());
    }

    private Vector EndSidePos(TilePos pos, TileDir dir, TileDir normal) {
      double sideDistance = (game.TrackTileSize * 0.5) - game.TrackTileMargin - game.CarHeight * 1.25;

      double centerX = (pos.X +0.5) * game.TrackTileSize;
      double centerY = (pos.Y +0.5) * game.TrackTileSize;

      double sideX = centerX + normal.X * sideDistance;
      double sideY = centerY + normal.Y * sideDistance;

      return new Vector(sideX, sideY);
    }
  }
}
