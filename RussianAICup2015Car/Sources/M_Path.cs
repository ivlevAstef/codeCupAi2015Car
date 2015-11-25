using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public class Path {
    public struct Cell {
      public PointInt Pos;

      public PointInt DirIn;
      public PointInt DirOut;
      public PointInt[] DirOuts;
    };

    private Car car = null;
    private World world = null;
    private Game game = null;
    private Map.Cell cell = new Map.Cell();

    private Cell[] path = null;

    public void SetupEnvironment(Car car, World world, Game game, Map.Cell cell) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.cell = cell;
    }

    public void CalculatePath() {
      List<Cell> newPath = mergePath(cell, 0);
      Logger.instance.Assert(null != newPath, "Can't find path.");

      this.path = newPath.ToArray();
      Logger.instance.Assert(3 <= this.path.Length, "Can't find full path.");
    }

    public int Count { get { return path.Length; } }
    public Cell this[int offset] { get { return Get(offset); } }

    public Cell Get(int offset) {
      Logger.instance.Assert(0 <= offset && offset < path.Length, "Offset out of range.");
      return path[offset];
    }

    private PointInt currentPos() {
      return new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));
    }

    private PointInt currentDir(bool use45 = false) {
      double x = Math.Cos(car.Angle);
      double y = Math.Sin(car.Angle);

      bool has45 = Math.Abs(x - y) < 0.1;
      if (use45 && has45) {
        return new PointInt(Math.Sign(x), Math.Sign(y));
      }

      if (Math.Abs(x) > Math.Abs(y)) {
        return new PointInt(Math.Sign(x), 0);
      } else {
        return new PointInt(0, Math.Sign(y));
      }
    }

    private List<Cell> mergePath(Map.Cell cell, int depth) {
      if (null != this.path && depth > 0 && this.path.Length > depth + 1) {
        foreach(Tuple<Map.Cell,int> neighboring in cell.NeighboringCells) {
          if(this.path[depth + 1].Pos.Equals(neighboring.Item1.Pos)) {
            List<Cell> newPath = mergePath(neighboring.Item1, depth - 1);
            newPath.Insert(0, this.path[depth]);
            return newPath;
          }
        }
      }

      HashSet<PointInt> visited = new HashSet<PointInt>();
      return calculatePath(cell, currentDir(), visited).Item1;
    }

    private Tuple<List<Cell>, double> calculatePath(Map.Cell cell, PointInt DirIn, HashSet<PointInt> visited) {
      if (visited.Contains(cell.Pos)) {
        return null;
      }
      visited.Add(cell.Pos);

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = DirIn;

      Tuple<List<Cell>, double> min = null;

      foreach(Tuple<Map.Cell,int> neighboring in cell.NeighboringCells) {
        PointInt dir = neighboring.Item1.Pos - cell.Pos;
        Tuple<List<Cell>, double> path = calculatePath(neighboring.Item1, dir, visited);
        if (null != path) {
          double priority = path.Item2 + cellPriority(DirIn, neighboring.Item1.Pos, cell.Pos, neighboring.Item2);

          if (null == min || priority > min.Item2) {
            min = new Tuple<List<Cell>, double>(path.Item1, priority);
            resultCell.DirOut = dir;
          }
        }
      }

      List<PointInt> dirOuts = new List<PointInt>();
      foreach(PointInt dir in cell.Dirs) {
        if (!dir.Equals(DirIn)) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      double resultPriority = cellPriority(cell.Pos);

      List<Cell> resultPath = new List<Cell>();
      resultPath.Add(resultCell);
      if (null != min) {
        resultPath.AddRange(min.Item1);
        resultPriority += min.Item2;
      }

      visited.Remove(cell.Pos);

      return new Tuple<List<Cell>, double>(resultPath, resultPriority);
    }

    private double cellPriority(PointInt cell) {
      double priority = 0;

      foreach (Bonus bonus in world.Bonuses) {
        PointInt pos = new PointInt((int)(bonus.X/game.TrackTileSize), (int)(bonus.Y/game.TrackTileSize));
        if (pos.Equals(cell)) {
          priority += 1.0;
        }
      }

      return priority;
    }

    private double cellPriority(PointInt dir, PointInt cell, PointInt from, int length) {
      double priority = (-length) - 1;

      if (dir.Equals(cell - from) && length > 0) {
        priority += 2;// car.Speed() / 5;
      }

      return priority;
    }

    private bool isNextThreePoints(PointInt point) {
      PointInt pos = currentPos();
      PointInt dir = currentDir();

      return point.Equals(pos) || point.Equals(pos + dir) || point.Equals(pos + dir + dir);
    }

  }
}
