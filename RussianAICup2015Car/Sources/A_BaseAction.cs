using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {

  public abstract class A_BaseAction : A_IAction {
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

    public abstract bool valid();

    public abstract void execute(Move move);

    public virtual void blockedBy(HashSet<ActionType> actions) { }

    public virtual HashSet<ActionType> blockers { get { return new HashSet<ActionType>(); } }
  }
}
