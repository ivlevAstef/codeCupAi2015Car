using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public enum PhysicEventType {
    PassageLine,
    OutLine,
    PassageTile,
    OutFromTile,
    AngleReach,
    SpeedReach,
    MapCrash,
    ObjectsCrash,
    IntersectOilStick,
  };

  public interface IPhysicEvent {
    PhysicEventType Type { get; }

    IPhysicEvent Copy();

    bool Check(PCar car);
    object InfoForCheck { get; }

    void SetCome(PCar car, int tick, object info = null);
    bool IsCome { get; }
    int TickCome { get; }
    PCar CarCome { get; }
    object infoCome { get; }
  }

  public abstract class PhysicEventBase : IPhysicEvent {
    public abstract PhysicEventType Type { get; }

    public abstract bool Check(PCar car);

    protected object checkInfo = null;
    public object InfoForCheck { get { return checkInfo; } }

    private PCar car = null;
    private int tick = 0;
    private object info = null;

    public abstract IPhysicEvent Copy();

    public void SetCome(PCar car, int tick, object info = null) {
      this.car = car;
      this.tick = tick;
      this.info = info;
    }

    public bool IsCome { get { return null != car; } }
    public int TickCome { get { return tick; } }
    public PCar CarCome { get { return car; } }
    public object infoCome { get { return info; } }
  }

  public static class PhysicEventExtensions {
    public static bool ComeContaints(this HashSet<IPhysicEvent> data, PhysicEventType pType) {
      foreach (IPhysicEvent pEvent in data) {
        if (pEvent.IsCome && pEvent.Type == pType) {
          return true;
        }
      }
      return false;
    }

    public static bool Containts(this HashSet<IPhysicEvent> data, PhysicEventType pType) {
      foreach (IPhysicEvent pEvent in data) {
        if (pEvent.Type == pType) {
          return true;
        }
      }
      return false;
    }

    public static IPhysicEvent GetEvent(this HashSet<IPhysicEvent> data, PhysicEventType pType) {
      foreach (IPhysicEvent pEvent in data) {
        if (pEvent.Type == pType) {
          return pEvent;
        }
      }

      return null;
    }
  }
}
