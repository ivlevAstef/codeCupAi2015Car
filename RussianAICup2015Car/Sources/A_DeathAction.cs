﻿using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources.Actions {
  class DeathAction : BaseAction {
    public override bool valid() {
      return car.Durability <= 1.0e-9;
    }

    public override void execute(Move move) {
      move.IsBrake = true;
      move.WheelTurn = 0;
    }
  }
}
