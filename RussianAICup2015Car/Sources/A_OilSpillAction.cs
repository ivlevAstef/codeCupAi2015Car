using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_OilSpillAction : A_BaseAction {
    public override bool valid() {
      if (car.RemainingOiledTicks > 0 || car.OilCanisterCount <= 0) {
        return false;
      }

      if (!turn()) {
        return false;
      }

      if (Math.Abs(car.AngularSpeed) < 0.02) {
        return false;
      }

      if (isFindAroundOils()) {
        return false;
      }

      return centering();
    }

    public override void execute(Move move) {
      move.IsSpillOil = true;
    }

    private bool turn() {
      return !path.FirstWayCell.DirIn.Equals(path.FirstWayCell.DirOut);
    }

    private PointDouble oilCenter() {
      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double backsideX = -Math.Cos(car.Angle) * (car.Width * 0.5 + game.OilSlickInitialRange);
      double backsideY = -Math.Sin(car.Angle) * (car.Width * 0.5 + game.OilSlickInitialRange);

      return new PointDouble(centerX + backsideX, centerY + backsideY);
    }

    private bool centering() {
      PointDouble center = oilCenter();
      return car.GetDistanceTo(center.X, center.Y) < 4 * game.OilSlickRadius;
    }

    private bool isFindAroundOils() {
      PointDouble center = oilCenter();

      foreach(OilSlick oil in world.OilSlicks) {
        if (oil.GetDistanceTo(center.X, center.Y) < game.TrackTileSize * 0.75) {
          return true;
        }
      }

      return false;
    }
  }
}
