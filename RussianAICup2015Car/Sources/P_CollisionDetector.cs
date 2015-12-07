using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;
using System.Collections.Generic;

using RussianAICup2015Car.Sources.Common;
using RussianAICup2015Car.Sources.Map;

using CollisionPair = System.Tuple<RussianAICup2015Car.Sources.Physic.CollisionObjectType, RussianAICup2015Car.Sources.Physic.CollisionObjectType>;

namespace RussianAICup2015Car.Sources.Physic {
  public class CollisionDetectorOld {
    public static readonly CollisionDetectorOld instance = new CollisionDetectorOld();

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
        Vector distanceDir = new Vector(Math.Sign(distanceFromCenter.X), Math.Sign(distanceFromCenter.Y));
        Vector edge = tileCenter + distanceDir * (game.TrackTileSize * 0.5);
        
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
        Vector distanceDir = new Vector(Math.Sign(distanceFromCenter.X), Math.Sign(distanceFromCenter.Y));
        Vector edge = center + distanceDir * (game.TrackTileSize * 0.5);

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


  public static class CollisionDetector {
    private delegate bool ConcreteCollisionChecker(CollisionInfo collision);

    private static Dictionary<CollisionPair, ConcreteCollisionChecker> collisionFunctions =
      new Dictionary<CollisionPair, ConcreteCollisionChecker> {
        { new CollisionPair(CollisionObjectType.Circle, CollisionObjectType.Circle), checkCollisionCircleWithCircle},
        { new CollisionPair(CollisionObjectType.Circle, CollisionObjectType.Rect), checkCollisionCircleWithRect},
        { new CollisionPair(CollisionObjectType.Circle, CollisionObjectType.Side), checkCollisionCircleWithSide},
        { new CollisionPair(CollisionObjectType.Rect, CollisionObjectType.Rect), checkCollisionRectWithRect},
        { new CollisionPair(CollisionObjectType.Rect, CollisionObjectType.Side), checkCollisionRectWithSide},
      };

    public static bool checkCollision(CollisionInfo collisionInfo) {
      CollisionPair pair = new CollisionPair(collisionInfo.obj1.Type, collisionInfo.obj2.Type);

      if (collisionFunctions.ContainsKey(pair)) {
        return collisionFunctions[pair](collisionInfo);
      }

      CollisionPair rPair = new CollisionPair(collisionInfo.obj2.Type, collisionInfo.obj1.Type);

      if (collisionFunctions.ContainsKey(rPair)) {
        CollisionInfo rCollisionInfo = new CollisionInfo(collisionInfo.obj2, collisionInfo.obj1);
        if (collisionFunctions[rPair](rCollisionInfo)) {
          collisionInfo.setCollisionData(rCollisionInfo.Point, rCollisionInfo.NormalObj2, rCollisionInfo.NormalObj1);
          return true;
        }
        return false;
      }

      Logger.instance.Error("Incorrect collision types. Can't found collision function for: {0}, {1}", collisionInfo.obj1.Type, collisionInfo.obj2.Type);
      return false;
    }

    private static bool checkCollisionCircleWithCircle(CollisionInfo collision) {
      CollisionCircle circle1 = collision.obj1 as CollisionCircle;
      CollisionCircle circle2 = collision.obj2 as CollisionCircle;
      Logger.instance.Assert(null != circle1 && null != circle2, "Collision objects incorrect data for CircleWithCircle");

      Vector direction = (circle1.Center - circle2.Center);
      if (direction.Length < circle1.Radius + circle2.Radius) {
        direction = direction.Normalize();
        Vector collisionPoint = circle1.Center - direction * circle1.Radius;

        collision.setCollisionData(collisionPoint, direction, direction.Negative());
        return true;
      }
      return false;
    }

    private static bool checkCollisionCircleWithRect(CollisionInfo collision) {
      CollisionCircle circle = collision.obj1 as CollisionCircle;
      CollisionRect rect = collision.obj2 as CollisionRect;
      Logger.instance.Assert(null != circle && null != rect, "Collision objects incorrect data for CircleWithRect");

      Vector TileDir = circle.Center - rect.Center;
      double distanceLength = TileDir.Dot(rect.Dir);
      double distanceCross = TileDir.Dot(rect.Dir.Perpendicular());

      //inside
      if (Math.Abs(distanceLength) < rect.Width * 0.5 && Math.Abs(distanceCross) < rect.Height * 0.5) {
        collision.setCollisionDataForInside(circle.Center);
        return true;
      }

      Vector[] points = new Vector[4] {
        rect.Center + (rect.Dir * rect.Width + rect.Dir.Perpendicular() * rect.Height) * 0.5,
        rect.Center + (rect.Dir * rect.Width + rect.Dir.Perpendicular() * -rect.Height) * 0.5,
        rect.Center + (rect.Dir * -rect.Width + rect.Dir.Perpendicular() * -rect.Height) * 0.5,
        rect.Center + (rect.Dir * -rect.Width + rect.Dir.Perpendicular() * rect.Height) * 0.5
      };

      Vector[] intersects = new Vector[4] {
        intersectLine(circle.Center, points[0], points[1]),
        intersectLine(circle.Center, points[1], points[2]),
        intersectLine(circle.Center, points[2], points[3]),
        intersectLine(circle.Center, points[3], points[0])
      };

      int minIndex = -1;
      double minDistance = circle.Radius;

      for (int i = 0; i < 4; i++) {
        double distance = (intersects[i] - circle.Center).Length;
        if (distance < minDistance) {
          minIndex = i;
          minDistance = distance;
        }
      }

      if (-1 != minIndex) {
        Vector dir = (points[minIndex] - points[(minIndex + 1) % 4]).Perpendicular().Normalize();
        collision.setCollisionData(intersects[minIndex], dir.Negative(), dir);
        return true;
      }

      return false;
    }

    private static bool checkCollisionCircleWithSide(CollisionInfo collision) {
      CollisionCircle circle = collision.obj1 as CollisionCircle;
      CollisionSide side = collision.obj2 as CollisionSide;
      Logger.instance.Assert(null != circle && null != side, "Collision objects incorrect data for CircleWithSide");


      double distanceToLine = (intersectLine(circle.Center, side.P1, side.P2) - circle.Center).Length;
      double distanceToInfinity = (side.P1 - circle.Center).Dot(side.DirOut);

      if (-circle.Radius - CollisionSide.SideWidth < distanceToInfinity && distanceToInfinity < circle.Radius &&
        distanceToLine <= distanceToInfinity + 1.0e-3) {

        Vector collisionPoint = circle.Center + side.DirOut * distanceToInfinity;
        collision.setCollisionData(collisionPoint, side.DirOut.Negative(), side.DirOut);
      }

      return false;

    }

    private static bool checkCollisionRectWithRect(CollisionInfo collision) {
      CollisionRect rect1 = collision.obj1 as CollisionRect;
      CollisionRect rect2 = collision.obj2 as CollisionRect;
      Logger.instance.Assert(null != rect1 && null != rect2, "Collision objects incorrect data for RectWithRect");

      return false;
    }

    private static bool checkCollisionRectWithSide(CollisionInfo collision) {
      CollisionRect rect = collision.obj1 as CollisionRect;
      CollisionSide side = collision.obj2 as CollisionSide;
      Logger.instance.Assert(null != rect && null != side, "Collision objects incorrect data for RectWithSide");

      Vector[] points = new Vector[4] {
        rect.Center + (rect.Dir * rect.Width + rect.Dir.Perpendicular() * rect.Height) * 0.5,
        rect.Center + (rect.Dir * rect.Width + rect.Dir.Perpendicular() * -rect.Height) * 0.5,
        rect.Center + (rect.Dir * -rect.Width + rect.Dir.Perpendicular() * -rect.Height) * 0.5,
        rect.Center + (rect.Dir * -rect.Width + rect.Dir.Perpendicular() * rect.Height) * 0.5
      };

      Vector[] intersects = new Vector[4] {
        intersectLine(points[0], side.P1, side.P2),
        intersectLine(points[1], side.P1, side.P2),
        intersectLine(points[2], side.P1, side.P2),
        intersectLine(points[3], side.P1, side.P2),
      };

      double[] distances = new double[4] {
        (points[0] - side.P1).Dot(side.DirOut),
        (points[1] - side.P1).Dot(side.DirOut),
        (points[2] - side.P1).Dot(side.DirOut),
        (points[3] - side.P1).Dot(side.DirOut)
      };

      for (int i = 0; i < 4; i++) {
        double distanceToLine = (points[i] - intersects[i]).Length;
        if (-CollisionSide.SideWidth < distances[i] && distances[i] < 0 && distanceToLine <= Math.Abs(distances[i]) + 1.0e-3) {
          collision.setCollisionData(intersects[i], side.DirOut.Negative(), side.DirOut);
          return true;
        }
      }

      return false;
    }


    private static Vector intersectLine(Vector point, Vector p1, Vector p2) {
      Vector delta = p2 - p1;
      Vector distanceP1 = point - p1;
      double t = delta.Normalize().Dot(distanceP1);
      t = Math.Max(0, Math.Min(t, 1));

      return delta * t + p1;
    }
  }
}
