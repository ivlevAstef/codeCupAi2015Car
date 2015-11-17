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
    private List<int[,]> paths = new List<int[,]>();//checkpoint index, depth[,]

    private const int depthMax = 16 * 16;

    public Path(World world, Logger log) {
      log.Assert(null != world, "zero world");
      log.Assert(world.Waypoints.Length >= 2, "waypoints length < 2");

      this.log = log;

      calculatePath(world);
    }

    public void update(Car self, World world, Game game) {
      PointInt nextWayPoint = new PointInt(self.NextWaypointX, self.NextWaypointY);
    }

    public PointInt[] wayPoints(Car self, World world, Game game, int count) {
      int iterPathIndex = (self.NextWaypointIndex + world.Waypoints.Length - 1) % world.Waypoints.Length;
      PointInt iterCheckPoint = checkPointFor(world, iterPathIndex);

      int[,] path = paths[iterPathIndex];
      log.Assert(null != path, "Can't find path for checkpoint");

      List<PointInt> points = new List<PointInt>();
      points.Add(new PointInt((int)(self.X / game.TrackTileSize), (int)(self.Y / game.TrackTileSize)));

      while (count > 0) {
        PointInt iterPos = points[points.Count-1];

        if (iterPos.Equals(iterCheckPoint)) {
          iterPathIndex = (iterPathIndex + 1) % world.Waypoints.Length;
          iterCheckPoint = checkPointFor(world, iterPathIndex);
          path = paths[iterPathIndex];
        }

        PointInt min = null;
        foreach (PointInt dir in directionsForTile(world.TilesXY[iterPos.X][iterPos.Y])) {
          PointInt nextPos = iterPos.Add(dir);
          if (null == min || path[nextPos.X, nextPos.Y] < path[min.X, min.Y]) {
            min = nextPos;
          }
        }

        log.Assert(null != min, "Can't find next way for tile");

        points.Add(min);
        count--;
      }

      return points.ToArray();
    }

    private PointInt checkPointFor(World world, int pathIndex) {
      log.Assert(null != world, "zero world");

      int checkPointIndex = (pathIndex + 1) % world.Waypoints.Length;
      return new PointInt(world.Waypoints[checkPointIndex][0], world.Waypoints[checkPointIndex][1]);
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

    private void calculatePath(World world) {
      log.Assert(null != world, "zero world");
      log.Assert(world.Waypoints.Length >= 2, "waypoints length < 2");

      for (int index = 0; index < world.Waypoints.Length; index++) {
        PointInt begin = new PointInt(world.Waypoints[index][0], world.Waypoints[index][1]);
        int nextIndex = (index + 1) % world.Waypoints.Length;
        PointInt end = new PointInt(world.Waypoints[nextIndex][0], world.Waypoints[nextIndex][1]);

        paths.Add(initPath(world));
        bool success = calculatePath(paths[paths.Count-1], begin, end, world);
        log.Assert(success, "can't find path to way point");
      }
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

    private bool calculatePath(int[,] path, PointInt pos, PointInt end, World world) {
      log.Assert(null != world, "zero world");

      if (path[pos.X, pos.Y] < depthMax) {
        return true;
      }

      if (path[pos.X, pos.Y] == depthMax) {
        return false;
      }

      if (pos.Equals(end)) {
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

        if (calculatePath(path, nextPos, end, world)) {
          min = Math.Min(min, path[nextPos.X, nextPos.Y] + 1);
        }
      }

      path[pos.X, pos.Y] = min;
      return (min < depthMax);
    }

  }
}
