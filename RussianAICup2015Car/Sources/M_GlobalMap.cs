using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Map {
  public class GlobalMap {
    public static GlobalMap Instance { get { return instance; } }

    public TilePos[] WayPoints { get { return wayPoints; } }
    public int Width { get { return width; } }
    public int Height { get { return height; } }

    private static GlobalMap instance = null;

    private static readonly Dictionary<TileType, HashSet<TileDir>> TileDirsByTileType = new Dictionary<TileType, HashSet<TileDir>> {
      {TileType.Empty , new HashSet<TileDir>()},
      {TileType.Vertical , new HashSet<TileDir> {TileDir.Down, TileDir.Up }},
      {TileType.Horizontal , new HashSet<TileDir> { TileDir.Right, TileDir.Left }},
      {TileType.LeftTopCorner , new HashSet<TileDir> { TileDir.Down, TileDir.Right }},
      {TileType.RightTopCorner , new HashSet<TileDir> { TileDir.Down, TileDir.Left }},
      {TileType.LeftBottomCorner , new HashSet<TileDir> { TileDir.Up, TileDir.Right }},
      {TileType.RightBottomCorner , new HashSet<TileDir> { TileDir.Up, TileDir.Left }},
      {TileType.LeftHeadedT , new HashSet<TileDir> { TileDir.Left, TileDir.Up, TileDir.Down }},
      {TileType.RightHeadedT , new HashSet<TileDir> { TileDir.Right, TileDir.Up, TileDir.Down }},
      {TileType.TopHeadedT , new HashSet<TileDir> { TileDir.Left, TileDir.Right, TileDir.Up }},
      {TileType.BottomHeadedT , new HashSet<TileDir> { TileDir.Left, TileDir.Right, TileDir.Down }},
      {TileType.Crossroads , new HashSet<TileDir> { TileDir.Left, TileDir.Right, TileDir.Up, TileDir.Down }},
      {TileType.Unknown , new HashSet<TileDir>()}
    };

    private World world;
    private Game game;

    private HashSet<TileDir>[,] dirs = null;
    private TileType[,] types = null;
    private TilePos[] wayPoints = null;
    private int width;
    private int height;
    private Dictionary<long, TilePos> carsTilePos = null;

    public static void InstanceInit(World world) {
      if (null == instance) {
        instance = new GlobalMap(world);
      }
    }

    public void SetupEnvironment(World world, Game game) {
      this.world = world;
      this.game = game;
    }

    public void Update() {
      Logger.instance.Assert(null != this.world, "Didn't Setup Environment.");
      Logger.instance.Assert(width == this.world.Width && height == this.world.Height, "Incorrect world size.");

      updateUseWorldTilesData();
    }

    public TileType Type(TilePos pos) {
      return Type(pos.X, pos.Y);
    }

    public TileType Type(int x, int y) {
      if (0 > x || x >= width ||
         0 > y || y >= height) {
        return TileType.Empty;
      }
      return types[x,y];
    }

    public HashSet<TileDir> Dirs(TilePos pos) {
      return Dirs(pos.X, pos.Y);
    }

    public HashSet<TileDir> Dirs(int x, int y) {
      if (0 > x || x >= width ||
          0 > y || y >= height) {
        return new HashSet<TileDir>();
      }
      return dirs[x, y];
    }

    public HashSet<TileDir> ReverseDirs(TilePos pos) {
      return ReverseDirs(pos.X, pos.Y);
    }


    public HashSet<TileDir> ReverseDirs(int x, int y) {
      HashSet<TileDir> result = new HashSet<TileDir> { TileDir.Left, TileDir.Right, TileDir.Up, TileDir.Down };
      result.ExceptWith(Dirs(x, y));
      return result;
    }

    protected GlobalMap(World lWorld) {
      this.width = lWorld.Width;
      this.height = lWorld.Height;

      this.dirs = new HashSet<TileDir>[this.width, this.height];
      this.types = new TileType[this.width, this.height];
      this.carsTilePos = new Dictionary<long, TilePos>();

      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          this.dirs[x, y] = new HashSet<TileDir>();
          this.types[x, y] = TileType.Unknown;
        }
      }

      List<int[]> worldWayPoints = new List<int[]>(lWorld.Waypoints);
      this.wayPoints = worldWayPoints.ConvertAll<TilePos>(new Converter<int[], TilePos>(wayIntArrayToTilePos)).ToArray();
    }

    private static TilePos wayIntArrayToTilePos(int[] points) {
      Logger.instance.Assert(null != points && 2 == points.Length, "Incorrect way point data from world.");
      return new TilePos(points[0], points[1]);
    }

    private void updateUseWorldTilesData() {
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          if (types[x, y] != world.TilesXY[x][y]) {
            Logger.instance.Assert(types[x, y] == TileType.Unknown, "Dynamic Changed map. Incorrect data from server?");
            types[x, y] = world.TilesXY[x][y];
            dirs[x, y] = TileDirsByTileType[types[x, y]];
          } else {
            AddDirsByNeighboring(x, y);
          }
        }
      }
    }

    private void AddDirsByNeighboring(int x, int y) {
      TilePos beginPos = new TilePos(x,y);
      foreach (TileDir dir in TileDirsByTileType[TileType.Crossroads]) {
        TilePos pos = beginPos + dir;

        if (0 <= pos.X && pos.X < width && 0 <= pos.Y && pos.Y < height) {
          foreach (TileDir neightborDir in TileDirsByTileType[types[pos.X, pos.Y]]) {
            if (beginPos == pos + neightborDir) {
              dirs[x, y].Add(dir);
            }
          }
        }
      }
    }
  }
}
