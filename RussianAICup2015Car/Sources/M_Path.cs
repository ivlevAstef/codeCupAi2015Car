using RussianAICup2015Car.Sources.Common;
using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources.Physic;

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
      public CellTransition Next;
      public readonly double CellPriority;
      public double TransitionPriority { get; set; }

      public double NextPriority(int depth) {
        return (null != Next) ? Next.Priority(depth) : 0;
      }

      public double Priority(int depth) {
        if (depth > 0) {
          return CellPriority + NextPriority(depth - 1) + TransitionPriority;
        } else {
          return CellPriority + TransitionPriority;
        } 
      }

      public CellTransition(Cell cell, CellTransition next, double cellPriority) {
        this.Cell = cell;
        this.Next = next;
        this.CellPriority = cellPriority;
      }
    };

    private Car car = null;
    private World world = null;
    private Game game = null;

    private CellTransition transition = null;
    private Cell[] path = null;
    private Cell pathLastCell = null;
    private TileDir beginDir = null;
    private bool hasUnknown = false;

    public void SetupEnvironment(Car car, World world, Game game) {
      this.car = car;
      this.world = world;
      this.game = game;

      if (null == pathLastCell) {
        pathLastCell = startLastCell();
      }
    }

    public void CalculatePath(LiMap.Cell firstCellWithTransition, bool hasUnknown) {
      Logger.instance.Assert(null != pathLastCell, "Don't set last cell. Please call SetupEnvironment");

      this.hasUnknown = hasUnknown;
      if (null != transition && !transition.Cell.Pos.Equals(firstCellWithTransition.Pos)) {
        pathLastCell = transition.Cell;
        transition = transition.Next;
      }

      beginDir = new TilePos(car.X, car.Y) - pathLastCell.Pos;

      HashSet<LiMap.Cell> visited = new HashSet<LiMap.Cell>();
      int depth = Math.Min(2, (int)(car.Speed()/10));

      transition = mergePath(firstCellWithTransition, pathLastCell, transition, depth);

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

    private CellTransition mergePath(LiMap.Cell mapCell, Cell lastCell, CellTransition iter, int depth) {
      if (null != iter && depth > 0) {
        if (iter.Cell.Pos == mapCell.Pos && null != iter.Next) {
          foreach (LiMap.Transition transition in mapCell.Transitions) {
            if (iter.Next.Cell.Pos.Equals(transition.ToCell.Pos)) {
              iter.Next = mergePath(transition.ToCell, iter.Cell, iter.Next, depth - 1);
              return iter;
            }
          }
        }
      }

      HashSet<LiMap.Cell> visited = new HashSet<LiMap.Cell>();
      return calculatePath(mapCell, lastCell, visited, 0);
    }

    private List<Cell> createPathFromTransition(CellTransition transition) {
      List<Cell> result = new List<Cell>();
      result.Add(transition.Cell);
      if (null != transition.Next) {
        result.AddRange(createPathFromTransition(transition.Next));
      }
      return result;
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
      if (visited.Contains(cell) || depth > Constant.PathMaxDepth) {
        return null;
      }
      visited.Add(cell);

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = cell.Pos - lastCell.Pos;

      CellTransition max = null;

      foreach(LiMap.Transition transition in cell.Transitions) {
        TileDir dir = transition.ToCell.Pos - cell.Pos;
        resultCell.DirOut = dir;

        CellTransition newTransition = calculatePath(transition.ToCell, resultCell, visited, depth + 1);
        if (null != newTransition) {
          newTransition.TransitionPriority = cellTransitionPriority(lastCell, resultCell, newTransition.Cell, transition.Weight);

          int checkDepth = transition.isCheckpoint ? 0 : 3;
          if (null == max || newTransition.Priority(checkDepth) > max.Priority(checkDepth)) {
            max = newTransition;
          }
        }
      }

      List<TileDir> dirOuts = new List<TileDir>();
      foreach(TileDir dir in cell.Dirs) {
        if (dir != resultCell.DirIn.Negative()) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      resultCell.DirOut = null;
      if (0 != dirOuts.Count) {
        if (null != max) {
          resultCell.DirOut = max.Cell.Pos - cell.Pos;
        }
      }

      CellTransition result = new CellTransition(resultCell, max, cellPriority(resultCell));
      visited.Remove(cell);

      return result;
    }

    private double cellPriority(Cell cell) {
      double priority = 0;

      if (!hasUnknown) {
        foreach (Bonus bonus in world.Bonuses) {
          TilePos pos = new TilePos(bonus.X, bonus.Y);
          if (pos.Equals(cell.Pos)) {
            priority += Constant.BonusPriority(bonus, car, false) / 100.0;
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

        priority -= 0.5 * countAccidentCarInCell;
      }

      return priority;
    }

    private double cellTransitionPriority(Cell lastCell, Cell cell, Cell nextCell, int length) {
      double priority = ((-length) - 1);

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
      if (nextDirIn.Negative() == nextDirOut) {
        return -6.5;
      }

      if (dirIn.Negative() == dirOut) {
        return -1;
      }

      if (dirIn == dirOut) {//line
        return 0.42;
      }

      if (null == nextDirOut || nextDirIn == nextDirOut) {//turn
        return -0.6;
      }

      if (dirIn == nextDirOut.Negative() && dirOut == nextDirIn) {//around
        return -1.5;
      } else if (dirIn == nextDirOut && dirOut == nextDirIn) {//snake
        return 0.45;
      }

      return 0;
    }
  }
}
