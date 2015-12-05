using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Map {
  public class Path {
    public class Cell {
      public PointInt Pos;

      public PointInt DirIn;
      public PointInt DirOut;
      public PointInt[] DirOuts;
    };

    private class CellTransition {
      public Cell Cell {get; set;}
      public CellTransition Next { get; set; }
      public double CellPriority { get; set; }
      public double TransitionPriority { get; set; }

      public double NextPriority {
        get {
          return (null != Next) ? Next.Priority : 0;
        }
      }

      public double Priority {
        get {
          return CellPriority + NextPriority + TransitionPriority;
        }
      }
    }

    private Car car = null;
    private World world = null;
    private Game game = null;
    private LiMap.Cell cell = null;

    private CellTransition transition = null;
    private Cell[] path = null;
    private Cell lastCell = null;
    private PointInt lastDir = null;

    public void SetupEnvironment(Car car, World world, Game game, LiMap.Cell cell) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.cell = cell;
    }

    public void CalculatePath() {
      if (null != transition && !transition.Cell.Pos.Equals(cell.Pos)) {
        lastCell = transition.Cell;

        PointInt nextPos = getNextPos(cell);
        if (nextPos.Equals(lastCell.Pos)) {
          lastCell = null;
          lastDir = nextPos - cell.Pos;
        } else {
          lastDir = cell.Pos - lastCell.Pos;
        }

        transition = transition.Next;
      }

      double speed = car.Speed();
      int mergeCells = Math.Min(3, (int)(speed / 6));//18

      lastDir = lastDir ?? currentDir();
      transition = mergePath(lastCell, lastDir, transition, cell, mergeCells, 0);

      Logger.instance.Assert(null != transition, "Can't find path.");

      path = createPathFromTransition(transition).ToArray();
      Logger.instance.Assert(3 <= path.Length, "Can't find full path.");
    }

    private PointInt getNextPos(LiMap.Cell mapCell) {
      Tuple<LiMap.Cell, int> next = null;
      foreach (Tuple<LiMap.Cell, int> neighboring in mapCell.NeighboringCells) {
        if (null == next || neighboring.Item2 < next.Item2) {
          next = neighboring;
        }
      }

      return next.Item1.Pos;
    }

    private CellTransition mergePath(Cell lastCell, PointInt dir, CellTransition iter, LiMap.Cell mapCell, int depthCount, int depth) {
      if (null != iter && depthCount > 0) {
        if (iter.Cell.Pos.Equals(mapCell.Pos) && null != iter.Next) {
          foreach (Tuple<LiMap.Cell, int> neighboring in mapCell.NeighboringCells) {
            if (iter.Next.Cell.Pos.Equals(neighboring.Item1.Pos)) {
              PointInt nextDir = iter.Next.Cell.Pos - iter.Cell.Pos;
              iter.Next = mergePath(iter.Cell, nextDir, iter.Next, neighboring.Item1, depthCount - 1, depth + 1);
              return iter;
            }
          }
        }
      }

      HashSet<LiMap.Cell> visited = new HashSet<LiMap.Cell>();
      return calculatePath(lastCell, mapCell, dir, visited, 8 - depth);
    }

    public int Count { get { return path.Length; } }
    public Cell this[int offset] { get { return Get(offset); } }

    public Cell Get(int offset) {
      Logger.instance.Assert(0 <= offset && offset < path.Length, "Offset out of range.");
      return path[offset];
    }

    private List<Cell> createPathFromTransition(CellTransition transition) {
      List<Cell> result = new List<Cell>();
      result.Add(transition.Cell);
      if (null != transition.Next) {
        result.AddRange(createPathFromTransition(transition.Next));
      }
      return result;
    }

    private PointInt currentPos() {
      return new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));
    }

    private PointInt currentDir() {
      Physic.PCar physicCar = new Physic.PCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);


      if (Math.Abs(physicCar.Dir.X) > Math.Abs(physicCar.Dir.X)) {
        return new PointInt(Math.Sign(physicCar.Dir.X), 0);
      } else {
        return new PointInt(0, Math.Sign(physicCar.Dir.Y));
      }
    }

    private CellTransition calculatePath(Cell lastCell, LiMap.Cell cell, PointInt DirIn, HashSet<LiMap.Cell> visited, int depth) {
      if (visited.Contains(cell) || depth <= 0) {
        return null;
      }
      visited.Add(cell);
      depth--;

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = DirIn;

      CellTransition max = null;

      foreach(Tuple<LiMap.Cell,int> neighboring in cell.NeighboringCells) {
        PointInt dir = neighboring.Item1.Pos - cell.Pos;
        resultCell.DirOut = dir;

        CellTransition transition = calculatePath(resultCell, neighboring.Item1, dir, visited, depth);
        if (null != transition) {
          transition.TransitionPriority = cellTransitionPriority(lastCell, resultCell, neighboring.Item2);

          if (null == max || transition.Priority > max.Priority) {
            max = transition;
          }
        }
      }

      List<PointInt> dirOuts = new List<PointInt>();
      foreach(PointInt dir in cell.Dirs) {
        if (!dir.Equals(DirIn.Negative())) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      if (null != max) {
        resultCell.DirOut = max.Cell.Pos - cell.Pos;
      }

      CellTransition result = new CellTransition();
      result.Cell = resultCell;
      result.CellPriority = cellPriority(resultCell);
      result.Next = max;

      visited.Remove(cell);

      return result;
    }

    private double cellPriority(Cell cell) {
      double priority = 0;

      foreach (Bonus bonus in world.Bonuses) {
        PointInt pos = new PointInt((int)(bonus.X/game.TrackTileSize), (int)(bonus.Y/game.TrackTileSize));
        if (pos.Equals(cell.Pos)) {
          priority += 0.1;
        }
      }

      int countAccidentCarInCell = 0;
      foreach (Car car in world.Cars) {
        if (car.IsTeammate) {
          continue;
        }

        PointInt carPos = new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));
        if (!carPos.Equals(cell.Pos)) {
          continue;
        }

        if (car.Speed() < 1) {
          countAccidentCarInCell++;
        }
      }

      priority -= countAccidentCarInCell * countAccidentCarInCell;

      return priority;
    }

    private double cellTransitionPriority(Cell lastCell, Cell cell, int length) {
      double priority = ((-length) - 1)*0.5;

      priority += tilePriority(lastCell, cell);

      return priority;
    }

    private bool smallAngle() {
      double angle = Math.Abs(car.AngleForZeroWheelTurn(game)) % (Math.PI / 2);
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

     private bool currentStraight(HashSet<LiMap.Cell> visited) {
      foreach(LiMap.Cell cell in visited) {
        if (!pointStraight(cell.Pos)) {
          return false;
        }
      }

      return true;
    }

    private double tilePriority(Cell lastCell, Cell cell) {
      if (null == lastCell) {
        return 0;
      }
      return tilePriority(lastCell.DirIn, cell.Pos - lastCell.Pos, cell.DirIn, cell.DirOut);
    }

    private double tilePriority(PointInt dirIn, PointInt dirOut, PointInt nextDirIn, PointInt nextDirOut) {
      if (dirIn.Negative().Equals(dirOut) || nextDirIn.Negative().Equals(nextDirOut)) {
        return -8;
      }

      if (dirIn.Equals(dirOut)) {//line
        return 0.42;
      }

      if (null == nextDirOut || nextDirIn.Equals(nextDirOut)) {//turn
        return -0.6;
      }

      if (dirIn.Equals(nextDirOut.Negative()) && dirOut.Equals(nextDirIn)) {//around
        return -5;
      } else if (dirIn.Equals(nextDirOut) && dirOut.Equals(nextDirIn)) {//snake
        return 0.45;
      }

      return 0;
    }
  }
}
