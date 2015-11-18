using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_StuckOutAction : A_IAction {
    public void setupEnvironment(Car car, World world, Game game, Path path) {

    }

    public bool valid() {
      return false;
    }

    public void execute(Dictionary<ActionType, bool> valid, Move move) {

    }
  }
}
