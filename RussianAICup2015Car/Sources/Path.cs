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

    private const int depthMax = 16 * 16;

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
        foreach (PointInt dir in directionsForTile(world.TilesXY[iterPos.X][iterPos.Y])) {
          PointInt nextPos = iterPos.Add(dir);
          if (path[nextPos.X, nextPos.Y] <= minDepth) {
            min = nextPos;
            minDepth = path[min.X, min.Y];
            if (dir.Equals(carDirection(self, game, world))) {
              minDepth -= 2;
            }
          }
        }

        if (null == min) {
          break;
        }

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
      int[,] path = initPath(world);
      bool success = calculatePath(path, begin, checkPoint, world, 0);
      log.Assert(success, "can't find path to way point");

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
          data[i, j] = int.MaxValue;
        }
      }
      return data;
    }

    private PointInt[] directionsForTile(TileType tile) {
      switch (tile) {
      case TileType.Empty:
        log.Error("Empty tile");
        return new PointInt[0];
      case TileType.Vertical:
        return new PointInt[2] {DirDown, DirUp };
      case TileType.Horizontal:
        return new PointInt[2] { DirRight, DirLeft };
      case TileType.LeftTopCorner:
        return new PointInt[2] { DirDown, DirRight };
      case TileType.RightTopCorner:
        return new PointInt[2] { DirDown, DirLeft };
      case TileType.LeftBottomCorner:
        return new PointInt[2] { DirUp, DirRight };
      case TileType.RightBottomCorner:
        return new PointInt[2] { DirUp, DirLeft };
      case TileType.LeftHeadedT:
        return new PointInt[3] { DirLeft, DirUp, DirDown };
      case TileType.RightHeadedT:
        return new PointInt[3] { DirRight, DirUp, DirDown };
      case TileType.TopHeadedT:
        return new PointInt[3] { DirLeft, DirRight, DirUp };
      case TileType.BottomHeadedT:
        return new PointInt[3] { DirLeft, DirRight, DirDown };
      case TileType.Crossroads:
        return new PointInt[4] { DirLeft, DirRight, DirUp, DirDown };
      default:
        return new PointInt[0];
      }
    }

    private bool calculatePath(int[,] path, PointInt pos, PointInt end, World world, int depth) {
      log.Assert(null != world, "zero world");

      if (path[pos.X, pos.Y] < depthMax) {
        return true;
      }

      if (path[pos.X, pos.Y] == depthMax) {
        return false;
      }

      if (pos.Equals(end) || TileType.Unknown == world.TilesXY[pos.X][pos.Y]) {
        path[pos.X, pos.Y] = 0;
        return true;
      }

      if (depth > 32) {
        path[pos.X, pos.Y] = 0;
        return true;
      }

      log.Assert(0 <= pos.X && pos.X < world.Width, "0 < x < width");
      log.Assert(0 <= pos.Y && pos.Y < world.Height, "0 < y < height");

      PointInt[] directions = directionsForTile(world.TilesXY[pos.X][pos.Y]);

      path[pos.X, pos.Y] = depthMax;

      int min = int.MaxValue;
      foreach (PointInt dirIter in directions) {
        PointInt nextPos = pos.Add(dirIter);

        if (calculatePath(path, nextPos, end, world, depth+1)) {
          min = Math.Min(min, path[nextPos.X, nextPos.Y] + 1);
        }
      }

      path[pos.X, pos.Y] = min;
      return (min < depthMax);
    }

  }
}
