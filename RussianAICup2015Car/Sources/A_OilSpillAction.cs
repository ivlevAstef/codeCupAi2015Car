using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions {
  class OilSpillAction : BaseAction {
    public override bool valid() {
      if (car.RemainingOiledTicks > 0 || car.OilCanisterCount <= 0) {
        return false;
      }

      if (!centering()) {
        return false;
      }

      if (isBackward()) {
        return false;
      }

      if (isFindAroundOils()) {
        return false;
      }

      if (isBackSelfCar()) {
        return false;
      }

      if (!isBackEnemyCar()) {
        return false;
      }

      return true;
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

    private bool isBackSelfCar() {
      foreach (Car iterCar in world.Cars) {
        if (iterCar.IsTeammate && iterCar.Id != car.Id) {
          TilePos tile = new TilePos(iterCar.X, iterCar.Y);
          int index = CarMovedPath.Instance.TilePosIndexForCar(tile, car);
          if (index < 5) {
            return true;
          }
        }
      }

      return false;
    }

    private bool isBackEnemyCar() {
      foreach (Car iterCar in world.Cars) {
        if (!iterCar.IsTeammate) {
          TilePos tile = new TilePos(iterCar.X, iterCar.Y);
          int index = CarMovedPath.Instance.TilePosIndexForCar(tile, car);
          if (0 < index && index <= 8) {
            return true;
          }
        }
      }

      return false;
    }

    private bool isBackward() {
      return Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY))) <= 0;
    }
  }
}
