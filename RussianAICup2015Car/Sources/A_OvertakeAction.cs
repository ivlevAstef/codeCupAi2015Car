using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_OvertakeAction : A_BaseAction {
    public override bool valid() {
      return false;
    }

    public override void execute(Move move) {

    }


    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.UseNitro
      };
    }
  }
}
