using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public struct PathCell {
    public PointInt Pos;
    public PointInt DirIn;
    public PointInt DirOut;

    public PathCell(PointInt pos, PointInt dirIn) {
      this.Pos = pos;
      this.DirIn = dirIn;
      this.DirOut = null;
    }

    public PathCell(PathCell cell, PointInt dirOut) {
      this.Pos = cell.Pos;
      this.DirIn = cell.DirIn;
      this.DirOut = dirOut;
    }
  }

  public class Path {
    public static PointInt DirLeft = new PointInt(-1, 0);
    public static PointInt DirRight = new PointInt(1, 0);
    public static PointInt DirUp = new PointInt(0, -1);
    public static PointInt DirDown = new PointInt(0, 1);

    public PathCell[] wayCells {
      get { return lastWayCells; }
    }

    private PathCell[] lastWayCells = null;

    private static Dictionary<TileType, PointInt[]> directionsByTileType = new Dictionary<TileType, PointInt[]> {
      {TileType.Empty , new PointInt[0]},
      {TileType.Vertical , new PointInt[2] {DirDown, DirUp }},
      {TileType.Horizontal , new PointInt[2] { DirRight, DirLeft }},
      {TileType.LeftTopCorner , new PointInt[2] { DirDown, DirRight }},
      {TileType.RightTopCorner , new PointInt[2] { DirDown, DirLeft }},
      {TileType.LeftBottomCorner , new PointInt[2] { DirUp, DirRight }},
      {TileType.RightBottomCorner , new PointInt[2] { DirUp, DirLeft }},
      {TileType.LeftHeadedT , new PointInt[3] { DirLeft, DirUp, DirDown }},
      {TileType.RightHeadedT , new PointInt[3] { DirRight, DirUp, DirDown }},
      {TileType.TopHeadedT , new PointInt[3] { DirLeft, DirRight, DirUp }},
      {TileType.BottomHeadedT , new PointInt[3] { DirLeft, DirRight, DirDown }},
      {TileType.Crossroads , new PointInt[4] { DirLeft, DirRight, DirUp, DirDown }},
      {TileType.Unknown , new PointInt[0]}
    };

    public void update(Car self, World world, Game game) {
      lastWayCells = calculateWayCells(self, world, game, 3);
      Logger.instance.Assert(3 == lastWayCells.Length, "incorrect calculate way cells.");
    }

    public bool isStraight() {
      foreach (PathCell cell in lastWayCells) {
        if (null == cell.DirOut || !cell.DirOut.Equals(cell.DirIn)) {
          return false;
        }
      }

      return true;
    }

    private PathCell[] calculateWayCells(Car self, World world, Game game, int count) {
      PointInt begin = new PointInt((int)(self.X / game.TrackTileSize), (int)(self.Y / game.TrackTileSize));

      int checkPointOffset = 0;
      PointInt checkPoint = checkpointByOffset(self, world, checkPointOffset);
      int[,] path = pathFor(begin, world, checkPoint);

      List<PathCell> cells = new List<PathCell>();
      cells.Add(new PathCell(begin, carDirection(self, game, world)));

      for (int i = 0; i < count; i++) {
        PathCell iter = cells[i];

        while (iter.Pos.Equals(checkPoint)) {
          checkPointOffset++;
          PointInt nextCheckPoint = checkpointByOffset(self, world, checkPointOffset);
          path = pathFor(checkPoint, world, nextCheckPoint);
          checkPoint = nextCheckPoint;
        }

        PointInt min = findMinPoint(iter.Pos, iter.DirIn, world, path);
        if (null == min) {
          break;
        }

        cells.Add(new PathCell(min, new PointInt(min.X - iter.Pos.X, min.Y - iter.Pos.Y)));
      }

      setDirOut(ref cells);
      if (cells.Count > count) {
        cells.RemoveRange(count, cells.Count - count);
      }

      return cells.ToArray();
    }

    private PointInt findMinPoint(PointInt from, PointInt dirIn, World world, int [,] path) {
      PointInt min = null;
      int minDepth = int.MaxValue;

      foreach (PointInt dir in directionsByTileType[world.TilesXY[from.X][from.Y]]) {
        PointInt nextPos = from.Add(dir);
        int depth = (0 == path[nextPos.X, nextPos.Y]) ? -10 : path[nextPos.X, nextPos.Y];//because checkpoint needs all time

        if (dir.Equals(dirIn) && checkToAlternative(world, path, from, nextPos)) {
          depth -= 2;
        }

        if (depth < minDepth) {
          min = nextPos;
          minDepth = depth;
        }
      }

      return min;
    }

    private void setDirOut(ref List<PathCell> cells) {
      for (int i = 1; i < cells.Count; i++) {
        cells[i - 1] = new PathCell(cells[i-1], cells[i].DirIn);
      }
    }

    private bool checkToAlternative(World world, int[,] path, PointInt pos, PointInt newPos) {
      foreach (PointInt nextDir in directionsByTileType[world.TilesXY[newPos.X][newPos.Y]]) {
        PointInt nextPos = newPos.Add(nextDir);
        if (!nextPos.Equals(pos) && path[nextPos.X, nextPos.Y] < path[newPos.X, newPos.Y]) {
          return true;
        }
      }

      return false;
    }

    private PointInt carDirection(Car car, Game game, World world) {
      if (Math.Abs(car.SpeedX) + Math.Abs(car.SpeedY) < 1.0e-3 && world.TickCount < game.InitialFreezeDurationTicks + 10) {
        return startDirection(world);
      }

      if(Math.Abs(car.SpeedX) > Math.Abs(car.SpeedY)) {
        return new PointInt(Math.Sign(car.SpeedX), 0);
      } else {
        return new PointInt(0, Math.Sign(car.SpeedY));
      }
    }

    private PointInt checkpointByOffset(Car self, World world, int offset) {
      int checkPointIndex = (self.NextWaypointIndex + offset) % world.Waypoints.Length;
      return new PointInt(world.Waypoints[checkPointIndex][0], world.Waypoints[checkPointIndex][1]);
    }

    private int[,] pathFor(PointInt begin, World world, PointInt checkPoint) {
      int[,] path = calculatePath(begin, checkPoint, world);
      Logger.instance.Assert(null != path, "can't find path to way point");

      return path;
    }

    private PointInt startDirection(World world) {
      switch (world.StartingDirection) {
      case Direction.Left:
        return DirLeft;
      case Direction.Right:
        return DirRight;
      case Direction.Up:
        return DirUp;
      case Direction.Down:
        return DirDown;
      }
      return new PointInt(0);
    }

    private int[,] initPath(World world) {
      int[,] data = new int[world.Width, world.Height];
      for (int i = 0; i < world.Width; i++) {
        for (int j = 0; j < world.Height; j++) {
          data[i, j] = world.Width * world.Height;
        }
      }
      return data;
    }

    private bool[,] initForward(World world) {
      bool[,] data = new bool[world.Width, world.Height];
      for (int i = 0; i < world.Width; i++) {
        for (int j = 0; j < world.Height; j++) {
          data[i, j] = false;
        }
      }
      return data;
    }

    private int[,] calculatePath(PointInt begin, PointInt end, World world) {
      Logger.instance.Assert(null != world, "zero world");

      int[,] result = initPath(world);
      bool[,] visited = initForward(world);

      Queue<PointInt> backStack = new Queue<PointInt>();

      Queue<PointInt> stack = new Queue<PointInt>();
      stack.Enqueue(begin);

      while (stack.Count > 0) {
        PointInt pos = stack.Dequeue();

        if (visited[pos.X, pos.Y]) {
          continue;
        }

        if (pos.Equals(end) || TileType.Unknown == world.TilesXY[pos.X][pos.Y]) {
          result[pos.X, pos.Y] = Math.Abs(pos.X - end.X) + Math.Abs(pos.Y - end.Y);
          backStack.Enqueue(pos);
        }

        visited[pos.X, pos.Y] = true;
        foreach (PointInt dir in directionsByTileType[world.TilesXY[pos.X][pos.Y]]) {
          stack.Enqueue(pos.Add(dir));
        }
      }

      while (backStack.Count > 0) {
        PointInt pos = backStack.Dequeue();

        foreach (PointInt dir in directionsByTileType[world.TilesXY[pos.X][pos.Y]]) {
          PointInt nextPos = pos.Add(dir);
          if (result[nextPos.X, nextPos.Y] > result[pos.X, pos.Y] + 1) {
            result[nextPos.X, nextPos.Y] = result[pos.X, pos.Y] + 1;
            backStack.Enqueue(nextPos);
          }
        }
      }

      return result;
    }

  }
}
