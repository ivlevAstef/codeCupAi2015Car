using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Map {
  public class Path {
    public class Cell {
      public TilePos Pos;

      public TileDir DirIn;
      public TileDir DirOut;
      public TileDir[] DirOuts;
    };

    private class CellTransition {
      public readonly Cell Cell;
      public readonly CellTransition Next;
      public readonly double CellPriority;
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

      public CellTransition(Cell cell, CellTransition next, double cellPriority) {
        this.Cell = cell;
        this.Next = next;
        this.CellPriority = cellPriority;
      }
    }

    private Car car = null;
    private World world = null;
    private Game game = null;

    private CellTransition transition = null;
    private Cell[] path = null;
    private Cell pathLastCell = null;

    public void SetupEnvironment(Car car, World world, Game game) {
      this.car = car;
      this.world = world;
      this.game = game;

      if (null == pathLastCell) {
        pathLastCell = startLastCell();
      }
    }

    public void CalculatePath(LiMap.Cell firstCellWithTransition) {
      Logger.instance.Assert(null != pathLastCell, "Don't set last cell. Please call SetupEnvironment");

      if (null != transition && !transition.Cell.Pos.Equals(firstCellWithTransition.Pos)) {
        pathLastCell = transition.Cell;
      }

      HashSet<LiMap.Cell> visited = new HashSet<LiMap.Cell>();
      transition = calculatePath(firstCellWithTransition, pathLastCell, visited, 8);

      Logger.instance.Assert(null != transition, "Can't find path.");

      path = createPathFromTransition(transition).ToArray();
      Logger.instance.Assert(3 <= path.Length, "Can't find full path.");
    }

    public int Count { get { return path.Length; } }
    public Cell this[int offset] { get { return Get(offset); } }

    public Cell Get(int offset) {
      Logger.instance.Assert(0 <= offset && offset < path.Length, "Offset out of range.");
      return path[offset];
    }

    private TilePos getNextPos(LiMap.Cell mapCell) {
      LiMap.Transition next = null;
      foreach (LiMap.Transition transition in mapCell.Transitions) {
        if (null == next || transition.Weight < next.Weight) {
          next = transition;
        }
      }

      return next.ToCell.Pos;
    }

    private List<Cell> createPathFromTransition(CellTransition transition) {
      List<Cell> result = new List<Cell>();
      result.Add(transition.Cell);
      if (null != transition.Next) {
        result.AddRange(createPathFromTransition(transition.Next));
      }
      return result;
    }

    private TileDir currentDir() {
      Physic.PCar physicCar = new Physic.PCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);


      if (Math.Abs(physicCar.Dir.X) > Math.Abs(physicCar.Dir.X)) {
        return new TileDir(Math.Sign(physicCar.Dir.X), 0);
      } else {
        return new TileDir(0, Math.Sign(physicCar.Dir.Y));
      }
    }

    private Cell startLastCell() {
      Cell resultCell = new Cell();
      resultCell.DirIn = TileDir.TileDirByDirection(world.StartingDirection);
      resultCell.DirOut = resultCell.DirIn;
      resultCell.DirOuts = new TileDir[] { resultCell.DirOut };
      resultCell.Pos = new TilePos(car.X, car.Y) - resultCell.DirOut;

      return resultCell;
    }

    private CellTransition calculatePath(LiMap.Cell cell, Cell lastCell, HashSet<LiMap.Cell> visited, int depth) {
      if (visited.Contains(cell) || depth <= 0) {
        return null;
      }
      visited.Add(cell);
      depth--;

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = cell.Pos - lastCell.Pos;

      CellTransition max = null;

      foreach(LiMap.Transition transition in cell.Transitions) {
        TileDir dir = transition.ToCell.Pos - cell.Pos;
        resultCell.DirOut = dir;

        CellTransition newTransition = calculatePath(transition.ToCell, resultCell, visited, depth);
        if (null != newTransition) {
          newTransition.TransitionPriority = cellTransitionPriority(lastCell, resultCell, transition.Weight);

          if (null == max || newTransition.Priority > max.Priority) {
            max = newTransition;
          }
        }
      }

      List<TileDir> dirOuts = new List<TileDir>();
      foreach(TileDir dir in cell.Dirs) {
        if (!dir.Equals(resultCell.DirIn.Negative())) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      if (null != max) {
        resultCell.DirOut = max.Cell.Pos - cell.Pos;
      }

      CellTransition result = new CellTransition(resultCell, max, cellPriority(resultCell));
      visited.Remove(cell);

      return result;
    }

    private double cellPriority(Cell cell) {
      double priority = 0;

      foreach (Bonus bonus in world.Bonuses) {
        TilePos pos = new TilePos(bonus.X, bonus.Y);
        if (pos.Equals(cell.Pos)) {
          priority += 0.1;
        }
      }

      int countAccidentCarInCell = 0;
      foreach (Car car in world.Cars) {
        if (car.IsTeammate) {
          continue;
        }

        TilePos carPos = new TilePos(car.X, car.Y);
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

    private double tilePriority(Cell lastCell, Cell cell) {
      if (null == lastCell) {
        return 0;
      }
      return tilePriority(lastCell.DirIn, cell.Pos - lastCell.Pos, cell.DirIn, cell.DirOut);
    }

    private double tilePriority(TileDir dirIn, TileDir dirOut, TileDir nextDirIn, TileDir nextDirOut) {
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
