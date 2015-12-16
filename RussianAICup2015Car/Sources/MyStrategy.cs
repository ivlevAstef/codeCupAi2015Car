using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

using RussianAICup2015Car.Sources.Common;
using RussianAICup2015Car.Sources.Actions;
using RussianAICup2015Car.Sources.Actions.Moving;
using RussianAICup2015Car.Sources.Map;
using RussianAICup2015Car.Sources.Physic;


namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private LiMap map = new LiMap();
    private Path path = new Path();

    private Dictionary<ActionType, IAction> actions = new Dictionary<ActionType, IAction> {
      { ActionType.InitialFreeze, new InitialFreezeAction()},
      { ActionType.Death, new DeathAction()},

      { ActionType.Forward, new ForwardMoving()},
      { ActionType.Backward, new BackwardMoving()},
      { ActionType.PreTurn, new PreTurnMoving()},
      { ActionType.SnakePreEnd, new SnakePreEndMoving()},
      { ActionType.Turn, new TurnMoving()},
      { ActionType.Snake, new SnakeMoving()},
      { ActionType.Around, new AroundMoving()},
      { ActionType.StuckOut, new StuckOutMoving()},

      { ActionType.AvoidSideHit, new AvoidSideHitMoving()},

      { ActionType.Shooting, new ShootingAction()},
      { ActionType.OilSpill, new OilSpillAction()},
      { ActionType.UseNitro, new UseNitroAction()},
    };

    private List<AdditionalPoints> additionalPointsActions = new List<AdditionalPoints> {
      new BlockCarHitMoving(),
      new BonusMoving(),
      new DodgeCarHitMoving(),
      new AvoidTireMoving(),
    };

    private ActionType[] baseActions = new ActionType[] {
      ActionType.InitialFreeze,
      ActionType.Death,

      ActionType.StuckOut,
      ActionType.Backward,
      ActionType.Around,
      ActionType.Snake,
      ActionType.Turn,
      ActionType.Forward,
    };

    public void Move(Car car, World world, Game game, Move move) {
      setupEnvironments(car, world, game, move);

      CarMovedPath.Instance.Update(car);

      GlobalMap.Instance.Update();
      path.CalculatePath(map.FirstCellWithTransitions(Constant.PathMaxDepth), map.HasUnknown);

      IAction callAction = null;

      foreach (ActionType actionType in baseActions) {
        if (actions[actionType].valid()) {
          callAction = actions[actionType];
          Logger.instance.Debug("Find action: {0} for tick: {1}", actionType, world.Tick);
          break;
        }
      }

      Logger.instance.Assert(null != callAction, String.Format("Can't find action for current state on tick:{0}", world.Tick));

      if (null == callAction) { //just in case for release.
        move.WheelTurn = 1.0;
        return;
      }

      callAction.setupAdditionalPoints(calculateAdditionalPoints());
      callAction.execute(move);

      foreach (ActionType actionType in callAction.GetParallelsActions()) {
        if (actions[actionType].valid()) {
          actions[actionType].execute(move);
        }
      }

      Logger.instance.Debug("Car Speed: {0:F} Car Angle:{1:F3}", car.Speed(), car.Angle);
    }

    private List<Vector> calculateAdditionalPoints() {
      List<Vector> result = new List<Vector>();

      foreach (AdditionalPoints action in additionalPointsActions) {
        List<Vector> points = action.GetPoints();
        if (null != points) {
          result.AddRange(points);
        }
      }

      return result;
    }

    private void setupEnvironments(Car car, World world, Game game, Move move) {
      TilePos.TileSize = game.TrackTileSize;

      GlobalMap.InstanceInit(world);
      GlobalMap.Instance.SetupEnvironment(world, game);

      AngleReachEvent.setupEnvironment(game);

      MoveToAngleFunction.setupEnvironment(world, game);
      
      PhysicExtensions.setupEnvironment(game);
      PhysicEventsCalculator.setupEnvironment(game, world);

      CollisionSide.SetupEnvironment(game);
      CollisionCircle.SetupEnvironment(game);
      CollisionDetector.SetupEnvironment(game, GlobalMap.Instance);

      map.SetupEnvironment(car, GlobalMap.Instance);
      path.SetupEnvironment(car, world, game);

      foreach (IAction action in actions.Values) {
        action.setupEnvironment(car, world, game, path);
      }

      foreach (AdditionalPoints action in additionalPointsActions) {
        action.setupEnvironment(car, world, game, path);
      }
    }
  }
}