using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_UseNitroAction : A_BaseAction {
    public override bool valid() {
      bool bigSpeed = car.Speed() > 30;

      return !bigSpeed && car.RemainingNitroCooldownTicks <= 0 && car.NitroChargeCount > 0 && path.isStraight();
    }

    public override void execute(Move move) {
      move.IsUseNitro = true;
    }
  }
}
