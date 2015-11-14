using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Logger log = new Logger();
    private Car self = null;
    private World world = null;
    private Game game = null;
    private Point<int>[] path = null;

    private static Point<int> DirLeft = new Point<int>(-1, 0);
    private static Point<int> DirRight = new Point<int>(1, 0);
    private static Point<int> DirUp = new Point<int>(0, -1);
    private static Point<int> DirDown = new Point<int>(0, 1);

    public MyStrategy() {
    }

    public void Move(Car self, World world, Game game, Move move) {
      this.self = self;
      this.world = world;
      this.game = game;

      if (null == path) {
        calculatePath();
      }

      if (world.Tick < game.InitialFreezeDurationTicks) {
        return;
      }

      Point<int> currentWay = new Point<int>((int)(self.X / game.TrackTileSize), (int)(self.Y /game.TrackTileSize));
      Point<int>[] wayPoints = wayPointsFrom(currentWay, 3);
      log.Assert(3 == wayPoints.Length, "incorrect calculate way points.");

      Point<int> posTypeSelfToNext = positionTypeFor(wayPoints[0], wayPoints[1]);
      Point<int> posTypeNextToNextNext = positionTypeFor(wayPoints[1], wayPoints[2]);

      Point<double> idealPoint = null;
      double procent = procentToWay(wayPoints[1], wayPoints[0]);
      if (procent > 0.5) {
        idealPoint = convert(wayPoints[0]);
        idealPoint.X += posTypeSelfToNext.X * game.TrackTileSize;
        idealPoint.Y += posTypeSelfToNext.Y * game.TrackTileSize;
        idealPoint.X += posTypeNextToNextNext.X * game.TrackTileSize * 0.5;
        idealPoint.Y += posTypeNextToNextNext.Y * game.TrackTileSize * 0.5;
      } else {
        idealPoint = convert(wayPoints[1]);
        idealPoint.X += posTypeNextToNextNext.X * game.TrackTileSize * 0.5;
        idealPoint.Y += posTypeNextToNextNext.Y * game.TrackTileSize * 0.5;
      }

      double angleToWaypoint = self.GetAngleTo(idealPoint.X, idealPoint.Y);
      double speedModule = hypot(self.SpeedX, self.SpeedY);

      log.Debug("Speed:{0}", speedModule);

      if (posTypeSelfToNext.X != posTypeNextToNextNext.X || posTypeSelfToNext.Y != posTypeNextToNextNext.Y) {
        move.WheelTurn = (angleToWaypoint * 90.0D / Math.PI);

        if (speedModule > 20) {
          move.IsBrake = true;
        } else {
          move.EnginePower = 1.0D;
        }
      } else {
        move.WheelTurn = (angleToWaypoint * 15.0D / Math.PI);
        move.EnginePower = 1.0D;
      }


      move.IsUseNitro = true;
    }

    private Point<double> convert(Point<int> point) {
      log.Assert(null != game, "zero game");

      double nextWaypointX = (point.X + 0.5D) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5D) * game.TrackTileSize;
      return new Point<double>(nextWaypointX, nextWaypointY);
    }

    private int wayIndex(Point<int> way) {
      log.Assert(null != path, "zero path");

      for (int index = 0; index < path.Length; index++) {
        if (path[index].X == way.X &&  path[index].Y == way.Y) {
          return index;
        }
      }

      log.Assert(false, "Didn't find way index");
      return 0;
    }

    private Point<int>[] wayPointsFrom(Point<int> way, int count) {
      log.Assert(null != way, "zero way");
      log.Assert(null != path, "zero path");

      int currentIndex = wayIndex(way);
      log.Assert(currentIndex >= 0, "negative current index");

      List<Point<int>> points = new List<Point<int>>();
      while (count > 0) {
        points.Add(path[currentIndex]);

        currentIndex = (currentIndex + 1) % path.Length;
        --count;
      }

      return points.ToArray();
    }

    private double pixelsToWay(Point<int> way, Point<int> prevWay) {
      log.Assert(null != way, "zero way");
      log.Assert(null != prevWay, "zero prev way");
      log.Assert(null != game, "zero game");

      double A = way.X - prevWay.X;
      double B = way.Y - prevWay.Y;

      Point<double> prevWayPixels = convert(prevWay);
      double Xcenter = prevWayPixels.X + A * game.TrackTileSize * 0.5;
      double Ycenter = prevWayPixels.Y + B * game.TrackTileSize * 0.5;

      double C = A * Xcenter + B * Ycenter;

      return Math.Abs(A * self.X + B * self.Y - C)/Math.Sqrt(A*A + B*B);
    }

    private double procentToWay(Point<int> way, Point<int> prevWay) {
      log.Assert(null != way, "zero way");
      log.Assert(null != prevWay, "zero prev way");
      log.Assert(null != game, "zero game");

      return pixelsToWay(way, prevWay) / game.TrackTileSize;
    }

    private Point<int> positionTypeFor(Point<int> way, Point<int> nextWay) {
      Point<int> posType = new Point<int>(nextWay.X - way.X, nextWay.Y - way.Y);
      
      log.Assert(posType.Equals(DirLeft) ||
                 posType.Equals(DirRight) ||
                 posType.Equals(DirUp) ||
                 posType.Equals(DirDown), "incorrect pos type");

      return posType;
    }

    private Point<int> startDirection() {
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
      return new Point<int>(0);
    }

    private void calculatePath() {
      log.Assert(null != world, "zero world");
      log.Assert(world.Waypoints.Length >= 2, "waypoints length < 2");

      List<Point<int>> newPath = new List<Point<int>>();

      Point<int> direction = startDirection();

      for (int index = 0; index < world.Waypoints.Length; index++) {
        Point<int> begin = new Point<int>(world.Waypoints[index][0], world.Waypoints[index][1]);
        int nextIndex = (index+1) % world.Waypoints.Length;
        Point<int> end = new Point<int>(world.Waypoints[nextIndex][0], world.Waypoints[nextIndex][1]);

        Tuple<List<Point<int>>, Point<int>> subpath = calculatePath(begin, end, direction);
        log.Assert(null != subpath, "zero subpath");

        newPath.AddRange(subpath.Item1);
        direction = subpath.Item2;
      }


      path = newPath.ToArray();
    }

    private Tuple<List<Point<int>>, Point<int>> calculatePath(Point<int> current, Point<int> end, Point<int> direction) {
      log.Assert(null != world, "zero world");

      if (current.X == end.X && current.Y == end.Y) {
        return new Tuple<List<Point<int>>,Point<int>>(new List<Point<int>>(), new Point<int>(direction));
      }

      log.Assert(0 <= current.X && current.X < world.Width, "0 < x < width");
      log.Assert(0 <= current.Y && current.Y < world.Height, "0 < y < height");

      List<Point<int>> directions = new List<Point<int>>();

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
        directions.Add(DirDown);
        break;
      case TileType.BottomHeadedT:
        directions.Add(DirLeft);
        directions.Add(DirUp);
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

      Tuple<List<Point<int>>, Point<int>> result = null;
      foreach (Point<int> dir in directions) {
        Tuple<List<Point<int>>, Point<int>> subpath = calculatePath(current.Add(dir), end, dir);
        if (null != subpath) {
          if (null == result || subpath.Item1.Count < result.Item1.Count) {
            result = subpath;
          }
        }
      }

      if (null != result) {
        result.Item1.Insert(0, current);
        return result;
      }

      log.Error("Can't find path.");
      return null;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}