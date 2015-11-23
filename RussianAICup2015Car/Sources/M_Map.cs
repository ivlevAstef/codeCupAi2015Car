using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public class Map {
    public struct Cell {
      public PointInt Pos;

      public Tuple<Cell, int>[] neighboringCells;//int - Value of shortening the distance to checkpoint. default -1.
      //contains only valid cells -> by which you can get to the checkpoint
    }

    public static PointInt DirLeft = new PointInt(-1, 0);
    public static PointInt DirRight = new PointInt(1, 0);
    public static PointInt DirUp = new PointInt(0, -1);
    public static PointInt DirDown = new PointInt(0, 1);

    private Car car = null;
    private World world = null;
    private Game game = null;

    private static Dictionary<TileType, PointInt[]> directionsByTileType = new Dictionary<TileType, PointInt[]> {
      {TileType.Empty , new PointInt[0]},
      {TileType.Vertical , new PointInt[2] {DirDown, DirUp }},
      {TileType.Horizontal , new PointInt[2] { DirRight, DirLeft }},
      {TileType.LeftTopCorner , new PointInt[2] { DirDown, DirRight }},
      {TileType.RightTopCorner , new PointInt[2] { DirDown, DirLeft }},
      {TileType.LeftBottomCorner , new PointInt[2] { DirUp, DirRight }},
      {TileType.RightBottomCorner , new PointInt[2] { DirUp, DirLeft }},
      {TileType.LeftHeadedT , new PointInt[3] { DirLeft, DirUp, DirDown }},
      {TileType.RightHeadedT , new PointInt[3] { DirRight, DirUp, DirDown }},
      {TileType.TopHeadedT , new PointInt[3] { DirLeft, DirRight, DirUp }},
      {TileType.BottomHeadedT , new PointInt[3] { DirLeft, DirRight, DirDown }},
      {TileType.Crossroads , new PointInt[4] { DirLeft, DirRight, DirUp, DirDown }},
      {TileType.Unknown , new PointInt[0]}
    };

    public void setupEnvironment(Car car, World world, Game game) {
      this.car = car;
      this.world = world;
      this.game = game;
    }

    public Cell cellByMaxDepth(int maxDepth) {
      PointInt current = new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));

      int[,] map = createMap(current, checkpointByOffset(0));

      Cell? result = createCell(map, current, 0, maxDepth);

      Logger.instance.Assert(result.HasValue, "Can't create cell");

      return result.Value;
    }

    private Cell? createCell(int[,] map, PointInt pos, int checkPointOffset, int maxDepth) {
      if (maxDepth <= 0) {
        return null;
      }

      if (pos.Equals(checkpointByOffset(checkPointOffset))) {
        checkPointOffset++;
        map = createMap(pos, checkpointByOffset(checkPointOffset));
      }

      Cell result = new Cell();
      result.Pos = pos;

      List<Tuple<Cell, int>> cells = new List<Tuple<Cell,int>>();
      foreach (PointInt dir in dirsByPos(pos)) {
        PointInt iterPos = pos.Add(dir);

        if (map[iterPos.X, iterPos.Y] < map[pos.X, pos.Y] || checkToAlternative(map, pos, iterPos)) {
          Cell? iterCell = createCell(map, iterPos, checkPointOffset, maxDepth - 1);
          int length = map[iterPos.X, iterPos.Y] - map[pos.X, pos.Y];

          if (iterCell.HasValue) {
            cells.Add(new Tuple<Cell,int>(iterCell.Value, length));
          }
        }
      }

      result.neighboringCells = cells.ToArray();

      return result;
    }

    private bool checkToAlternative(int[,] map, PointInt currentPos, PointInt alternativePos) {
      foreach (PointInt dir in dirsByPos(alternativePos)) {
        PointInt pos = alternativePos.Add(dir);
        if (!pos.Equals(currentPos) && map[pos.X, pos.Y] < map[alternativePos.X, alternativePos.Y]) {
          return true;
        }
      }

      return false;
    }

    private PointInt checkpointByOffset(int offset) {
      int checkPointIndex = (car.NextWaypointIndex + offset) % world.Waypoints.Length;
      return new PointInt(world.Waypoints[checkPointIndex][0], world.Waypoints[checkPointIndex][1]);
    }

    private int[,] createMap(PointInt begin, PointInt end) {
      Logger.instance.Assert(null != world, "zero world");

      int[,] result = initMapData();
      bool[,] visited = initVisitedData();

      Queue<PointInt> backStack = new Queue<PointInt>();

      Queue<PointInt> stack = new Queue<PointInt>();
      stack.Enqueue(begin);

      while (stack.Count > 0) {
        PointInt pos = stack.Dequeue();

        if (visited[pos.X, pos.Y]) {
          continue;
        }

        if (pos.Equals(end) || TileType.Unknown == world.TilesXY[pos.X][pos.Y]) {
          result[pos.X, pos.Y] = Math.Abs(pos.X - end.X) + Math.Abs(pos.Y - end.Y);
          backStack.Enqueue(pos);
        }

        visited[pos.X, pos.Y] = true;
        foreach (PointInt dir in dirsByPos(pos)) {
          stack.Enqueue(pos.Add(dir));
        }
      }

      while (backStack.Count > 0) {
        PointInt pos = backStack.Dequeue();

        foreach (PointInt dir in dirsByPos(pos)) {
          PointInt nextPos = pos.Add(dir);
          if (result[nextPos.X, nextPos.Y] > result[pos.X, pos.Y] + 1) {
            result[nextPos.X, nextPos.Y] = result[pos.X, pos.Y] + 1;
            backStack.Enqueue(nextPos);
          }
        }
      }

      return result;
    }

    private PointInt[] dirsByPos(PointInt pos) {
      return directionsByTileType[world.TilesXY[pos.X][pos.Y]];
    }

    private int[,] initMapData() {
      int[,] data = new int[world.Width, world.Height];
      for (int i = 0; i < world.Width; i++) {
        for (int j = 0; j < world.Height; j++) {
          data[i, j] = world.Width * world.Height;
        }
      }
      return data;
    }

    private bool[,] initVisitedData() {
      bool[,] data = new bool[world.Width, world.Height];
      for (int i = 0; i < world.Width; i++) {
        for (int j = 0; j < world.Height; j++) {
          data[i, j] = false;
        }
      }
      return data;
    }

  }
}
