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
    private Map.Cell cell = null;

    private CellTransition transition = null;
    private Cell[] path = null;
    private Cell lastCell = null;

    public void SetupEnvironment(Car car, World world, Game game, Map.Cell cell) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.cell = cell;
    }

    public void CalculatePath() {
      if (null != transition && !transition.Cell.Pos.Equals(cell.Pos)) {
        lastCell = transition.Cell;
        transition = transition.Next;
      }

      transition = mergePath(lastCell, transition, cell, 3);

      Logger.instance.Assert(null != transition, "Can't find path.");

      path = createPathFromTransition(transition).ToArray();
      Logger.instance.Assert(3 <= path.Length, "Can't find full path.");
    }

    private CellTransition mergePath(Cell lastCell, CellTransition iter, Map.Cell mapCell, int depthCount) {
      if (null != iter && depthCount > 0) {
        if (!iter.Cell.Pos.Equals(mapCell.Pos) && null != iter.Next) {
          foreach (Tuple<Map.Cell, int> neighboring in mapCell.NeighboringCells) {
            if (iter.Next.Cell.Pos.Equals(neighboring.Item1.Pos)) {
              iter.Next = mergePath(iter.Cell, iter.Next, neighboring.Item1, depthCount - 1);
              return iter;
            }
          }
        }
      }

      HashSet<Map.Cell> visited = new HashSet<Map.Cell>();
      if (null != lastCell) {
        PointInt dir = mapCell.Pos - lastCell.Pos;
        return calculatePath(lastCell, mapCell, dir, visited);
      }
      return calculatePath(null, mapCell, currentDir(), visited);
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
      //TODO: supported get real angle for wheelTurn = 0
      double x = Math.Cos(car.Angle + car.AngularSpeed * (car.WheelTurn / game.CarWheelTurnChangePerTick));
      double y = Math.Sin(car.Angle + car.AngularSpeed * (car.WheelTurn / game.CarWheelTurnChangePerTick));

      if (Math.Abs(x) > Math.Abs(y)) {
        return new PointInt(Math.Sign(x), 0);
      } else {
        return new PointInt(0, Math.Sign(y));
      }
    }

    private CellTransition calculatePath(Cell lastCell, Map.Cell cell, PointInt DirIn, HashSet<Map.Cell> visited) {
      if (visited.Contains(cell)) {
        return null;
      }
      visited.Add(cell);

      Cell resultCell = new Cell();
      resultCell.Pos = cell.Pos;
      resultCell.DirIn = DirIn;

      CellTransition max = null;

      foreach(Tuple<Map.Cell,int> neighboring in cell.NeighboringCells) {
        PointInt dir = neighboring.Item1.Pos - cell.Pos;
        resultCell.DirOut = dir;

        CellTransition transition = calculatePath(resultCell, neighboring.Item1, dir, visited);
        if (null != transition) {
          CellTransition last = lastTransition(lastCell, resultCell, this.transition);

          double lastPriority = double.MinValue;
          double currentPriority = double.MinValue;
          if (null != last) {
            lastPriority = last.TransitionPriority;
          }  
          if (null != lastCell) {
            currentPriority = cellTransitionPriority(lastCell, resultCell, neighboring.Item2);
          } 
          
          if(null != last || null != lastCell) {
            transition.TransitionPriority = Math.Max(lastPriority, currentPriority);
          } else {
            transition.TransitionPriority = 0;
          }

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

    private bool equalsCellTransition(Cell cell, CellTransition transition) {
      return null != cell && null != transition && 
         cell.Pos.Equals(transition.Cell.Pos) &&
         cell.DirIn.Equals(transition.Cell.DirIn) &&
         cell.DirOut.Equals(transition.Cell.DirOut);
    }

    private CellTransition lastTransition(Cell lastCell, Cell cell, CellTransition transition) {
      if (null == transition) {
        return null;
      }

      if (equalsCellTransition(lastCell, transition) && equalsCellTransition(cell, transition.Next)) {
        return transition;
      }

      return lastTransition(lastCell, cell, transition.Next);
    }

    private double cellPriority(Cell cell) {
      double priority = 0;

      foreach (Bonus bonus in world.Bonuses) {
        PointInt pos = new PointInt((int)(bonus.X/game.TrackTileSize), (int)(bonus.Y/game.TrackTileSize));
        if (pos.Equals(cell.Pos)) {
          priority += 0.35;
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
      if (dirIn.Negative().Equals(dirOut) || nextDirIn.Negative().Equals(nextDirOut)) {
        return -10;
      }

      if (dirIn.Equals(dirOut)) {//line
        return 0.42;
      }

      if (null == nextDirOut || nextDirIn.Equals(nextDirOut)) {
        return 0;
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
