using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_ShootingAction : A_BaseAction {
    public override bool valid() {
      return false;
    }

    public override void execute(Move move) {

    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze }; } }
  }
}
