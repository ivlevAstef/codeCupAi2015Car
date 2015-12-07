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
      public readonly CellTransition Next;
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

    private static readonly int MaxDepthUsePhysic = 2;

    private Car car = null;
    private World world = null;
    private Game game = null;

    private CellTransition transition = null;
    private Cell[] path = null;
    private Cell pathLastCell = null;
    private TileDir beginDir = null;
    private Dictionary<TilePos, double> physicPriorityCache = new Dictionary<TilePos, double>();

    public void SetupEnvironment(Car car, World world, Game game) {
      this.car = car;
      this.world = world;
      this.game = game;
      physicPriorityCache.Clear();

      if (null == pathLastCell) {
        pathLastCell = startLastCell();
      }
    }

    public void CalculatePath(LiMap.Cell firstCellWithTransition) {
      Logger.instance.Assert(null != pathLastCell, "Don't set last cell. Please call SetupEnvironment");

      if (null != transition && !transition.Cell.Pos.Equals(firstCellWithTransition.Pos)) {
        pathLastCell = transition.Cell;
      }

      beginDir = new TilePos(car.X, car.Y) - pathLastCell.Pos;

      HashSet<LiMap.Cell> visited = new HashSet<LiMap.Cell>();
      transition = calculatePath(firstCellWithTransition, pathLastCell, visited, 0);

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
          newTransition.TransitionPriority = cellTransitionPriority(lastCell, resultCell, newTransition.Cell, transition.Weight, depth < MaxDepthUsePhysic);

          if (null == max || newTransition.Priority(3) > max.Priority(3)) {
            max = newTransition;
          }
        }
      }

      List<TileDir> dirOuts = new List<TileDir>();
      foreach(TileDir dir in cell.Dirs) {
        if (dir == resultCell.DirIn.Negative()) {
          dirOuts.Add(dir);
        }
      }
      resultCell.DirOuts = dirOuts.ToArray();

      if (null != max) {
        resultCell.DirOut = max.Cell.Pos - cell.Pos;
      } else if (1 == dirOuts.Count) {
        resultCell.DirOut = dirOuts[0];
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

    private double cellTransitionPriority(Cell lastCell, Cell cell, Cell nextCell, int length, bool usePhysic) {
      double priority = ((-length) - 1);

      priority += tilePriority(lastCell, cell);
      if (usePhysic && null != nextCell) {
        if (physicPriorityCache.ContainsKey(nextCell.Pos)) {
          priority += physicPriorityCache[nextCell.Pos];
        } else {
          double pPriority = physicPriority(nextCell.Pos);
          priority += physicPriority(nextCell.Pos);
          physicPriorityCache.Add(nextCell.Pos, pPriority);
        }
      }

      return priority;
    }

    private double tilePriority(Cell lastCell, Cell cell) {
      if (null == lastCell) {
        return 0;
      }
      return tilePriority(lastCell.DirIn, cell.Pos - lastCell.Pos, cell.DirIn, cell.DirOut);
    }

    private double tilePriority(TileDir dirIn, TileDir dirOut, TileDir nextDirIn, TileDir nextDirOut) {
      if (dirIn.Negative() == dirOut || nextDirIn.Negative() == nextDirOut) {
        return -10;
      }

      if (dirIn == dirOut) {//line
        return 0.42;
      }

      if (null == nextDirOut || nextDirIn == nextDirOut) {//turn
        return -0.1;
      }

      if (dirIn == nextDirOut.Negative() && dirOut == nextDirIn) {//around
        return -1.5;
      } else if (dirIn == nextDirOut && dirOut == nextDirIn) {//snake
        return 0.45;
      }

      return 0;
    }

    private double physicPriority(TilePos pos) {
      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);
      physicCar.disableNitro();

      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new MapCrashEvent(null),
        new PassageTileEvent(pos)
      };

      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToTile(pos), pEvents, calculateEventCheckEnd);

      if (pEvents.ComeContaints(PhysicEventType.PassageTile)) {
        Vector dirBegin = new Vector(car.SpeedX, car.SpeedY).Normalize();
        Vector dirEnd = pEvents.GetEvent(PhysicEventType.PassageTile).CarCome.Dir;
        return 0.15 + 0.5 * dirBegin.Dot(dirEnd);
      }

      if (pEvents.ComeContaints(PhysicEventType.MapCrash)) {
        return -3.0;
      }

      return 0;
    }

    private bool calculateEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > 100) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.MapCrash) || pEvents.ComeContaints(PhysicEventType.PassageTile);
    }
  }
}
