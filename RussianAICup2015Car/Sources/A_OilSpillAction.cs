using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_OilSpillAction : A_BaseAction {
    public override bool valid() {
      if (car.RemainingOiledTicks > 0 || car.OilCanisterCount <= 0) {
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

    private Vector oilCenter() {
      Vector backSide = Vector.sincos(car.Angle) * (car.Width * 0.5 + game.OilSlickInitialRange);

      return new Vector(car.X, car.Y) - backSide;
    }

    private Vector carTileCenter() {
      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      return new Vector(centerX , centerY);
    }

    private bool centering() {
      Vector dirMove = new Vector(path[0].DirIn.X,path[0].DirIn.Y);
      return Math.Abs((carTileCenter() - oilCenter()).Cross(dirMove)) < game.OilSlickRadius * 0.25;
    }

    private bool isFindAroundOils() {
      Vector center = oilCenter();

      foreach(OilSlick oil in world.OilSlicks) {
        if (oil.GetDistanceTo(center.X, center.Y) < game.TrackTileSize * 0.75) {
          return true;
        }
      }

      return false;
    }
  }
}
