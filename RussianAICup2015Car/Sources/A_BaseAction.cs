﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {

  public abstract class A_BaseAction : A_IAction {
    protected Car car = null;
    protected World world = null;
    protected Game game = null;
    protected Map path = null;

    public void setupEnvironment(Car car, World world, Game game, Map path) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.path = path;
    }

    public abstract bool valid();

    public abstract void execute(Move move);


    public virtual HashSet<ActionType> GetParallelsActions() { return new HashSet<ActionType>(); }
  }
}
