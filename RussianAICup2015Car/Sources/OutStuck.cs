using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class OutStuck {
    private int ignoreTicks = 10;
    private int zeroSpeedTick = 0;

    private int outStuckTicks = 0;
    private PointDouble outStuckPos = null;

    public void update(Car self) {
      if (0 == self.Durability) {
        ignoreTicks = 15;
        return;
      }

      if (ignoreTicks > 0) {
        ignoreTicks--;
        zeroSpeedTick = 0;
        return;
      }

      double speed = self.SpeedX * self.SpeedX + self.SpeedY * self.SpeedY;
      if (speed < 0.05) {
        zeroSpeedTick++;
      } else {
        zeroSpeedTick = 0;
      }

      if (zeroSpeedTick > 5 && null == outStuckPos) {
        outStuckTicks = 0;
        outStuckPos = new PointDouble(self.X, self.Y);
      }
    }

    public bool needRunOutStuck() {
      return null != outStuckPos;
    }

    public void updateUseOutStuck(Car self, PointInt dir, Game game, Move move) {
      const double maxTicks = 115;
      double needDistance = game.TrackTileSize * 0.5;

      zeroSpeedTick = 0;
      if (self.EnginePower < 0 || outStuckTicks > 10) {
        outStuckTicks++;
      }

      double distanceX = Math.Abs(self.X - outStuckPos.X);
      double distanceY = Math.Abs(self.Y - outStuckPos.Y);
      double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

      if (distance >= needDistance || outStuckTicks > maxTicks) {
        ignoreTicks = 10;
        outStuckPos = null;
        return;
      }

      double timePower = Math.Sin((Math.PI*0.5) * (double)(maxTicks - outStuckTicks) / maxTicks);
      move.EnginePower = -timePower;

      double angle = -self.GetAngleTo(self.X + dir.X, self.Y + dir.Y) * timePower;

      move.WheelTurn = (25 * angle / Math.PI);
    }
  }
}
