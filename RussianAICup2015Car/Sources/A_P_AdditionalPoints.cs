using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Map;

namespace RussianAICup2015Car.Sources.Actions {
  public abstract class AdditionalPoints {
    protected Car car = null;
    protected World world = null;
    protected Game game = null;
    protected Path path = null;

    public void setupEnvironment(Car car, World world, Game game, Path path) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.path = path;
    }
    
    public abstract List<Vector> GetPoints();
  }
}
