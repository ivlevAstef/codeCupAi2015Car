using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public class Path {
    public class Cell {
      public PointInt Pos;

      public PointInt DirIn;
      public PointInt DirOut;
      public PointInt[] DirOuts;
    };

    private Car car = null;
    private World world = null;
    private Game game = null;
    private Map.Cell cell = null;

    private Cell[] path = null;
    private Cell lastCell = null;

    public void SetupEnvironment(Car car, World world, Game game, Map.Cell cell) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.cell = cell;
    }

    public void CalculatePath() {
      if (null != path && !path[0].Pos.Equals(cell.Pos)) {
        lastCell = path[0];
      }

      HashSet<Map.Cell> visited = new HashSet<Map.Cell>();
      List<Cell> newPath = null;
      if (null != lastCell) {
        PointInt dir = cell.Pos - lastCell.Pos;
        newPath = calculatePath(lastCell, cell, dir, visited).Item1;
      } else {
        newPath = calculatePath(null, cell, currentDir(), visited).Item1;
      }

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

    private PointInt currentDir() {
      //TODO: supported get real angle for wheelTurn = 0
      double x = Math.Cos(car.Angle + car.AngularSpeed);
      double y = Math.Sin(car.Angle + car.AngularSpeed);

      if (Math.Abs(x) > Math.Abs(y)) {
        return new PointInt(Math.Sign(x), 0);
      } else {
        return new PointInt(0, Math.Sign(y));
      }
    }

    private Tuple<List<Cell>, double> calculatePath(Cell lastCell, Map.Cell cell, PointInt DirIn, HashSet<Map.Cell> visited) {
      if (visited.Contains(cell)) {
        return null;
      }
      visited.Add(cell);
      bool isStraight = currentStraight(visited);

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = DirIn;

      Tuple<List<Cell>, double> max = null;
      PointInt maxDir = null;

      foreach(Tuple<Map.Cell,int> neighboring in cell.NeighboringCells) {
        PointInt dir = neighboring.Item1.Pos - cell.Pos;
        resultCell.DirOut = dir;

        Tuple<List<Cell>, double> path = calculatePath(resultCell, neighboring.Item1, dir, visited);
        if (null != path && path.Item1.Count > 0) {
          double priority = path.Item2;
          if (null != lastCell) {
            priority += cellTransitionPriority(lastCell, resultCell, neighboring.Item2, isStraight);
          }

          if (null == max || priority > max.Item2) {
            max = new Tuple<List<Cell>, double>(path.Item1, priority);
            maxDir = dir;
          }
        }
      }

      resultCell.DirOut = maxDir;

      List<PointInt> dirOuts = new List<PointInt>();
      foreach(PointInt dir in cell.Dirs) {
        if (!dir.Equals(DirIn.Negative())) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      double resultPriority = cellPriority(resultCell);

      List<Cell> resultPath = new List<Cell>();
      resultPath.Add(resultCell);
      if (null != max) {
        resultPath.AddRange(max.Item1);
        resultPriority += max.Item2;
      }

      visited.Remove(cell);

      return new Tuple<List<Cell>, double>(resultPath, resultPriority);
    }

    private double cellPriority(Cell cell) {
      double priority = 0;

      foreach (Bonus bonus in world.Bonuses) {
        PointInt pos = new PointInt((int)(bonus.X/game.TrackTileSize), (int)(bonus.Y/game.TrackTileSize));
        if (pos.Equals(cell.Pos)) {
          priority += 0.2;
        }
      }

      return priority;
    }

    private double cellTransitionPriority(Cell lastCell, Cell cell, int length, bool isStraight) {
      double priority = ((-length) - 1)*0.5;

      if (cell.DirIn.Equals(cell.DirOut) && cell.DirIn.Equals(currentDir())) {
        if (isStraight && pointStraight(cell.Pos)) {
          priority += 0.55;
        }

        if (cell.Pos.Equals(currentPos()) && smallAngle()) {
          priority += car.Speed() / 10;
        }
      }

      priority += tilePriority(lastCell, cell);

      return priority;
    }

    private bool smallAngle() {
      double angle = Math.Abs(car.Angle) % (Math.PI / 2);
      double angleReverse = Math.Abs(Math.PI / 2  - angle) % (Math.PI / 2);

      return Math.Min(angle, angleReverse) < Math.PI / 9;
    }

    private bool pointStraight(PointInt pos) {
      if (!smallAngle()) {
        return false;
      }

      PointInt distance = pos - currentPos();
      PointInt dir = currentDir();

      int distanceLength = Math.Abs(distance.X) + Math.Abs(distance.Y);

      return 0 == distanceLength || (Math.Sign(distance.X) == dir.X && Math.Sign(distance.Y) == dir.Y && distanceLength < 4);
    }

     private bool currentStraight(HashSet<Map.Cell> visited) {
      foreach(Map.Cell cell in visited) {
        if (!pointStraight(cell.Pos)) {
          return false;
        }
      }

      return true;
    }

    private double tilePriority(Cell lastCell, Cell cell) {
      return tilePriority(lastCell.DirIn, cell.Pos - lastCell.Pos, cell.DirIn, cell.DirOut);
    }

    private double tilePriority(PointInt dirIn, PointInt dirOut, PointInt nextDirIn, PointInt nextDirOut) {
      if (null == nextDirOut || dirIn.Equals(dirOut) || nextDirIn.Equals(nextDirOut)) {
        return 0;
      }

      if (dirIn.Equals(dirOut.Negative())) {
        return -5;
      }

      if (dirIn.Equals(nextDirOut.Negative()) && dirOut.Equals(nextDirIn)) {//around
        return -2;
      } else if (dirIn.Equals(nextDirOut) && dirOut.Equals(nextDirIn)) {//snake
        return 0.45;
      }

      return 0;
    }
  }
}
