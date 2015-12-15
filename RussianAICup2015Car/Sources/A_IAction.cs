using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

using RussianAICup2015Car.Sources.Map;
using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Actions {

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

    AvoidSideHit,

    Shooting,
    OilSpill,
    UseNitro
  };

  public interface IAction {
    void setupEnvironment(Car car, World world, Game game, Path path);

    void setupAdditionalPoints(List<Vector> additionalPoints);

    bool valid();

    void execute(Move move);

    List<ActionType> GetParallelsActions();
  }
}
