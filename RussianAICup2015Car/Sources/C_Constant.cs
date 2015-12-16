using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources.Common {
  public static class Constant {
    public static readonly int PathMaxDepth = 8;
    public static readonly double MinBrakeSpeed = 12;

    public static double BonusPriority(Bonus bonus, Car car, bool useDistance) {
      Dictionary<BonusType, double> priority = new Dictionary<BonusType, double> {
        { BonusType.AmmoCrate , Math.Max(10, 70 - 10 * car.ProjectileCount) },
        { BonusType.NitroBoost , Math.Max(10, 80 - 10 * car.NitroChargeCount) },
        { BonusType.OilCanister , Math.Max(10, 50 - 10 * car.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , 150 * (1.0 - car.Durability) }
      };

      if (useDistance) {
        double distance = car.GetDistanceTo(bonus);
        return priority[bonus.Type] - (distance * 0.08);
      }

      return priority[bonus.Type];
    }
  }
}
