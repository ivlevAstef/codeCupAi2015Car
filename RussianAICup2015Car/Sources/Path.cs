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
    private PointInt[] path = null;

    private int pathIndex = 0;

    public Path(World world, Logger log) {
      this.log = log;

      calculatePath(world);
    }

    public void update(Car self, World world, Game game) {
      PointInt currentWay = new PointInt((int)(self.X / game.TrackTileSize), (int)(self.Y / game.TrackTileSize));
      updateWayIndex(currentWay);
    }

    public PointInt[] wayPoints(int count) {
      log.Assert(null != path, "zero path");

      int index = pathIndex;
      log.Assert(index >= 0, "negative current index");

      List<PointInt> points = new List<PointInt>();
      while (count > 0) {
        points.Add(path[index]);

        index = (index + 1) % path.Length;
        --count;
      }

      return points.ToArray();
    }

    private int wayIndexFor(PointInt way) {
      log.Assert(null != path, "zero path");

      for (int index = 0; index < path.Length; index++) {
        int realIndex = (pathIndex + index) % path.Length;
        if (path[realIndex].X == way.X && path[realIndex].Y == way.Y) {
          return realIndex;
        }
      }

      log.Error("Didn't find way index");
      return pathIndex;
    }

    private void updateWayIndex(PointInt point) {
      pathIndex = wayIndexFor(point);
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

    private void calculatePath(World world) {
      log.Assert(null != world, "zero world");
      log.Assert(world.Waypoints.Length >= 2, "waypoints length < 2");

      List<PointInt> newPath = new List<PointInt>();

      PointInt direction = startDirection(world);

      for (int index = 0; index < world.Waypoints.Length; index++) {
        PointInt begin = new PointInt(world.Waypoints[index][0], world.Waypoints[index][1]);
        int nextIndex = (index + 1) % world.Waypoints.Length;
        PointInt end = new PointInt(world.Waypoints[nextIndex][0], world.Waypoints[nextIndex][1]);

        HashSet<PointInt> visited = new HashSet<PointInt>();

        Tuple<List<PointInt>, PointInt> subpath = calculatePath(world, begin, end, direction, visited);
        log.Assert(null != subpath, "zero subpath");

        newPath.AddRange(subpath.Item1);
        direction = subpath.Item2;
      }


      path = newPath.ToArray();
    }

    private Tuple<List<PointInt>, PointInt> calculatePath(World world, PointInt current, PointInt end, PointInt direction, HashSet<PointInt> visited) {
      log.Assert(null != world, "zero world");

      if (current.X == end.X && current.Y == end.Y) {
        return new Tuple<List<PointInt>, PointInt>(new List<PointInt>(), new PointInt(direction));
      }

      if (visited.Contains(current) || visited.Count > 30) {
        return null;
      }

      log.Assert(0 <= current.X && current.X < world.Width, "0 < x < width");
      log.Assert(0 <= current.Y && current.Y < world.Height, "0 < y < height");

      List<PointInt> directions = new List<PointInt>();

      switch (world.TilesXY[current.X][current.Y]) {
      case TileType.Empty:
        log.Error("Empty tile");
        return null;
      case TileType.Vertical:
        directions.Add(DirDown);
        directions.Add(DirUp);
        break;
      case TileType.Horizontal:
        directions.Add(DirRight);
        directions.Add(DirLeft);
        break;
      case TileType.LeftTopCorner:
        directions.Add(DirDown);
        directions.Add(DirRight);
        break;
      case TileType.RightTopCorner:
        directions.Add(DirDown);
        directions.Add(DirLeft);
        break;
      case TileType.LeftBottomCorner:
        directions.Add(DirUp);
        directions.Add(DirRight);
        break;
      case TileType.RightBottomCorner:
        directions.Add(DirUp);
        directions.Add(DirLeft);
        break;
      case TileType.LeftHeadedT:
        directions.Add(DirLeft);
        directions.Add(DirUp);
        directions.Add(DirDown);
        break;
      case TileType.RightHeadedT:
        directions.Add(DirRight);
        directions.Add(DirUp);
        directions.Add(DirDown);
        break;
      case TileType.TopHeadedT:
        directions.Add(DirLeft);
        directions.Add(DirRight);
        directions.Add(DirUp);
        break;
      case TileType.BottomHeadedT:
        directions.Add(DirLeft);
        directions.Add(DirDown);
        directions.Add(DirRight);
        break;
      case TileType.Crossroads:
        directions.Add(DirLeft);
        directions.Add(DirRight);
        directions.Add(DirUp);
        directions.Add(DirDown);
        break;
      }

      directions.RemoveAll(p => p.X == -direction.X && p.Y == -direction.Y);

      visited.Add(current);

      Tuple<List<PointInt>, PointInt> result = null;
      foreach (PointInt dir in directions) {
        Tuple<List<PointInt>, PointInt> subpath = calculatePath(world, current.Add(dir), end, dir, visited);

        if (null != subpath) {
          if (null == result || subpath.Item1.Count < result.Item1.Count) {
            result = subpath;
          }
        }
      }

      visited.Remove(current);

      if (null != result) {
        result.Item1.Insert(0, current);
        return result;
      }

      return null;
    }

  }
}
