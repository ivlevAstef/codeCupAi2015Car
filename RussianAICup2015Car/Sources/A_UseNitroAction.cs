using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_UseNitroAction : A_BaseAction {
    public override bool valid() {
      if (car.RemainingNitroCooldownTicks > 0 || car.NitroChargeCount <= 0) {
        return false;
      }

      if (car.Speed() > 30) {
        return false;
      }

      Vector carDir = Vector.sincos(car.Angle);
      Vector carSpeed = new Vector(car.SpeedX, car.SpeedY);
      if (carDir.Cross(carSpeed.Normalize()) > 0.01) {
        return false;
      }

      return true;
    }

    public override void execute(Move move) {
      move.IsUseNitro = true;
    }
  }
}
