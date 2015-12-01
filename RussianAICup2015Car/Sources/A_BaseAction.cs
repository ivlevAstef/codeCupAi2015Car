using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {

  public abstract class A_BaseAction : A_IAction {
    protected Car car = null;
    protected World world = null;
    protected Game game = null;
    protected Map map = null;
    protected Path path = null;

    public void setupEnvironment(Car car, World world, Game game, Map map, Path path) {
      this.car = car;
      this.world = world;
      this.game = game;
      this.map = map;
      this.path = path;
    }

    public abstract bool valid();

    public abstract void execute(Move move);


    public virtual List<ActionType> GetParallelsActions() { return new List<ActionType>(); }
  }
}
