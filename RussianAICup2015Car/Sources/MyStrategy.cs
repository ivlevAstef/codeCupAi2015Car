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
      log.Assert(3 == wayPoints.Length);


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
      log.Assert(null != game);

      double nextWaypointX = (point.X + 0.5D) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5D) * game.TrackTileSize;
      return new Point<double>(nextWaypointX, nextWaypointY);
    }

    private int wayIndex(Point<int> way) {
      log.Assert(null != path);

      for (int index = 0; index < path.Length; index++) {
        if (path[index].X == way.X &&  path[index].Y == way.Y) {
          return index;
        }
      }

      log.Assert(false);
      return 0;
    }

    private Point<int>[] wayPointsFrom(Point<int> way, int count) {
      log.Assert(null != way);
      log.Assert(null != path);

      int currentIndex = wayIndex(way);
      log.Assert(currentIndex >= 0);

      List<Point<int>> points = new List<Point<int>>();
      while (count > 0) {
        points.Add(path[currentIndex]);

        currentIndex = (currentIndex + 1) % path.Length;
        --count;
      }

      return points.ToArray();
    }

    private double pixelsToWay(Point<int> way, Point<int> prevWay) {
      log.Assert(null != way);
      log.Assert(null != prevWay);
      log.Assert(null != game);

      double A = way.X - prevWay.X;
      double B = way.Y - prevWay.Y;

      Point<double> prevWayPixels = convert(prevWay);
      double Xcenter = prevWayPixels.X + A * game.TrackTileSize * 0.5;
      double Ycenter = prevWayPixels.Y + B * game.TrackTileSize * 0.5;

      double C = A * Xcenter + B * Ycenter;

      return Math.Abs(A * self.X + B * self.Y - C)/Math.Sqrt(A*A + B*B);
    }

    private double procentToWay(Point<int> way, Point<int> prevWay) {
      log.Assert(null != way);
      log.Assert(null != prevWay);
      log.Assert(null != game);

      return pixelsToWay(way, prevWay) / game.TrackTileSize;
    }

    private Point<int> positionTypeFor(Point<int> way, Point<int> nextWay) {
      Point<int> posType = new Point<int>(nextWay.X - way.X, nextWay.Y - way.Y);
      
      log.Assert(posType.Equals(DirLeft) ||
                 posType.Equals(DirRight) ||
                 posType.Equals(DirUp) ||
                 posType.Equals(DirDown));

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
      log.Assert(null != world);
      log.Assert(world.Waypoints.Length >= 2);

      List<Point<int>> newPath = new List<Point<int>>();

      Point<int> direction = startDirection();

      for (int index = 0; index < world.Waypoints.Length; index++) {
        Point<int> begin = new Point<int>(world.Waypoints[index][0], world.Waypoints[index][1]);
        int nextIndex = (index+1) % world.Waypoints.Length;
        Point<int> end = new Point<int>(world.Waypoints[nextIndex][0], world.Waypoints[nextIndex][1]);

        Tuple<List<Point<int>>, Point<int>> subpath = calculatePath(begin, end, direction);
        log.Assert(null != subpath);

        newPath.AddRange(subpath.Item1);
        direction = subpath.Item2;
      }


      path = newPath.ToArray();
    }

    private Tuple<List<Point<int>>, Point<int>> addIfNeed(Tuple<List<Point<int>>, Point<int>> data, Point<int> point) {
      if (null == data) {
        return null;
      }
      data.Item1.Insert(0, point);
      return data;
    }

    private Tuple<List<Point<int>>, Point<int>> calculatePath(Point<int> current, Point<int> end, Point<int> direction) {
      log.Assert(null != world);

      if (current.X == end.X && current.Y == end.Y) {
        return new Tuple<List<Point<int>>,Point<int>>(new List<Point<int>>(), new Point<int>(direction));
      }

      log.Assert(0 <= current.X && current.X < world.Width);
      log.Assert(0 <= current.Y && current.Y < world.Height);

      switch (world.TilesXY[current.X][current.Y]) {
      case TileType.Empty:
        log.Assert(false);
        return null;
      case TileType.Vertical:
        if (direction.Y > 0) {
          return addIfNeed(calculatePath(current.Add(DirDown), end, DirDown), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirUp), end, DirUp), current);
        }
      case TileType.Horizontal:
        if (direction.X > 0) {
          return addIfNeed(calculatePath(current.Add(DirRight), end, DirRight), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirLeft), end, DirLeft), current);
        }
      case TileType.LeftTopCorner:
        if (direction.X < 0) {
          return addIfNeed(calculatePath(current.Add(DirDown), end, DirDown), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirRight), end, DirRight), current);
        }
      case TileType.RightTopCorner:
        if (direction.X > 0) {
          return addIfNeed(calculatePath(current.Add(DirDown), end, DirDown), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirLeft), end, DirLeft), current);
        }
      case TileType.LeftBottomCorner:
        if (direction.X < 0) {
          return addIfNeed(calculatePath(current.Add(DirUp), end, DirUp), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirRight), end, DirRight), current);
        }
      case TileType.RightBottomCorner:
        if (direction.X > 0) {
          return addIfNeed(calculatePath(current.Add(DirUp), end, DirUp), current);
        } else {
          return addIfNeed(calculatePath(current.Add(DirLeft), end, DirLeft), current);
        }
      case TileType.LeftHeadedT:
        break;
      case TileType.RightHeadedT:
        break;
      case TileType.TopHeadedT:
        break;
      case TileType.BottomHeadedT:
        break;
      case TileType.Crossroads:
        break;
      }

      log.Assert(false);
      return null;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}