using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources.Common {
  public class CarMovedPath {
    public static readonly CarMovedPath Instance = new CarMovedPath();

    private static int MaxQueueSize = 10;

    private Dictionary<long, List<TilePos>> paths = new Dictionary<long, List<TilePos>>();

    public void Update(Car car) {

      if (!paths.ContainsKey(car.Id)) {
        paths.Add(car.Id, new List<TilePos>());
      }

      TilePos carTile = new TilePos(car.X, car.Y);

      List<TilePos> list = paths[car.Id];
      if (0 == list.Count || list[list.Count - 1] != carTile) {
        list.Add(carTile);
      }

      if (list.Count > MaxQueueSize) {
        list.RemoveAt(0);
      }
    }

    public bool HasTilePosForCar(TilePos pos, Car car) {
      if (!paths.ContainsKey(car.Id)) {
        return false;
      }

      return paths[car.Id].Contains(pos);
    }

    public int TilePosIndexForCar(TilePos pos, Car car) {
      if (!HasTilePosForCar(pos,car)) {
        return int.MaxValue;
      }

      List<TilePos> positions = paths[car.Id];
      for (int i = 0; i < positions.Count; i++) {
        if (positions[i] == pos) {
          return positions.Count - i;
        }
      }

      return int.MaxValue;
    }

  }
}
