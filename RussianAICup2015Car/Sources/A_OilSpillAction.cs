using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_OilSpillAction : A_BaseAction {
    public override bool valid() {
      return car.RemainingOiledTicks <= 0 && car.OilCanisterCount > 0 && turn() && centering();
    }

    public override void execute(Move move) {
      move.IsSpillOil = true;
    }

    private bool turn() {
      return !path.FirstWayCell.DirIn.Equals(path.FirstWayCell.DirOut);
    }

    private bool centering() {
      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      return car.GetDistanceTo(centerX, centerY) < game.OilSlickRadius;

    }
  }
}
