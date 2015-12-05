using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class OvertakeMoving : MovingBase {
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
