using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;
using System.Collections.Generic;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
  public sealed class MyStrategy : IStrategy {
    private Path path = new Path();

    private Dictionary<ActionType, A_IAction> actions = new Dictionary<ActionType, A_IAction> {
      { ActionType.InitialFreeze, new A_InitialFreeze()},

      { ActionType.Forward, new A_M_ForwardAction()},
      { ActionType.Backward, new A_M_BackwardAction()},
      { ActionType.PreTurn, new A_M_PreTurnAction()},
      { ActionType.Turn, new A_M_TurnAction()},
      { ActionType.Snake, new A_M_SnakeAction()},
      //{ ActionType.Around, new A_M_AroundAction()},
      { ActionType.StuckOut, new A_M_StuckOutAction()},

      { ActionType.MoveToBonus, new A_MoveToBonusAction()},
      //{ ActionType.Overtake, new A_OvertakeAction()},
      //{ ActionType.AvoidSideHit, new ()},
      //{ ActionType.AvoidWindShieldHit, new ()},

      { ActionType.Shooting, new A_ShootingAction()},
      { ActionType.OilSpill, new A_OilSpillAction()},
      { ActionType.UseNitro, new A_UseNitroAction()},
    };

    public void Move(Car self, World world, Game game, Move move) {
      path.update(self, world, game);

      foreach (A_IAction action in actions.Values) {
        action.setupEnvironment(self, world, game, path);
      }

      HashSet<ActionType> validActions = new HashSet<ActionType>();
      HashSet<ActionType> blockActions = new HashSet<ActionType>();

      foreach (KeyValuePair<ActionType, A_IAction> actionInfo in actions) {
        if (actionInfo.Value.valid()) {
          validActions.Add(actionInfo.Key);
          blockActions.UnionWith(actionInfo.Value.GetBlocks());
        }
      }

      move.WheelTurn = 0;
      move.EnginePower = 0;

      HashSet<ActionType> actionsCall = new HashSet<ActionType>(validActions);
      actionsCall.ExceptWith(blockActions);

      foreach (ActionType actionType in actionsCall) {
        actions[actionType].execute(move);
      }

      actionsCall = new HashSet<ActionType>(validActions);
      actionsCall.IntersectWith(blockActions);

      foreach (ActionType actionType in actionsCall) {
        actions[actionType].blockedBy(blockActions);
      }
    }
  }
}