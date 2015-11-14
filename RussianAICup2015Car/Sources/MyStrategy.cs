using System;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using RussianAICup2015Car.Sources;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {
    public sealed class MyStrategy : IStrategy {
        private Logger log = new Logger();

        public MyStrategy() {

        }

        public void Move(Car self, World world, Game game, Move move) {

            if (world.Tick < game.InitialFreezeDurationTicks) {
                return;
            }

            double nextWaypointX = (self.NextWaypointX + 0.5D) * game.TrackTileSize;
            double nextWaypointY = (self.NextWaypointY + 0.5D) * game.TrackTileSize;


            double cornerTileOffset = 0.25D * game.TrackTileSize;

            switch (world.TilesXY[self.NextWaypointX][self.NextWaypointY]) {
                case TileType.LeftTopCorner:
                    nextWaypointX += cornerTileOffset;
                    nextWaypointY += cornerTileOffset;
                    break;
                case TileType.RightTopCorner:
                    nextWaypointX -= cornerTileOffset;
                    nextWaypointY += cornerTileOffset;
                    break;
                case TileType.LeftBottomCorner:
                    nextWaypointX += cornerTileOffset;
                    nextWaypointY -= cornerTileOffset;
                    break;
                case TileType.RightBottomCorner:
                    nextWaypointX -= cornerTileOffset;
                    nextWaypointY -= cornerTileOffset;
                    break;
            }

            double angleToWaypoint = self.GetAngleTo(nextWaypointX, nextWaypointY);
            double speedModule = hypot(self.SpeedX, self.SpeedY);

            move.WheelTurn = (angleToWaypoint * 32.0D / Math.PI);
            move.EnginePower = 0.75D;

            if (speedModule * speedModule * Math.Abs(angleToWaypoint) > 2.5D * 2.5D * Math.PI) {
                move.IsBrake = true;
            }

            move.IsUseNitro = true;
        }
        
        
        public static double hypot(double a, double b) {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
    }
}