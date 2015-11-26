using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  public enum ActionType {
    InitialFreeze,
    Death,

    Forward,
    Backward,
    PreTurn, //moved car to opposite side border turn
    SnakePreEnd,
    Turn,//90 degrees
    Snake,//45 degrees
    Around,//180 degrees
    StuckOut,

    MoveToBonus,
    Overtake,
    AvoidSideHit,
    AvoidWindShieldHit,

    Shooting,
    OilSpill,
    UseNitro
  };

  public interface A_IAction {
    void setupEnvironment(Car car, World world, Game game, Path path);

    bool valid();

    void execute(Move move);

    HashSet<ActionType> GetParallelsActions();
  }
}
