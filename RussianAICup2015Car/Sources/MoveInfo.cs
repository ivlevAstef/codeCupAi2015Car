using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class MoveInfo : Move {
    private bool hasEnginePower;
    private bool hasWheelTurn;

    public bool HasEnginePower {
      get { return hasEnginePower; }
      set { hasEnginePower = value; }
    }

    public bool HasWheelTurn {
      get { return hasWheelTurn; }
      set { hasWheelTurn = value; }
    }
  }
}
