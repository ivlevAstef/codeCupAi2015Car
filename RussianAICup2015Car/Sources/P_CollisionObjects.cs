﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public enum CollisionObjectType {
    Circle,
    Rect,
    Side
  }

  public interface ICollisionObject {
    CollisionObjectType Type { get; }
  }

  public class CollisionCircle : ICollisionObject {
    public readonly Vector Center;
    public readonly double Radius;

    public CollisionCircle(Vector center, double radius) {
      this.Center = center;
      this.Radius = radius;
    }

    public CollisionObjectType Type { get { return CollisionObjectType.Circle; } }
  }

  public class CollisionRect : ICollisionObject {
    public readonly Vector Center;
    public readonly Vector Dir;
    public readonly double Width;
    public readonly double Height;

    public CollisionRect(Vector rectCenter, Vector rectDir, double width, double height) {
      this.Center = rectCenter;
      this.Dir = rectDir;
      this.Width = width;
      this.Height = height;
    }

    public CollisionRect(Car car) {
      this.Center = new Vector(car.X, car.Y);
      this.Dir = Vector.sincos(car.Angle);
      this.Width = car.Width;
      this.Height = car.Height;
    }

    public CollisionObjectType Type { get { return CollisionObjectType.Rect; } }
  }

  public class CollisionSide : ICollisionObject {
    public static double SideWidth = 0;
    private static double tileSize = 0;
    private static double tileMargin = 0;

    public static void SetupEnvironment(Game game) {
      tileSize = game.TrackTileSize;
      tileMargin = game.TrackTileMargin;
      SideWidth = tileMargin;
    }

    public readonly Vector P1;
    public readonly Vector P2;
    public readonly Vector DirOut;

    public CollisionSide(TilePos tilePos, TileDir tileDirOut) {
      Vector center = tilePos.ToVector(0.5, 0.5);
      DirOut = new Vector(tileDirOut.X, tileDirOut.Y);
      Vector centerToDir = center + DirOut * (tileSize * 0.5 - tileMargin);
      this.P1 = centerToDir + DirOut.PerpendicularLeft() * tileSize * 0.5;
      this.P2 = centerToDir + DirOut.PerpendicularRight() * tileSize * 0.5;
    }

    public CollisionObjectType Type { get { return CollisionObjectType.Side; } }
  }

  public class CollisionInfo {
    public readonly ICollisionObject obj1;
    public readonly ICollisionObject obj2;

    public Vector Point { get { return point; } }
    public Vector NormalObj1 { get { return point; } }
    public Vector NormalObj2 { get { return point; } }
    public bool Inside { get { return inside; } }

    private Vector point = null;
    private Vector normalObj1 = null;
    private Vector normalObj2 = null;
    private bool inside = false;

    public CollisionInfo(ICollisionObject obj1, ICollisionObject obj2) {
      this.obj1 = obj1;
      this.obj2 = obj2;
    }

    public void setCollisionData(Vector point, Vector normalObj1, Vector normalObj2) {
      this.point = point;
      this.normalObj1 = normalObj1;
      this.normalObj2 = normalObj2;
      this.inside = false;
    }

    public void setCollisionDataForInside(Vector point) {
      this.point = point;
      this.inside = true;
    }
  }
}
