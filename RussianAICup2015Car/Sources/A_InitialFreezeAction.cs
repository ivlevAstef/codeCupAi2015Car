using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Actions {
  class InitialFreezeAction : BaseAction {
    public override bool valid() {
      return world.Tick < game.InitialFreezeDurationTicks;
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;
    }
  }
}
