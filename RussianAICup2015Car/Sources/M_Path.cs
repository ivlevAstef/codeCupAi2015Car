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

    private PointInt pathPos = null;
    private Cell[] path = null;

    public void SetupEnvironment(Car car, World world, Game game, Map.Cell cell) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.cell = cell;
    }

    public void CalculatePath() {
      if (null == pathPos || !pathPos.Equals(currentPos())) {
        HashSet<PointInt> visited = new HashSet<PointInt>();
        Tuple<List<Cell>, double> path = calculatePath(cell, currentDir(), visited);
        Logger.instance.Assert(null != path.Item1, "Can't find path.");

        this.path = path.Item1.ToArray();
        Logger.instance.Assert(3 <= this.path.Length, "Can't find full path.");

        pathPos = currentPos();
      }
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
      PointInt carPos = currentPos();
      PointInt firstWayPoint = new PointInt(world.Waypoints[0][0], world.Waypoints[0][1]);

      double speed = Math.Abs(car.SpeedX) + Math.Abs(car.SpeedY);

      if (speed < 1 && carPos.Equals(firstWayPoint)) {
        return startDirection(world);
      }

      bool has45 = Math.Abs(Math.Abs(car.SpeedX) - Math.Abs(car.SpeedY)) < speed / 4;
      if (use45 && has45) {
        return new PointInt(Math.Sign(car.SpeedX), Math.Sign(car.SpeedX));
      }

      if (Math.Abs(car.SpeedX) > Math.Abs(car.SpeedY)) {
        return new PointInt(Math.Sign(car.SpeedX), 0);
      } else {
        return new PointInt(0, Math.Sign(car.SpeedY));
      }
    }

    private PointInt startDirection(World world) {
      switch (world.StartingDirection) {
      case Direction.Left:
        return Map.DirLeft;
      case Direction.Right:
        return Map.DirRight;
      case Direction.Up:
        return Map.DirUp;
      case Direction.Down:
        return Map.DirDown;
      }
      return new PointInt(0);
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
          priority += 2.0;
        }
      }

      return priority;
    }

    private double cellPriority(PointInt dir, PointInt cell, PointInt from, int length) {
      double priority = (-length) - 1;

      if (isNextThreePoints(from) && dir.Equals(cell - from)) {
        priority += car.Speed() / 5;
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
