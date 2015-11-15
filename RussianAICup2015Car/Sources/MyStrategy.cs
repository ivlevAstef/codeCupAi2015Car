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

      PointInt[] wayPoints = path.wayPoints(3);
      log.Assert(3 == wayPoints.Length, "incorrect calculate way points.");

      PointInt posTypeSelfToNext = positionTypeFor(wayPoints[0], wayPoints[1]);
      PointInt posTypeNextToNextNext = positionTypeFor(wayPoints[1], wayPoints[2]);

      Point<double> idealPoint = null;
      double procent = procentToWay(wayPoints[1], wayPoints[0]);
      if (procent > 1.0) {
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
        move.WheelTurn = (angleToWaypoint * 180.0 / Math.PI);

        if (speedModule > 22) {
          move.IsBrake = true;
        } else {
          move.EnginePower = 1.0;
        }
      } else {
        move.WheelTurn = (angleToWaypoint * 15.0 / Math.PI);
        move.EnginePower = 1.0;
      }


      move.IsUseNitro = true;
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

    private PointInt positionTypeFor(PointInt way, PointInt nextWay) {
      PointInt posType = new PointInt(nextWay.X - way.X, nextWay.Y - way.Y);
      
      log.Assert(posType.Equals(Path.DirLeft) ||
                 posType.Equals(Path.DirRight) ||
                 posType.Equals(Path.DirUp) ||
                 posType.Equals(Path.DirDown), "incorrect pos type");

      return posType;
    }
        
    private static double hypot(double a, double b) {
       return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
    }

  }
}