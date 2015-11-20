using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class A_InitialFreeze : A_BaseAction {
    public override bool valid() {
      return world.Tick < game.InitialFreezeDurationTicks;
    }

    public override void execute(Move move) {
    }

    public override HashSet<ActionType> GetBlocks() { 
      return new HashSet<ActionType>() { 
        ActionType.MoveToBonus,
        ActionType.StuckOut,
        ActionType.OilSpill,
        ActionType.Shooting,
        ActionType.UseNitro
      };
    }
  }
}
