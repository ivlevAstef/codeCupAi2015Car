using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources.Actions {
  class UseNitroAction : BaseAction {
    private int IgnoreTeammateTicks = 80;

    public override bool valid() {
      if (car.RemainingNitroCooldownTicks > 0 || car.NitroChargeCount <= 0) {
        return false;
      }

      if (car.RemainingOiledTicks > 0) {
        return false;
      }

      if (isStartedNotActiveTeamMate()) {
        return false;
      }

      if (car.Speed() > 35) {
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

    public bool isStartedNotActiveTeamMate() {
      if (world.Tick > game.InitialFreezeDurationTicks + IgnoreTeammateTicks) {
        return false;
      }

      if (haveTeammate() && CarType.Buggy == car.Type) {
        return true;
      }

      return false;
    }

    public bool haveTeammate() {
      int countTeammate = 0;
      foreach (Car car in world.Cars) {
        if (car.IsTeammate) {
          countTeammate++;
        }
      }

      return countTeammate >= 2;
    }
  }
}
