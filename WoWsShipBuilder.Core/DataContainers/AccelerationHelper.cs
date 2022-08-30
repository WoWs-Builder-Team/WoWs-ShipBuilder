using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    private const string CaraccioloId = "PISB107";
    private const double Dt = 0.01;

    /// <summary>
    /// Create <see cref="AccelerationData"/> for the given ship and parameter.
    /// Sources for all the math and behaviours are the following:
    /// <list type="bullet">
    /// <item>
    /// <term>Base info</term>
    /// <description> https://bbs.nga.cn/read.php?tid=26933715</description>
    /// </item>
    /// <item>
    /// <term>Advanced info</term>
    /// <description> https://bbs.nga.cn/read.php?tid=27647748</description>
    /// </item>
    /// <item>
    /// <term>Speed boost info</term>
    /// <description> https://bbs.nga.cn/read.php?tid=31558840</description>
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="shipIndex">Index of the ship.</param>
    /// <param name="hull">Hull module of the ship.</param>
    /// <param name="engine">Engine module of the ship.</param>
    /// <param name="shipClass">Class of the ship.</param>
    /// <param name="speedMultiplier">Multiplier for the base speed to apply.</param>
    /// <param name="engineForwardUpTimeModifiers">Multipliers for the engine forward up time to apply.</param>
    /// <param name="engineBackwardUpTimeModifiers">Multipliers for the engine backwards up time to apply.</param>
    /// <param name="engineForwardForsageMaxSpeedModifier">Multipliers for the forward forsage max speed to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <param name="engineBackwardForsageMaxSpeedModifier">Multipliers the backward forsage max speed to  to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <param name="engineForwardForsagePowerModifier">Multipliers for the forward forsage power to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <param name="engineBackwardForsagePowerModifier">Multipliers for the backward forsage power to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <returns>The data regarding acceleration.</returns>
    public static AccelerationData CalculateAcceleration(
        string shipIndex,
        Hull hull,
        Engine engine,
        ShipClass shipClass,
        double speedMultiplier = 1,
        double engineForwardUpTimeModifiers = 1,
        double engineBackwardUpTimeModifiers = 1,
        double engineForwardForsageMaxSpeedModifier = 1,
        double engineBackwardForsageMaxSpeedModifier = 1,
        double engineForwardForsagePowerModifier = 1,
        double engineBackwardForsagePowerModifier = 1)
    {
        var result = new List<AccelerationPoints>();

        // get starting stats
        var baseShipSpeed = decimal.ToDouble((1 + engine.SpeedCoef) * hull.MaxSpeed);
        var horsepower = decimal.ToDouble(hull.EnginePower);
        int tonnage = hull.Tonnage;
        var fullPowerForwardTime = decimal.ToDouble(engine.ForwardEngineUpTime);
        var fullPowerBackwardTime = decimal.ToDouble(engine.BackwardEngineUpTime);
        var timeConstant = decimal.ToDouble(Constants.TimeScale);
        var forwardEngineForsag = decimal.ToDouble(engine.ForwardEngineForsag);
        var backwardEngineForsag = decimal.ToDouble(engine.BackwardEngineForsag);
        var forwardEngineForsagMaxSpeed = decimal.ToDouble(engine.ForwardEngineForsagMaxSpeed);
        var backwardEngineForsagMaxSpeed = decimal.ToDouble(engine.BackwardEngineForsagMaxSpeed);

        double speed = 0;
        double time = 0;
        double power = 0;

        // throttle goes from -1 to 4, depending on the gear.
        double throttle = 4;

        // calculate other stats
        var maxForwardSpeed = baseShipSpeed * speedMultiplier;
        var maxReverseSpeed = ((baseShipSpeed / 4) + 4.9) * speedMultiplier;

        var powerRatio = horsepower / tonnage;
        var maxPowerForward = Math.Pow(powerRatio, 0.42) * Math.Pow(speedMultiplier, 2);
        var maxPowerBackwards = (maxPowerForward / GetPfToPbRatio(shipClass, shipIndex)) * Math.Pow(speedMultiplier, 2);

        var timeForward = (fullPowerForwardTime / timeConstant) * engineForwardUpTimeModifiers;
        var timeBackward = (fullPowerBackwardTime / timeConstant) * engineBackwardUpTimeModifiers;

        var forsageForward = forwardEngineForsag * engineForwardForsagePowerModifier;
        var forsageBackwards = backwardEngineForsag * engineBackwardForsagePowerModifier;

        var forsageForwardMaxSpeed = forwardEngineForsagMaxSpeed * engineForwardForsageMaxSpeedModifier;
        var forsageBackwardsMaxSpeed = backwardEngineForsagMaxSpeed * engineBackwardForsageMaxSpeedModifier;

        var powerIncreaseForward = Dt * maxPowerForward / timeForward;
        var powerIncreaseBackward = Dt * maxPowerBackwards / timeBackward;

        // begin the pain, aka the math
        int isReversingDirection = 0;

        result.Add(new(speed, time));

        double speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

        // we go forward!
        if (throttle > 0)
        {
            while (speed < speedLimit - 0.05)
            {
                GenerateAccelerationPoints(result, throttle, isReversingDirection, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards);
            }
        }
        else
        {
            while (speed > speedLimit + 0.05)
            {
                GenerateAccelerationPoints(result, throttle, isReversingDirection, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards);
            }
        }

        // set the power to max of the current throttle. This is done because the speed function is asymptotic to MaxSpeed, so we force it to finish slightly earlier.
        power = throttle >= 0 ? maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isReversingDirection) : maxPowerBackwards;
        double timeToFullForward = time;

        // and now we sail for a bit and then stop!
        if (throttle > 0)
        {
            throttle = 0;
            speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

            while (speed > 0)
            {
                GenerateAccelerationPoints(result, throttle, isReversingDirection, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards);
            }
        }
        else
        {
            throttle = 0;
            speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

            while (speed < 0)
            {
                GenerateAccelerationPoints(result, throttle, isReversingDirection, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards, true);
            }
        }

        double timeToFullBackward = time;

        return new(timeToFullForward, timeToFullBackward, result);
    }

    /// <summary>
    /// Returns the ratio between power forward and power backward. General value is 4 for BBs, 3 for cruiser, 2 for destroyers.<br/>
    /// Caracciolo is the only current exception, with a value of 3.
    /// </summary>
    /// <param name="shipClass">Class of the ship.</param>
    /// <param name="shipIndex">Index of the ship.</param>
    /// <returns>The value of the ratio.</returns>
    private static int GetPfToPbRatio(ShipClass shipClass, string shipIndex)
    {
        if (shipIndex.Equals(CaraccioloId))
        {
            return 3;
        }

        return shipClass switch
        {
            ShipClass.Battleship => 4,
            ShipClass.Cruiser => 3,
            ShipClass.Destroyer => 2,
            _ => 4,
        };
    }

    /// <summary>
    /// Calculate the drag, aka water resistance, for the current speed.
    /// </summary>
    /// <param name="speed">The current ship speed.</param>
    /// <param name="maxForwardSpeed">The ship maximum speed forward.</param>
    /// <param name="maxPowerForward">The ship engine maximum power forward.</param>
    /// <param name="maxReverseSpeed">The ship maximum speed backwards.</param>
    /// <param name="maxPowerBackwards">The ship engine maximum power backwards.</param>
    /// <returns>The value of the drag.</returns>
    private static double GetDrag(double speed, double maxForwardSpeed, double maxPowerForward, double maxReverseSpeed, double maxPowerBackwards)
    {
        // drag=@(x) (-x.*abs(x)/(Vmax)^2*PF).*(x>0) + (-x.*abs(x)/(Vmin)^2*PB).*(x<0);
        // first part if x > 0, second if x < 0
        double drag;
        if (speed > 0)
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxForwardSpeed, 2);
            drag = speedRatio * maxPowerForward;
        }
        else
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxReverseSpeed, 2);
            drag = -speedRatio * maxPowerBackwards;
        }

        return drag;
    }

    /// <summary>
    /// Get the speed limit based on the throttle.
    /// </summary>
    /// <param name="throttle">The current throttle.</param>
    /// <param name="maxForwardSpeed">The maximum speed forward.</param>
    /// <param name="maxReverseSpeed">The maximum speed backwards.</param>
    /// <returns>The speed limit.</returns>
    private static double GetSpeedLimit(double throttle, double maxForwardSpeed, double maxReverseSpeed)
    {
        // speed_limit=@(x) x/4*Vmax*(x>=0)-Vmin*(x<0);
        // first part if x > 0, second if x < 0
        double speedLimit;
        if (throttle >= 0)
        {
            speedLimit = throttle / 4 * maxForwardSpeed;
        }
        else
        {
            speedLimit = -maxReverseSpeed;
        }

        return speedLimit;
    }

    /// <summary>
    /// Calculate a single iteration for the acceleration.
    /// </summary>
    /// <param name="pointList">List to which to add the calculated point.</param>
    /// <param name="throttle">Current throttle.</param>
    /// <param name="isReversingDirection">If the ship is reversing direction from forward to backward.</param>
    /// <param name="time">Current time, passed as ref. Will get modified.</param>
    /// <param name="speed">Current speed, passed as ref. Will get modified.</param>
    /// <param name="speedLimit">Current speed limit.</param>
    /// <param name="power">Current power, passed as ref. Will get modified.</param>
    /// <param name="powerIncreaseForward">Increase of power when going forward.</param>
    /// <param name="maxPowerForward">Maximum power forward.</param>
    /// <param name="maxForwardSpeed">Maximum speed forward.</param>
    /// <param name="forsageForwardMaxSpeed">Maximum forsage speed forward.</param>
    /// <param name="forsageForward">Forsage forward.</param>
    /// <param name="powerIncreaseBackward">Increase of power when going backwards.</param>
    /// <param name="maxPowerBackwards">Maximum power backwards.</param>
    /// <param name="maxReverseSpeed">Maximum speed backwards.</param>
    /// <param name="forsageBackwardsMaxSpeed">Maximum forsage speed backwards.</param>
    /// <param name="forsageBackwards">Forsage backwards.</param>
    /// <param name="stoppingFromBackward">If the ship is stopping from backward.</param>
    private static void GenerateAccelerationPoints(
        ICollection<AccelerationPoints> pointList,
        double throttle,
        double isReversingDirection,
        ref double time,
        ref double speed,
        double speedLimit,
        ref double power,
        double powerIncreaseForward,
        double maxPowerForward,
        double maxForwardSpeed,
        double forsageForwardMaxSpeed,
        double forsageForward,
        double powerIncreaseBackward,
        double maxPowerBackwards,
        double maxReverseSpeed,
        double forsageBackwardsMaxSpeed,
        double forsageBackwards,
        bool stoppingFromBackward = false)
    {
        int acc;
        if (speedLimit > speed)
        {
            power = Math.Min(Math.Max(power, 0) + powerIncreaseForward, maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isReversingDirection));
            acc = 1;
        }
        else if (speedLimit < speed)
        {
            power = Math.Max(Math.Min(power, 0) - powerIncreaseBackward, -maxPowerBackwards);
            acc = -1;
        }
        else
        {
            if (speed > 0)
            {
                power = maxPowerForward * Math.Pow(throttle / 4, 2);
            }
            else
            {
                power = -maxPowerBackwards;
            }

            acc = 0;
        }

        double drag = GetDrag(speed, maxForwardSpeed, maxPowerForward, maxReverseSpeed, maxPowerBackwards);
        double acceleration = (power + drag) * Math.Abs(acc);

        // this is needed to avoid getting an oscillating speed when at maxForsageSpeed (due to low sampling rate)
        double previousSpeed = speed;
        speed += Dt * acceleration;

        // forsage part
        var isForsageForward = false;
        var isForsageBackward = false;
        if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
        {
            acceleration = (maxPowerForward * forsageForward) - drag;
            isForsageForward = true;
        }
        else if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
        {
            acceleration = (-maxPowerBackwards * forsageBackwards) + drag;
            isForsageBackward = true;
        }

        if (isForsageBackward || isForsageForward)
        {
            speed = previousSpeed + (Dt * acceleration);
        }

        if (speedLimit < speed && acc == 1 && power * previousSpeed > 0)
        {
            speed = speedLimit;
        }
        else if (speedLimit > speed && acc == -1 && power * previousSpeed > 0)
        {
            speed = speedLimit;
        }
        else if (isForsageForward && speed > forsageForwardMaxSpeed)
        {
            speed = forsageForwardMaxSpeed;
        }
        else if (isForsageBackward && speed < -forsageBackwardsMaxSpeed)
        {
            speed = -forsageBackwardsMaxSpeed;
        }

        switch (stoppingFromBackward)
        {
            case true when throttle == 0 && speed > 0:
            case false when throttle == 0 && speed < 0:
                speed = 0;
                break;
        }

        time += Dt;

        pointList.Add(new (speed, time));
    }

    public sealed record AccelerationPoints(double Speed, double Time);

    public sealed record AccelerationData(double TimeToFullSpeedForward, double TimeToFullSpeedBackward, List<AccelerationPoints> AccelerationPointsList);
}
