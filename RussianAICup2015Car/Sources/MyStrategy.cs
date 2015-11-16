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

    private Path path = null;

    public MyStrategy() {
    }

    public void Move(Car self, World world, Game game, Move move) {
      this.self = self;
      this.world = world;
      this.game = game;

      if (null == path) {
        path = new Path(world, log);        
      }

      if (world.Tick < game.InitialFreezeDurationTicks) {
        return;
      }

      path.update(self, world, game);

      PointInt[] wayPoints = path.wayPoints(self, world, game, 2);
      log.Assert(3 == wayPoints.Length, "incorrect calculate way points.");

      PointInt dirSelfToNext = dirFor(wayPoints[0], wayPoints[1]);
      PointInt dirNextToNextNext = dirFor(wayPoints[1], wayPoints[2]);

      double procent = procentToWay(wayPoints[1], wayPoints[0]);
      double needAngle = 0;

      double speedModule = hypot(self.SpeedX, self.SpeedY);
      double speedNormal = (self.SpeedX * dirSelfToNext.X) + (self.SpeedY * dirSelfToNext.Y);
      double procentIdeal = (speedNormal * 23) / game.TrackTileSize;

      if (procent < procentIdeal) {
        needAngle = self.GetAngleTo(self.X + dirNextToNextNext.X, self.Y + dirNextToNextNext.Y);
        move.EnginePower = speedModule * (1.0f - Math.Abs(needAngle / Math.PI));
      } else {
        needAngle = self.GetAngleTo(self.X + dirSelfToNext.X, self.Y + dirSelfToNext.Y);
        move.EnginePower = 1.0;
      }

      if (dirSelfToNext.X != dirNextToNextNext.X || dirSelfToNext.Y != dirNextToNextNext.Y) {
        if (speedNormal > game.TrackTileSize / 60) {
          move.IsBrake = true;
        }
      }

      needAngle -= self.AngularSpeed;

      move.WheelTurn = (needAngle * 15.0 / Math.PI);

      if (isStraight()) {
        move.IsUseNitro = true;
      }
    }

    private bool isStraight() {
      PointInt[] wayPoints = path.wayPoints(self, world, game, 4);
      log.Assert(5 == wayPoints.Length, "incorrect calculate way points.");

      int moveX = 0;
      int moveY = 0;
      for (int index = 1; index < wayPoints.Length; index++) {
        moveX += Math.Abs(wayPoints[index].X - wayPoints[index - 1].X);
        moveY += Math.Abs(wayPoints[index].Y - wayPoints[index - 1].Y);
      }

      return (0 == moveX) || (0 == moveY);
    }

    private Point<double> convert(PointInt point) {
      log.Assert(null != game, "zero game");

      double nextWaypointX = (point.X + 0.5D) * game.TrackTileSize;
      double nextWaypointY = (point.Y + 0.5D) * game.TrackTileSize;
      return new Point<double>(nextWaypointX, nextWaypointY);
    }

    private double pixelsToWay(PointInt way, PointInt prevWay) {
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

    private double procentToWay(PointInt way, PointInt prevWay) {
      log.Assert(null != way, "zero way");
      log.Assert(null != prevWay, "zero prev way");
      log.Assert(null != game, "zero game");

      return pixelsToWay(way, prevWay) / game.TrackTileSize;
    }

    private PointInt dirFor(PointInt way, PointInt nextWay) {
      PointInt dir = new PointInt(nextWay.X - way.X, nextWay.Y - way.Y);

      log.Assert(dir.Equals(Path.DirLeft) ||
                 dir.Equals(Path.DirRight) ||
                 dir.Equals(Path.DirUp) ||
                 dir.Equals(Path.DirDown), "incorrect dir");

      return dir;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}