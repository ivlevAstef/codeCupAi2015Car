using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_OilSpillAction : A_BaseAction {
    public override bool valid() {
      return false;
    }

    public override void execute(Dictionary<ActionType, bool> valid, Move move) {

    }
  }
}
