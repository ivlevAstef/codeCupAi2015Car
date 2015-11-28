using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class A_Death : A_BaseAction {
    public override bool valid() {
      return car.Durability == 0;
    }

    public override void execute(Move move) {
      move.IsBrake = true;
      move.WheelTurn = 0;
    }
  }
}
