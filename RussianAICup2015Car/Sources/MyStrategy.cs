using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Path path = new Path();

    private Dictionary<ActionType, A_IAction> actions = new Dictionary<ActionType, A_IAction> {
      { ActionType.InitialFreeze, new A_InitialFreeze()},
      { ActionType.Death, new A_Death()},

      { ActionType.Forward, new A_M_ForwardAction()},
      { ActionType.Backward, new A_M_BackwardAction()},
      { ActionType.PreTurn, new A_M_PreTurnAction()},
      { ActionType.Turn, new A_M_TurnAction()},
      { ActionType.Snake, new A_M_SnakeAction()},
      { ActionType.Around, new A_M_AroundAction()},
      { ActionType.StuckOut, new A_M_StuckOutAction()},

      { ActionType.MoveToBonus, new A_MoveToBonusAction()},
      //{ ActionType.Overtake, new A_OvertakeAction()},
      //{ ActionType.AvoidSideHit, new ()},
      //{ ActionType.AvoidWindShieldHit, new ()},

      { ActionType.Shooting, new A_ShootingAction()},
      { ActionType.OilSpill, new A_OilSpillAction()},
      { ActionType.UseNitro, new A_UseNitroAction()},
    };

    private ActionType[] baseActions = new ActionType[] {
      ActionType.InitialFreeze,
      ActionType.Death,

      ActionType.StuckOut,
      ActionType.Backward,
      ActionType.Around,
      ActionType.Snake,
      ActionType.Turn,
      ActionType.PreTurn,
      ActionType.MoveToBonus,//moved to dont base
      ActionType.Forward,
    };

    public void Move(Car self, World world, Game game, Move move) {
      path.update(self, world, game);

      foreach (A_IAction action in actions.Values) {
        action.setupEnvironment(self, world, game, path);
      }

      A_IAction callAction = null;

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

      callAction.execute(move);

      foreach (ActionType actionType in callAction.GetParallelsActions()) {
        if (actions[actionType].valid()) {
          actions[actionType].execute(move);
        }
      }

      Logger.instance.Debug("Car Speed: {0:F} Car Angle:{1:F3}", self.Speed(), self.Angle);
    }
  }
}