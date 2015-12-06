using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public static class PhysicExtensions {
    private static Game game = null;

    public static void setupEnvironment(Game lGame) {
      game = lGame;
    }

    public static PCar GetZeroWheelTurnCar(this PCar car) {
      PCar physicCar = new PCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }
  }
}
