using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public class Map {
    public class Cell {
      public PointInt Pos { get; set; }

      public PointInt[] Dirs { get; set; }
      public List<Tuple<Cell, int>> NeighboringCells { get; set; }//int - Value of shortening the distance to checkpoint. default -1.
      //contains only valid cells -> by which you can get to the checkpoint
    }

    private class CellKey {
      public PointInt Pos;
      public int CheckPoint;

      public CellKey(PointInt pos, int checkPoint) {
        this.Pos = pos;
        this.CheckPoint = checkPoint;
      }

      public override bool Equals(object obj) {
        var p = obj as CellKey;
        if (null == p) {
          return false;
        }

        return Pos.Equals(p.Pos) && CheckPoint.Equals(p.CheckPoint);
      }

      public override int GetHashCode() {
        return Pos.GetHashCode() ^ CheckPoint.GetHashCode();
      }
    }

    private class PrivateCellData {
      public Cell Cell;
      public int CheckPointOffset;
      public int Depth;
      public bool Alternative;

      public PrivateCellData(Cell cell, int checkPoint, int depth, bool alternative) {
        this.Cell = cell;
        this.CheckPointOffset = checkPoint;
        this.Depth = depth;
        this.Alternative = alternative;
      }
    }

    public static PointInt DirLeft = new PointInt(-1, 0);
    public static PointInt DirRight = new PointInt(1, 0);
    public static PointInt DirUp = new PointInt(0, -1);
    public static PointInt DirDown = new PointInt(0, 1);

    private Car car = null;
    private World world = null;
    private Game game = null;

    private PointInt posCache = null;
    private Dictionary<PointInt, int[,]> mapCache = new Dictionary<PointInt, int[,]>();

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
      if (null == posCache || !posCache.Equals(current)) {
        mapCache.Clear();
        posCache = current;
      }

      Cell result = createCell(current, 0, maxDepth);

      HashSet<Cell> visited = new HashSet<Cell>();
      result = simplifiedCell(result, visited);

      return result;
    }

    private Cell createCell(PointInt beginPos, int beginCheckPointOffset, int maxDepth) {
      Cell res = new Cell();
      res.Pos = beginPos;

      Dictionary<CellKey, Cell> allCells = new Dictionary<CellKey, Cell>();

      Queue<PrivateCellData> stack = new Queue<PrivateCellData>();
      stack.Enqueue(new PrivateCellData(res, beginCheckPointOffset, 0, false));
      allCells.Add(new CellKey(beginPos, beginCheckPointOffset), res);

      while(stack.Count > 0) {
        PrivateCellData data = stack.Dequeue();

        if (data.Depth >= maxDepth) {
          continue;
        }

        while (data.Cell.Pos.Equals(checkpointByOffset(data.CheckPointOffset))) {
          data.CheckPointOffset++;
        }

        fillCell(ref data.Cell, data.CheckPointOffset, allCells, !data.Alternative);

        foreach(Tuple<Cell,int> subData in data.Cell.NeighboringCells) {
          CellKey key = new CellKey(subData.Item1.Pos, data.CheckPointOffset);
          if (!allCells.ContainsKey(key)) {
            stack.Enqueue(new PrivateCellData(subData.Item1, data.CheckPointOffset, data.Depth + 1, subData.Item2 > 0));
            allCells.Add(key, subData.Item1);
          }
        }
      }

      return res;
    }

    private Cell simplifiedCell(Cell cell, HashSet<Cell> visited) {
      List<Tuple<Cell, int>> neighboring = new List<Tuple<Cell,int>>();

      visited.Add(cell);

      foreach (Tuple<Cell, int> data in cell.NeighboringCells) {
        if(null != data.Item1.Dirs && null != data.Item1.NeighboringCells) {
          if (visited.Contains(data.Item1)) {
            neighboring.Add(new Tuple<Cell, int>(data.Item1, data.Item2));
          } else {
            neighboring.Add(new Tuple<Cell, int>(simplifiedCell(data.Item1, visited), data.Item2));
          }
        }
      }

      cell.NeighboringCells = neighboring;

      return cell;
    }

    private void fillCell(ref Cell cell, int checkPointOffset, Dictionary<CellKey, Cell> allCells, bool useAlternative) {
      int[,] map = getMap(checkpointByOffset(checkPointOffset));
      PointInt pos = cell.Pos;

      cell.Dirs = dirsByPos(pos);

      List<Tuple<Cell, int>> cells = new List<Tuple<Cell, int>>();
      foreach (PointInt dir in cell.Dirs) {
        PointInt iterPos = pos + dir;

        if (map[iterPos.X, iterPos.Y] < map[pos.X, pos.Y] || (useAlternative && checkToAlternative(map, pos, iterPos))) {
          CellKey key = new CellKey(iterPos, checkPointOffset);

          Cell iterCell = null;
          if (allCells.ContainsKey(key)) {
            iterCell = allCells[key];
          } else {
            iterCell = new Cell();
            iterCell.Pos = iterPos;
          }

          int length = map[iterPos.X, iterPos.Y] - map[pos.X, pos.Y];
          cells.Add(new Tuple<Cell, int>(iterCell, length));
        }
      }

      cell.NeighboringCells = cells;
    }

    private bool checkToAlternative(int[,] map, PointInt currentPos, PointInt alternativePos) {
      foreach (PointInt dir in dirsByPos(alternativePos)) {
        PointInt pos = alternativePos + dir;
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

    private int[,] getMap(PointInt checkpoint) {
      if (!mapCache.ContainsKey(checkpoint)) {
        mapCache[checkpoint] = createMap(posCache, checkpoint);
      }
      return mapCache[checkpoint];
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

        if (pos.Equals(end)) {
          result[pos.X, pos.Y] = 0;
          backStack.Enqueue(pos);
        }

        visited[pos.X, pos.Y] = true;

        bool allUnknown = true;
        foreach (PointInt dir in dirsByPos(pos)) {
          PointInt iterPos = pos + dir;
          if (!visited[iterPos.X, iterPos.Y]) {
            allUnknown &= (TileType.Unknown == world.TilesXY[iterPos.X][iterPos.Y]);
            stack.Enqueue(iterPos);
          }
        }

        if (allUnknown) {
          result[pos.X, pos.Y] = Math.Abs(pos.X - end.X) + Math.Abs(pos.Y - end.Y);
          backStack.Enqueue(pos);
        }
      }

      while (backStack.Count > 0) {
        PointInt pos = backStack.Dequeue();

        foreach (PointInt dir in dirsByPos(pos)) {
          PointInt nextPos = pos + dir;
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
