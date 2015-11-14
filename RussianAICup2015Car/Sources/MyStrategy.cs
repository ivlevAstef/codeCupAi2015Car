using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Logger Log = new Logger();
    private Car Car = null;
    private World World = null;
    private Game Game = null;

    public MyStrategy() {
    }

    public void Move(Car self, World world, Game game, Move move) {
      Car = self;
      World = world;
      Game = game;

      if (world.Tick < game.InitialFreezeDurationTicks) {
          return;
      }

      Point<int>[] wayPoints = wayPointsFromCurrent(3);
      Log.Assert(3 == wayPoints.Length);

      Point<double> nextWayPos = convert(wayPoints[1]);

      double cornerTileOffset = 0.25D * game.TrackTileSize;

      switch (world.TilesXY[self.NextWaypointX][self.NextWaypointY]) {
      case TileType.LeftTopCorner:
        nextWayPos.X += cornerTileOffset;
        nextWayPos.Y += cornerTileOffset;
        break;
      case TileType.RightTopCorner:
        nextWayPos.X -= cornerTileOffset;
        nextWayPos.Y += cornerTileOffset;
        break;
      case TileType.LeftBottomCorner:
        nextWayPos.X += cornerTileOffset;
        nextWayPos.Y -= cornerTileOffset;
        break;
      case TileType.RightBottomCorner:
        nextWayPos.X -= cornerTileOffset;
        nextWayPos.Y -= cornerTileOffset;
        break;
      }

      double angleToWaypoint = self.GetAngleTo(nextWayPos.X, nextWayPos.Y);
      double speedModule = hypot(self.SpeedX, self.SpeedY);

      move.WheelTurn = (angleToWaypoint * 32.0D / Math.PI);
      move.EnginePower = 1.0D;

      if (speedModule * speedModule * Math.Abs(angleToWaypoint) > 3.0D * 3.0D * Math.PI) {
         move.IsBrake = true;
      }

      move.IsUseNitro = true;
    }

    private Point<double> convert(Point<int> point) {
      Log.Assert(null != Game);

      double nextWaypointX = (point.X + 0.5D) * Game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5D) * Game.TrackTileSize;
      return new Point<double>(nextWaypointX, nextWaypointY);
    }

    private Point<int>[] wayPointsFromCurrent(int count) {
      Log.Assert(null != World);
      Log.Assert(null != Car);

      int currentIndex = 0;
      for (currentIndex = 0; currentIndex < World.Waypoints.Length; currentIndex++) {
        int[] point = World.Waypoints[currentIndex];
        Log.Assert(2 == point.Length);

        if (point[0] == Car.NextWaypointX && point[1] == Car.NextWaypointY) {
          currentIndex = (currentIndex + World.Waypoints.Length - 1) % World.Waypoints.Length;
          break;
        }
      }

      Log.Assert(currentIndex >= 0);

      List<Point<int>> points = new List<Point<int>>();
      while (count > 0) {
        int[] point = World.Waypoints[currentIndex];
        Log.Assert(2 == point.Length);

        points.Add(new Point<int>(point[0], point[1]));

        currentIndex = (currentIndex + 1) % World.Waypoints.Length;
        --count;
      }

      return points.ToArray();
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}