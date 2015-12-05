using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;
using System.Collections.Generic;

using RussianAICup2015Car.Sources.Common;
using RussianAICup2015Car.Sources.Map;

namespace RussianAICup2015Car.Sources.Physic {
  public class CollisionDetector {
    public static readonly CollisionDetector instance = new CollisionDetector();

    private Game game;
    private GlobalMap gmap;

    public void SetupEnvironment(Game game, GlobalMap gmap) {
      this.game = game;
      this.gmap = gmap;
    }

    ////////////////////////////////////////////////CIRCLE

    /// <returns>intersect pos</returns>
    public Vector IntersectCircleWithMap(Vector center, double radius) {
      TilePos tilePos = new TilePos(center.X, center.Y);
      HashSet<TileDir> dirs = gmap.ReverseDirs(tilePos);
      if (null == dirs || 4 == dirs.Count/*undefined or empty*/) {
        return null;
      }

      double sideRadius = game.TrackTileMargin;
      double distanceToSide = game.TrackTileSize * 0.5 - game.TrackTileMargin;
      double minDistanceToSide = distanceToSide - radius;

      Vector tileCenter = new Vector(tilePos.X + 0.5, tilePos.Y + 0.5) * game.TrackTileSize;
      Vector distanceFromCenter = center - tileCenter;

      //circle near the center tile
      if (Math.Abs(distanceFromCenter.X) < minDistanceToSide && Math.Abs(distanceFromCenter.Y) < minDistanceToSide) {
        return null;
      }

      foreach (TileDir dirInt in dirs) {
        Vector dir = new Vector(dirInt.X, dirInt.Y);
        Vector intersectPos = IntersectCircleWithSide(center, radius, dir, tileCenter + dir * distanceToSide);
        if (null != intersectPos) {
          return intersectPos;
        }
      }

      //edge
      if (Math.Abs(distanceFromCenter.X) > minDistanceToSide && Math.Abs(distanceFromCenter.Y) > minDistanceToSide) {
        Vector edge = tileCenter;
        edge.X += Math.Sign(distanceFromCenter.X) * game.TrackTileSize * 0.5;
        edge.Y += Math.Sign(distanceFromCenter.Y) * game.TrackTileSize * 0.5;

        Vector intersectPos = IntersectCircleWithEdge(center, radius, edge, sideRadius);
        if (null != intersectPos) {
          return intersectPos;
        }
      }

      return null;
    }

    public Vector IntersectCircleWithSide(Vector center, double radius, Vector sidePerpInside, Vector sidePos) {
      double distance = (sidePos - center).Dot(sidePerpInside);

      if (distance < radius) {
        return center + sidePerpInside * distance;
      }

      return null;
    }

    public Vector IntersectCircleWithEdge(Vector center, double radius, Vector edge, double sideRadius) {
      Vector distance = edge - center;

      if (distance.Length < radius + sideRadius) {
        return center + distance.Normalize() * radius;
      }

      return null;
    }

    ////////////////////////////////////////////////CAR

    /// <returns>normal outside map</returns>
    public Vector IntersectCarWithMap(Vector carPos, Vector carDir, TileDir[] additionalSide = null) {
      TilePos tilePos = new TilePos(carPos.X, carPos.Y);
      HashSet<TileDir> dirs = gmap.ReverseDirs(tilePos);
      if (null == dirs || 4 == dirs.Count/*undefined or empty*/) {
        return null;
      }

      if (null != additionalSide) {
        dirs.UnionWith(additionalSide);
      }

      double sideRadius = game.TrackTileMargin * 1.05;
      double distanceToSide = game.TrackTileSize * 0.5 - game.TrackTileMargin * 1.05;
      double minDistanceToSide = distanceToSide - game.CarWidth * 0.5;

      Vector center = new Vector(tilePos.X + 0.5, tilePos.Y + 0.5) * game.TrackTileSize;
      Vector distanceFromCenter = carPos - center;

      //car near the center tile
      if (Math.Abs(distanceFromCenter.X) < minDistanceToSide && Math.Abs(distanceFromCenter.Y) < minDistanceToSide) {
        return null;
      }

      foreach (TileDir dirInt in dirs) {
        Vector dir = new Vector(dirInt.X, dirInt.Y);
        if (IntersectCarWithSide(carPos, carDir, dir, center + dir * distanceToSide)) {
          return dir.Negative();
        }
      }
      //edge
      if (Math.Abs(distanceFromCenter.X) > minDistanceToSide && Math.Abs(distanceFromCenter.Y) > minDistanceToSide) {
        Vector edge = center;
        edge.X += Math.Sign(distanceFromCenter.X) * game.TrackTileSize * 0.5;
        edge.Y += Math.Sign(distanceFromCenter.Y) * game.TrackTileSize * 0.5;

        if (IntersectCarWithCircle(carPos, carDir, edge, sideRadius)) {
          return (carPos - edge).Normalize();
        }
      }

      return null;
    }

    public bool IntersectCarWithCircle(Vector pos, Vector dir, Vector center, double radius) {
      Vector TileDir = center - pos;
      double distanceLength = TileDir.Dot(dir);
      double distanceCross = TileDir.Dot(dir.Perpendicular());

      //inside
      if (Math.Abs(distanceLength) < game.CarWidth * 0.5 && Math.Abs(distanceCross) < game.CarHeight * 0.5) {
        return true;
      }

      Vector p1 = (dir * game.CarWidth + dir.Perpendicular() * game.CarHeight) * 0.5;
      Vector p2 = (dir * game.CarWidth + dir.Perpendicular() * -game.CarHeight) * 0.5;
      Vector p3 = (dir * -game.CarWidth + dir.Perpendicular() * -game.CarHeight) * 0.5;
      Vector p4 = (dir * -game.CarWidth + dir.Perpendicular() * game.CarHeight) * 0.5;

      double d = DistanceToLine(center, pos + p1, pos + p2);
      d = Math.Min(d, DistanceToLine(center, pos + p2, pos + p3));
      d = Math.Min(d, DistanceToLine(center, pos + p3, pos + p4));
      d = Math.Min(d, DistanceToLine(center, pos + p4, pos + p1));

      return d < radius;
    }

    public bool IntersectCarWithSide(Vector pos, Vector dir, Vector sidePerpInside, Vector sidePos) {
      Vector p1 = (dir * game.CarWidth + dir.Perpendicular() * game.CarHeight) * 0.5;
      Vector p2 = (dir * game.CarWidth + dir.Perpendicular() * -game.CarHeight) * 0.5;
      Vector p3 = (dir * -game.CarWidth + dir.Perpendicular() * -game.CarHeight) * 0.5;
      Vector p4 = (dir * -game.CarWidth + dir.Perpendicular() * game.CarHeight) * 0.5;

      double p1Sign = (p1 + pos - sidePos).Dot(sidePerpInside);
      double p2Sign = (p2 + pos - sidePos).Dot(sidePerpInside);
      double p3Sign = (p3 + pos - sidePos).Dot(sidePerpInside);
      double p4Sign = (p4 + pos - sidePos).Dot(sidePerpInside);

      return !(p1Sign < 0 && p2Sign < 0 && p3Sign < 0 && p4Sign < 0);
    }

    private double DistanceToLine(Vector point, Vector p1, Vector p2) {
      Vector delta = p2 - p1;
      Vector distanceP1 = point - p1;
      double t = delta.Normalize().Dot(distanceP1);
      t = Math.Max(0, Math.Min(t, 1));

      Vector res = delta * t + p1;
      return (res - point).Length;
    }
  }
}
