using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class PCar {
    private static readonly double dt = 1;
    private const double oilFrictionMult = 0.25;
    private const double oilBrakeFactor = 0.25;

    public double WheelTurn { get { return wheelTurn; } }
    public double EnginePower { get { return enginePower; } }

    public Vector Pos { get { return pos; } }
    public Vector LastPos { get { return lastPos; } }
    public Vector Speed { get { return spd; } }
    public double Assel { get { return carAccel * enginePower; } }
    public Vector Dir { get { return dir; } }
    public double Angle { get { return angle; } }
    public double AngularSpeed { get { return angleSpeed; } }

    public Car Car { get { return car; } }

    private Car car;
    private Game game;

    private double idealWheelTurn;
    private double idealEnginePower;
    private bool brake = false;
    private int nitroTicks = 0;
    private int oilTicks = 0;

    private double wheelTurn;
    private double enginePower;

    private Vector lastPos;
    private Vector pos;
    private Vector spd;
    private Vector dir;
    private double angle;
    private double angleSpeed;

    private readonly double carAccel;
    private readonly double frictionMove;
    private readonly double frictionLenght;
    private readonly double frictionCross;

    private readonly double angleSpeedFactor;
    private readonly double frictionAngle;
    private readonly double frictionMaxAngleSpeed;

    public PCar(Car car, Game game) {
      this.car = car;
      this.game = game;

      carAccel = carAcceleration(car, game, dt);
      frictionMove = Math.Pow(1 - game.CarMovementAirFrictionFactor, dt);
      frictionLenght = game.CarLengthwiseMovementFrictionFactor * dt;
      frictionCross = game.CarCrosswiseMovementFrictionFactor * dt;

      angleSpeedFactor = game.CarAngularSpeedFactor;
      frictionAngle = Math.Pow(1 - game.CarRotationFrictionFactor, dt);
      frictionMaxAngleSpeed = game.CarRotationFrictionFactor * dt;

      init();
    }

    public PCar(PCar physicCar) : this(physicCar.car, physicCar.game) {
      wheelTurn = physicCar.wheelTurn;
      idealWheelTurn = physicCar.idealWheelTurn;
      enginePower = physicCar.enginePower;
      idealEnginePower = physicCar.idealEnginePower;

      pos = physicCar.pos;
      lastPos = physicCar.lastPos;
      spd = physicCar.spd;
      angle = physicCar.angle;
      dir = physicCar.dir;
      angleSpeed = physicCar.angleSpeed;
      nitroTicks = physicCar.nitroTicks;
      oilTicks = physicCar.oilTicks;
      brake = physicCar.brake;
    }

    public void setWheelTurn(double newWheelTurn) {
      idealWheelTurn = limit(newWheelTurn, 1);
    }

    public void setEnginePower(double newEnginePower) {
      idealEnginePower = limit(newEnginePower, 1);
    }

    public void setBrake(bool isBrake) {
      brake = isBrake;
    }

    public void useNitro() {
      nitroTicks = game.NitroDurationTicks;
    }

    public void disableNitro() {
      nitroTicks = 0;
      enginePower = 1.0;
    }

    public void traveledOnOil(OilSlick oil = null) {
      if (oilTicks > 0) {
        return;
      }
      oilTicks = game.MaxOiledStateDurationTicks;
      if (null != oil) {
        oilTicks = Math.Min(oilTicks, oil.RemainingLifetime);
      }
    }

    public void Iteration(int ticks) {
      lastPos = pos;
      for (int i = 0; i < ticks; i++) {
        wheelTurn = signLimitChange(idealWheelTurn, wheelTurn, game.CarWheelTurnChangePerTick);
        enginePower = signLimitChange(idealEnginePower, enginePower, game.CarEnginePowerChangePerTick);

        if (nitroTicks > 0) {
          enginePower = 2;
          nitroTicks--;
        }

        double brakeV = brake ? 1 : 0;
        double frictionMult = 1;
        if (oilTicks > 0) {
          brakeV = brake ? oilBrakeFactor : 0;
          frictionMult = oilFrictionMult;
          oilTicks--;
        }

        Vector accel = dir * (enginePower * (1 - brakeV) * carAccel);

        pos = pos + spd * dt;
        spd = (spd + accel) * frictionMove;

        double lengthFriction = (1 - brakeV) * frictionLenght + brakeV * frictionCross;
        spd = spd - dir * limit(spd.Dot(dir), lengthFriction * frictionMult) - dir.PerpendicularLeft() * limit(spd.Cross(dir), frictionCross * frictionMult);

        double baseAngleSpeed = wheelTurn * game.CarAngularSpeedFactor * spd.Dot(dir);

        angle += angleSpeed * dt;
        angle = angle.AngleNormalize();
        angleSpeed = baseAngleSpeed + (angleSpeed - baseAngleSpeed) * frictionAngle;
        angleSpeed -= limit(angleSpeed - baseAngleSpeed, frictionMaxAngleSpeed);

        dir = Vector.sincos(angle);
      }
    }

    private void init() {
      wheelTurn = car.WheelTurn;
      setWheelTurn(wheelTurn);
      enginePower = car.EnginePower;
      setEnginePower(enginePower);

      pos = new Vector(car.X, car.Y);
      lastPos = new Vector(car.X, car.Y);
      spd = new Vector(car.SpeedX, car.SpeedY);
      angle = car.Angle;
      dir = Vector.sincos(angle);
      angleSpeed = car.AngularSpeed;

      nitroTicks = car.RemainingNitroTicks;
      oilTicks = car.RemainingOiledTicks;
    }

    private double signLimitChange(double need, double value, double add) {
      int sign = Math.Sign(need - value);
      value += sign * add;

      if (sign != Math.Sign(need - value)) {
        value = need;
      }

      return value;
    }

    private double limit(double v, double maxmin) {
      return Extensions.Limit(v, maxmin);
    }

    private static double carAcceleration(Car car, Game game, double dt) {
      switch (car.Type) {
      case CarType.Buggy:
        return game.BuggyEngineForwardPower / game.BuggyMass * dt;
      case CarType.Jeep:
        return game.JeepEngineForwardPower / game.JeepMass * dt;
      }
      return 0;
    }

  }
}
