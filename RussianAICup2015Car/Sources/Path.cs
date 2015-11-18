using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class Path {
    public static PointInt DirLeft = new PointInt(-1, 0);
    public static PointInt DirRight = new PointInt(1, 0);
    public static PointInt DirUp = new PointInt(0, -1);
    public static PointInt DirDown = new PointInt(0, 1);

    private Logger log = null;

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

    public Path(World world, Logger log) {
      log.Assert(null != world, "zero world");
      log.Assert(world.Waypoints.Length >= 2, "waypoints length < 2");

      this.log = log;
    }


    public PointInt[] wayPoints(Car self, World world, Game game, int count) {
      PointInt begin = new PointInt((int)(self.X / game.TrackTileSize), (int)(self.Y / game.TrackTileSize));

      int checkPointOffset = 0;
      PointInt checkPoint = checkpointByOffset(self, world, checkPointOffset);
      int[,] path = pathFor(begin, world, checkPoint);

      List<PointInt> points = new List<PointInt>();
      points.Add(begin);

      PointInt wayDir = carDirection(self, game, world);

      while (count > 0) {
        PointInt iterPos = points[points.Count-1];

        while (iterPos.Equals(checkPoint)) {
          checkPointOffset++;
          PointInt nextCheckPoint = checkpointByOffset(self, world, checkPointOffset);
          path = pathFor(checkPoint, world, nextCheckPoint);
          checkPoint = nextCheckPoint;
        }

        PointInt min = null;
        int minDepth = int.MaxValue;
        foreach (PointInt dir in directionsByTileType[world.TilesXY[iterPos.X][iterPos.Y]]) {
          PointInt nextPos = iterPos.Add(dir);
          int depth = (0 == path[nextPos.X, nextPos.Y]) ? -10 : path[nextPos.X, nextPos.Y];//because checkpoint needs all time
          depth -= dir.Equals(wayDir) ? 2 : 0;

          if (depth < minDepth) {
            min = nextPos;
            minDepth = depth;
          }
        }

        if (null == min) {
          break;
        }

        wayDir = new PointInt(min.X - iterPos.X, min.Y - iterPos.Y);
        points.Add(min);
        count--;
      }

      return points.ToArray();
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
      log.Assert(null != path, "can't find path to way point");

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
      log.Assert(null != world, "zero world");

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
