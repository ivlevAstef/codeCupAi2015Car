﻿using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Map {
  public class LiMap {
    public class Cell {
      public readonly TilePos Pos;
      public readonly HashSet<TileDir> Dirs;
      public Transition[] Transitions { get { return transitions; } }
      private Transition[] transitions;

      public Cell(TilePos pos, HashSet<TileDir> dirs) {
        this.Pos = pos;
        this.Dirs = dirs;
        this.transitions = new Transition[0];
      }

      public void setTransitions(List<Transition> transitions) {
        this.transitions = transitions.ToArray();
      }
    }

    public class Transition {
      public readonly Cell ToCell;
      public readonly int Weight;
      public readonly bool isCheckpoint;

      public Transition(Cell toCell, int weight, bool isCheckpoint) {
        this.ToCell = toCell;
        this.Weight = weight;
        this.isCheckpoint = isCheckpoint;
      }
    }

    private class CellKey {
      public readonly TilePos Pos;
      public readonly int CheckPoint;

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

    private class CellData {
      public Cell Cell { get; set; }
      public int CheckPointOffset { get; set; }
      public int Depth { get; set; }
      public bool Alternative { get; set; }

      public CellData(Cell cell, int checkPoint, int depth, bool alternative) {
        this.Cell = cell;
        this.CheckPointOffset = checkPoint;
        this.Depth = depth;
        this.Alternative = alternative;
      }
    }

    private Car car = null;
    private GlobalMap gmap = null;

    private TilePos lastCarTilePos = null;
    private Dictionary<TilePos, int[,]> mapCache = new Dictionary<TilePos, int[,]>();
    private bool hasUnknown = false;

    public void SetupEnvironment(Car car, GlobalMap gmap) {
      this.car = car;
      this.gmap = gmap;
      hasUnknown = false;
    }

    public Cell FirstCellWithTransitions(int maxDepth) {
      TilePos currentCarTilePos = new TilePos(car.X, car.Y);
      if (null == lastCarTilePos || !lastCarTilePos.Equals(currentCarTilePos)) {
        mapCache.Clear();
        lastCarTilePos = currentCarTilePos;
      }

      Cell result = new Cell(currentCarTilePos, gmap.Dirs(currentCarTilePos));
      fillCell(result, 0, maxDepth);

      HashSet<Cell> visited = new HashSet<Cell>();
      Logger.instance.Assert(MaxDepth(result, visited) >= 3, "Can't create full path (first cell with trasitions)");

      return result;
    }

    public bool HasUnknown { get {return hasUnknown; } }

    private int MaxDepth(Cell cell, HashSet<Cell> visited) {
      visited.Add(cell);

      int max = 0;
      foreach (Transition data in cell.Transitions) {
        if (null != data.ToCell && !visited.Contains(data.ToCell)) {
          max = Math.Max(max, MaxDepth(data.ToCell, visited)+1);
        }
      }

      return max;
    }

    private void fillCell(Cell cell, int beginCheckPointOffset, int maxDepth) {
      Dictionary<CellKey, Cell> cellsCache = new Dictionary<CellKey, Cell>();
      cellsCache.Add(new CellKey(cell.Pos, beginCheckPointOffset), cell);

      Queue<CellData> stack = new Queue<CellData>();
      stack.Enqueue(new CellData(cell, beginCheckPointOffset, 0, false));

      while(stack.Count > 0) {
        CellData data = stack.Dequeue();

        if (data.Depth >= maxDepth || TileType.Unknown == gmap.Type(data.Cell.Pos)) {
          continue;
        }

        while (data.Cell.Pos == checkpointByOffset(data.CheckPointOffset)) {
          data.CheckPointOffset++;
        }

        fillCell(data, cellsCache);

        foreach(Transition subData in data.Cell.Transitions) {
          CellKey key = new CellKey(subData.ToCell.Pos, data.CheckPointOffset);
          if (!cellsCache.ContainsKey(key)) {
            stack.Enqueue(new CellData(subData.ToCell, data.CheckPointOffset, data.Depth + 1, subData.Weight > 0));
            cellsCache.Add(key, subData.ToCell);
          }
        }
      }
    }

    private void fillCell(CellData data, Dictionary<CellKey, Cell> cellsCache) {
      int[,] map = getMap(data.CheckPointOffset);
      TilePos pos = data.Cell.Pos;

      List<Transition> transitions = new List<Transition>();
      foreach (TileDir dir in data.Cell.Dirs) {
        TilePos iterPos = pos + dir;

        bool alternative = (!data.Alternative && checkToAlternative(map, pos, iterPos));
        bool unknown = checkToUnknown(pos, iterPos, 2);

        Logger.instance.Assert(0 <= iterPos.X && iterPos.X < gmap.Width && 0 <= iterPos.Y && iterPos.Y < gmap.Height, "WTF?");

        if (map[iterPos.X, iterPos.Y] < map[pos.X, pos.Y] || unknown || alternative) {
          CellKey key = new CellKey(iterPos, data.CheckPointOffset);

          Cell iterCell = null;
          if (cellsCache.ContainsKey(key)) {
            iterCell = cellsCache[key];
          } else {
            iterCell = new Cell(iterPos, gmap.Dirs(iterPos));
          }

          int length = map[iterPos.X, iterPos.Y] - map[pos.X, pos.Y];
          if (gmap.Width * gmap.Height != map[iterPos.X, iterPos.Y]) {
            transitions.Add(new Transition(iterCell, length, checkpointByOffset(data.CheckPointOffset) == iterPos));
            hasUnknown |= unknown;
          }
        }
      }

      data.Cell.setTransitions(transitions);
    }

    private bool checkToAlternative(int[,] map, TilePos currentPos, TilePos alternativePos) {
      foreach (TileDir dir in gmap.Dirs(alternativePos)) {
        TilePos pos = alternativePos + dir;
        if (pos != currentPos && map[pos.X, pos.Y] < map[alternativePos.X, alternativePos.Y]) {
          return true;
        }
      }

      return false;
    }

    private bool checkToUnknown(TilePos lastPos, TilePos pos, int depth) {
      if (depth <= 0) {
        return false;
      }

      if (gmap.Type(pos) == TileType.Unknown) {
        return true;
      }

      bool allUnknown = true;
      int count = 0;
      foreach (TileDir dir in gmap.Dirs(pos)) {
        if (pos + dir == lastPos) {
          continue;
        }

        allUnknown &= checkToUnknown(pos, pos + dir, depth - 1);
        count++;
      }

      return count > 0 && allUnknown;
    }

    private TilePos checkpointByOffset(int offset) {
      int checkPointIndex = (car.NextWaypointIndex + offset + gmap.WayPoints.Length) % gmap.WayPoints.Length;
      return new TilePos(gmap.WayPoints[checkPointIndex].X, gmap.WayPoints[checkPointIndex].Y);
    }

    private int[,] getMap(int offset) {
      TilePos checkPoint = checkpointByOffset(offset);
      if (!mapCache.ContainsKey(checkPoint)) {
        TilePos lastPos = 0 == (offset) ? lastCarTilePos : checkpointByOffset(offset - 1);
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

        if (visited[pos.X, pos.Y] || gmap.Type(pos) == TileType.Unknown) {
          continue;
        }

        visited[pos.X, pos.Y] = true;

        if (pos == end) {
          result[pos.X, pos.Y] = 0;
          backStack.Enqueue(pos);
        }

        bool findUnknown = false;
        foreach (TileDir dir in gmap.Dirs(pos)) {
          TilePos iterPos = pos + dir;

          if (!visited[iterPos.X, iterPos.Y]) {
            findUnknown |= (gmap.Type(iterPos) == TileType.Unknown);
            stack.Enqueue(iterPos);
          }
        }

        if (findUnknown) {
          backUnknownStack.Enqueue(pos);
        }
      }

      int additionalUnknownWeight = backStack.Count > 0 ? 16 : 3;

      while (backStack.Count + backUnknownStack.Count > 0) {
        while (backStack.Count > 0) {
          TilePos pos = backStack.Dequeue();

          foreach (TileDir dir in gmap.Dirs(pos)) {
            TilePos nextPos = pos + dir;
            if (result[nextPos.X, nextPos.Y] > result[pos.X, pos.Y] + 1) {
              result[nextPos.X, nextPos.Y] = result[pos.X, pos.Y] + 1;
              if (nextPos != begin) {
                backStack.Enqueue(nextPos);
              }
            }
          }
        }

        while (backUnknownStack.Count > 0) {
          TilePos pos = backUnknownStack.Dequeue();

          foreach (TileDir dir in gmap.Dirs(pos)) {
            TilePos nextPos = pos + dir;

            if (gmap.Type(nextPos) == TileType.Unknown) {
              result[nextPos.X, nextPos.Y] = additionalUnknownWeight + (Math.Abs(nextPos.X - end.X) + Math.Abs(nextPos.Y - end.Y));
            }

            result[pos.X, pos.Y] = Math.Min(result[nextPos.X, nextPos.Y] + 1, result[pos.X, pos.Y]);
          }
          backStack.Enqueue(pos);
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
