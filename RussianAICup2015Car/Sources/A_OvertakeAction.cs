using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_OvertakeAction : A_BaseAction {
    public override bool valid() {
      return false;
    }

    public override void execute(Move move) {

    }


    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting,
        ActionType.UseNitro
      };
    }
  }
}
