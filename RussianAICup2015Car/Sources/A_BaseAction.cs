using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

using RussianAICup2015Car.Sources.Map;
using RussianAICup2015Car.Sources.Common;
using System;

namespace RussianAICup2015Car.Sources.Actions {

  public abstract class BaseAction : IAction {
    protected Car car = null;
    protected World world = null;
    protected Game game = null;
    protected Path path = null;
    protected List<Tuple<Vector, double>> additionalPoints = null;

    public void setupEnvironment(Car car, World world, Game game, Path path) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.path = path;
    }

    public void setupAdditionalPoints(List<Tuple<Vector, double>> additionalPoints) {
      this.additionalPoints = additionalPoints;
    }

    public abstract bool valid();

    public abstract void execute(Move move);

    public virtual List<ActionType> GetParallelsActions() { return new List<ActionType>(); }
  }
}
