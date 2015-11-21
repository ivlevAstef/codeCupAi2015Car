using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_UseNitroAction : A_BaseAction {
    private PointInt lastPos = null;

    public override bool valid() {
      bool newTile = null != lastPos && lastPos.Equals(path.FirstWayCell.Pos);
      lastPos = path.FirstWayCell.Pos;

      if (car.Speed() > 30) {
        return false;
      }

      if (car.RemainingNitroCooldownTicks > 0 || car.NitroChargeCount <= 0) {
        return false;
      }

      return newTile;
    }

    public override void execute(Move move) {
      move.IsUseNitro = true;
    }
  }
}
