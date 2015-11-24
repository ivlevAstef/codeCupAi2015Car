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
      Tuple<List<Cell>, double> path = calculatePath(cell, currentDir());
      Logger.instance.Assert(null != path.Item1, "Can't find path.");

      this.path = path.Item1.ToArray();
    }

    public int Count { get { return path.Length; } }
    public Cell this[int offset] { get { return Get(offset); } }

    public Cell Get(int offset) {
      Logger.instance.Assert(0 <= offset && offset < path.Length, "Offset out of range.");
      return path[offset];
    }

    private PointInt currentDir() {
      PointInt carPos = new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));
      PointInt firstWayPoint = new PointInt(world.Waypoints[0][0], world.Waypoints[0][1]);

      if (Math.Abs(car.SpeedX) + Math.Abs(car.SpeedY) < 1 && carPos.Equals(firstWayPoint)) {
        return startDirection(world);
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

    private Tuple<List<Cell>, double> calculatePath(Map.Cell cell, PointInt DirIn) {
      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = DirIn;

      Tuple<List<Cell>, double> min = null;

      foreach(Tuple<Map.Cell,int> neighboring in cell.NeighboringCells) {
        PointInt dir = neighboring.Item1.Pos - cell.Pos;
        Tuple<List<Cell>, double> path = calculatePath(neighboring.Item1, dir);
        double priority = path.Item2 + cellPriority(DirIn, neighboring.Item1.Pos, cell.Pos, neighboring.Item2);

        if (null == min || priority > min.Item2) {
          min = new Tuple<List<Cell>,double>(path.Item1, priority);
          resultCell.DirOut = dir;
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

      if (dir.Equals(cell - from)) {
        priority += car.Speed() / 5;
      }

      return priority;
    }



  }
}
