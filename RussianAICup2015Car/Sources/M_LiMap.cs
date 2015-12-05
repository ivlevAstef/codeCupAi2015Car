using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Map {
  public class LiMap {
    public class Cell {
      public TilePos Pos { get; set; }

      public HashSet<TileDir> Dirs { get; set; }
      public List<Tuple<Cell, int>> NeighboringCells { get; set; }//int - Value of shortening the distance to checkpoint. default -1.
      //contains only valid cells -> by which you can get to the checkpoint
    }

    private class CellKey {
      public TilePos Pos;
      public int CheckPoint;

      public CellKey(TilePos pos, int checkPoint) {
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

    private Car car = null;
    private GlobalMap gmap = null;

    private TilePos posCache = null;
    private Dictionary<TilePos, int[,]> mapCache = new Dictionary<TilePos, int[,]>();

    public void setupEnvironment(Car car, GlobalMap gmap) {
      this.car = car;
      this.gmap = gmap;
    }

    public Cell cellByMaxDepth(int maxDepth) {
      TilePos current = new TilePos(car.X, car.Y);
      if (null == posCache || !posCache.Equals(current)) {
        mapCache.Clear();
        posCache = current;
      }

      Cell result = createCell(current, 0, maxDepth);

      HashSet<Cell> visited = new HashSet<Cell>();
      result = simplifiedCell(result, visited);

      return result;
    }

    private Cell createCell(TilePos beginPos, int beginCheckPointOffset, int maxDepth) {
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
      int[,] map = getMap(checkPointOffset);
      TilePos pos = cell.Pos;

      cell.Dirs = gmap.Dirs(pos);

      List<Tuple<Cell, int>> cells = new List<Tuple<Cell, int>>();
      foreach (TileDir dir in cell.Dirs) {
        TilePos iterPos = pos + dir;

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

    private bool checkToAlternative(int[,] map, TilePos currentPos, TilePos alternativePos) {
      foreach (TileDir dir in gmap.Dirs(alternativePos)) {
        TilePos pos = alternativePos + dir;
        if (!pos.Equals(currentPos) && map[pos.X, pos.Y] < map[alternativePos.X, alternativePos.Y]) {
          return true;
        }
      }

      return false;
    }

    private TilePos checkpointByOffset(int offset) {
      int checkPointIndex = (car.NextWaypointIndex + offset) % gmap.WayPoints.Length;
      return new TilePos(gmap.WayPoints[checkPointIndex].X, gmap.WayPoints[checkPointIndex].Y);
    }

    private int[,] getMap(int offset) {
      TilePos checkPoint = checkpointByOffset(offset);
      if (!mapCache.ContainsKey(checkPoint)) {
        TilePos lastPos = 0 == offset ? posCache : checkpointByOffset(offset - 1);
        mapCache[checkPoint] = createMap(lastPos, checkPoint);
      }
      return mapCache[checkPoint];
    }

    private int[,] createMap(TilePos begin, TilePos end) {
      int[,] result = initMapData();
      bool[,] visited = initVisitedData();

      Queue<TilePos> backStack = new Queue<TilePos>();
      Queue<TilePos> backUnknownStack = new Queue<TilePos>();

      Queue<TilePos> stack = new Queue<TilePos>();
      stack.Enqueue(begin);

      while (stack.Count > 0) {
        TilePos pos = stack.Dequeue();

        if (visited[pos.X, pos.Y]) {
          continue;
        }

        if (pos.Equals(end)) {
          result[pos.X, pos.Y] = 0;
          backStack.Enqueue(pos);
        }

        visited[pos.X, pos.Y] = true;

        bool foundUnknown = false;
        foreach (TileDir dir in gmap.Dirs(pos)) {
          TilePos iterPos = pos + dir;
          if (!visited[iterPos.X, iterPos.Y]) {
            foundUnknown |= (TileType.Unknown == gmap.Type(iterPos));
            stack.Enqueue(iterPos);
          }
        }

        if (foundUnknown) {
          backUnknownStack.Enqueue(pos);
        }
      }

      int unknownMult = (backStack.Count > 0) ? 2 : 1;
      while (backUnknownStack.Count > 0) {
        TilePos pos = backUnknownStack.Dequeue();
        result[pos.X, pos.Y] = unknownMult * (Math.Abs(pos.X - end.X) + Math.Abs(pos.Y - end.Y));
        backStack.Enqueue(pos);
      }


      while (backStack.Count > 0) {
        TilePos pos = backStack.Dequeue();

        foreach (TileDir dir in gmap.Dirs(pos)) {
          TilePos nextPos = pos + dir;
          if (result[nextPos.X, nextPos.Y] > result[pos.X, pos.Y] + 1) {
            result[nextPos.X, nextPos.Y] = result[pos.X, pos.Y] + 1;
            if (!nextPos.Equals(begin)) {
              backStack.Enqueue(nextPos);
            }
          }
        }
      }

      return result;
    }

    private int[,] initMapData() {
      int[,] data = new int[gmap.Width, gmap.Height];
      for (int i = 0; i < gmap.Width; i++) {
        for (int j = 0; j < gmap.Height; j++) {
          data[i, j] = gmap.Width * gmap.Height;
        }
      }
      return data;
    }

    private bool[,] initVisitedData() {
      bool[,] data = new bool[gmap.Width, gmap.Height];
      for (int i = 0; i < gmap.Width; i++) {
        for (int j = 0; j < gmap.Height; j++) {
          data[i, j] = false;
        }
      }
      return data;
    }


    private bool containsDir(TileDir dir, TileDir[] dirs) {
      bool contains = false;
      foreach(TileDir iterDir in dirs) {
        contains |= iterDir.Equals(dir);
      }
      return contains;
    }
  }
}
