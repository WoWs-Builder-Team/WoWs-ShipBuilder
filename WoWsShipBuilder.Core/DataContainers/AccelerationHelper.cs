using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    private const string CaraccioloId = "PISB107";
    private const double Dt = 0.01;
    private const double Margin = 0.05;
    private const int MaxIterations = 10000;

    public const int FullReverse = -1;
    public const int Zero = 0;
    public const int OneQuarter = 1;
    public const int Half = 2;
    public const int ThreeQuarter = 3;
    public const int FullAhead = 4;

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
    /// <param name="throttleList">List of throttle to use. First is the starting throttle.</param>
    /// <param name="accelerationModifiers"> Modifiers affecting the acceleration.</param>
    /// <param name="speedBoostModifiers"> Modifiers coming from speed boost.</param>
    /// <exception cref="InvalidOperationException">Thrown when the throttleList contains invalid numbers.</exception>
    /// <exception cref="OverflowException">Thrown when the calculation takes too many iterations.</exception>
    /// <returns>The data regarding acceleration.</returns>
    public static AccelerationData CalculateAcceleration(
        string shipIndex,
        Hull hull,
        Engine engine,
        ShipClass shipClass,
        List<int> throttleList,
        AccelerationModifiers accelerationModifiers,
        SpeedBoostAccelerationModifiers speedBoostModifiers)
    {
        // check that only valid values are contained in throttleList
        if (throttleList.Any(throttle => throttle is > 4 or < -1))
        {
            throw new InvalidOperationException("Throttles must be between -1 and 4. Use the constant of AccelerationHelper for safety.");
        }

        var result = new List<AccelerationPoints>();
        var timeForGear = new List<double>();

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

        // calculate other stats
        var maxForwardSpeed = baseShipSpeed * accelerationModifiers.SpeedMultiplier;
        var maxReverseSpeed = ((baseShipSpeed / 4) + 4.9) * accelerationModifiers.SpeedMultiplier;

        var powerRatio = horsepower / tonnage;
        var maxPowerForward = Math.Pow(powerRatio, 0.42) * Math.Pow(accelerationModifiers.SpeedMultiplier, 2);
        var maxPowerBackwards = (maxPowerForward / GetPfToPbRatio(shipClass, shipIndex)) * Math.Pow(accelerationModifiers.SpeedMultiplier, 2);

        var timeForward = (fullPowerForwardTime / timeConstant) * accelerationModifiers.EngineForwardUpTimeModifiers;
        var timeBackward = (fullPowerBackwardTime / timeConstant) * accelerationModifiers.EngineBackwardUpTimeModifiers;

        var forsageForward = speedBoostModifiers.ForwardEngineForsagOverride == 0 ? forwardEngineForsag * accelerationModifiers.EngineForwardForsagePowerModifier : speedBoostModifiers.ForwardEngineForsagOverride;
        var forsageBackwards = speedBoostModifiers.BackwardEngineForsagOverride == 0 ? backwardEngineForsag * accelerationModifiers.EngineBackwardForsagePowerModifier : speedBoostModifiers.BackwardEngineForsagOverride;

        var forsageForwardMaxSpeed = speedBoostModifiers.SpeedBoostEngineForwardForsageMaxSpeedOverride == 0 ? forwardEngineForsagMaxSpeed * accelerationModifiers.EngineForwardForsageMaxSpeedModifier : speedBoostModifiers.SpeedBoostEngineForwardForsageMaxSpeedOverride;
        var forsageBackwardsMaxSpeed = speedBoostModifiers.SpeedBoostEngineBackwardEngineForsagOverride == 0 ? backwardEngineForsagMaxSpeed * accelerationModifiers.EngineBackwardForsageMaxSpeedModifier : speedBoostModifiers.SpeedBoostEngineBackwardEngineForsagOverride;

        var powerIncreaseForward = Dt * maxPowerForward / timeForward;
        var powerIncreaseBackward = Dt * maxPowerBackwards / timeBackward;

        // begin the pain, aka the math

        // calculate initial stats
        // throttle goes from -1 to 4, depending on the gear.
        double oldThrottle = throttleList.First();

        // initial speed is equal to the speed limit
        double speed = GetSpeedLimit(oldThrottle, maxForwardSpeed, maxReverseSpeed);
        double power = GetPowerFromThrottle(oldThrottle, maxPowerForward, maxPowerBackwards);
        double time = 0;

        result.Add(new(speed, time));

        foreach (var throttle in throttleList.Skip(1))
        {
            // get new throttle speedLimit
            double speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

            int isDown = (throttle < oldThrottle && throttle > 0) ? 1 : 0;

            int iterations = 0;
            while (ShouldCycle(throttle, oldThrottle, speed, speedLimit))
            {
                GenerateAccelerationPoints(result, throttle, isDown, ref time, ref speed, speedLimit, ref power, powerIncreaseForward, maxPowerForward, maxForwardSpeed, forsageForwardMaxSpeed, forsageForward, powerIncreaseBackward, maxPowerBackwards, maxReverseSpeed, forsageBackwardsMaxSpeed, forsageBackwards, throttle > oldThrottle);

                // isDown needs to be 1 only for one cycle, so we set it to 0.
                isDown = 0;

                // infinite iterations failsafe.
                iterations++;
                if (iterations > MaxIterations)
                {
                    throw new OverflowException("Too many iterations for ship " + shipIndex);
                }
            }

            // update data for next cycle
            oldThrottle = throttle;

            // set the power to max of the current throttle. This is done because the speed function is asymptotic to MaxSpeed, so we force it to finish slightly earlier.
            power = GetPowerFromThrottle(throttle, maxForwardSpeed, maxPowerBackwards);
            timeForGear.Add(time);
        }

        return new(timeForGear, result);
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
    /// Get the power for a determined throttle.
    /// </summary>
    /// <param name="throttle">The throttle for the power to get.</param>
    /// <param name="maxPowerForward">The maximum power forward.</param>
    /// <param name="maxPowerBackwards">The maximum power backwards.</param>
    /// <returns>The power of the given throttle.</returns>
    private static double GetPowerFromThrottle(double throttle, double maxPowerForward, double maxPowerBackwards)
    {
        if (throttle > 0)
        {
            return maxPowerForward * Math.Pow(throttle / 4, 2);
        }
        else
        {
            return maxPowerBackwards;
        }
    }

    /// <summary>
    /// Return if the calculation cycle should continue or stop.
    /// </summary>
    /// <param name="newThrottle">Current throttle to reach.</param>
    /// <param name="oldThrottle">Starting throttle.</param>
    /// <param name="speed">Current speed.</param>
    /// <param name="speedLimit">Current Speed limit.</param>
    /// <returns>If the cycle should continue.</returns>
    private static bool ShouldCycle(double newThrottle, double oldThrottle, double speed, double speedLimit)
    {
        // we are accelerating/going towards max speed. Graph is going upwards
        if (newThrottle > oldThrottle)
        {
            return speed < speedLimit - Margin;
        }

        // we are decelerating/going towards reverse speed. Graph is going downwards.
        return speed > speedLimit + Margin;
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
        bool stoppingFromBackward)
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

    /// <summary>
    /// Record to hold the data calculated by <see cref="AccelerationHelper"/>.
    /// </summary>
    /// <param name="Speed">The speed at the time of the Time parameter.</param>
    /// <param name="Time">The current time.</param>
    public sealed record AccelerationPoints(double Speed, double Time);

    /// <summary>
    /// Wrapper holder for the data calculated by <see cref="AccelerationHelper"/>.
    /// </summary>
    /// <param name="TimeForGear">List of times needed to reach the gear given in input to <see cref="AccelerationHelper.CalculateAcceleration"/>.</param>
    /// <param name="AccelerationPointsList">List of the points calculated.</param>
    public sealed record AccelerationData(List<double> TimeForGear, List<AccelerationPoints> AccelerationPointsList);

    /// <summary>
    /// Records that holds all the non speed boost modifiers.
    /// </summary>
    /// <param name="SpeedMultiplier">Multiplier for the base speed to apply.</param>
    /// <param name="EngineForwardUpTimeModifiers">Multipliers for the engine forward up time to apply.</param>
    /// <param name="EngineBackwardUpTimeModifiers">Multipliers for the engine backwards up time to apply.</param>
    /// <param name="EngineForwardForsageMaxSpeedModifier">Multipliers for the forward forsage max speed to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <param name="EngineBackwardForsageMaxSpeedModifier">Multipliers the backward forsage max speed to  to apply. <b>IMPORTANT:</b> speed boost <b>OVERRIDE</b> this parameter, it does not stack.</param>
    /// <param name="EngineForwardForsagePowerModifier">Multipliers for the forward forsage power to apply. <b>IMPORTANT:</b> for speed boost <b>USE</b> the speedBoostEngineForwardForsageMaxSpeedOverride parameter of <see cref="SpeedBoostAccelerationModifiers"/>. Speed boost is not a multiplier, but override the value directly.</param>
    /// <param name="EngineBackwardForsagePowerModifier">Multipliers for the backward forsage power to apply. <b>IMPORTANT:</b> for speed boost <b>USE</b> the speedBoostEngineForwardForsageMaxSpeedOverride parameter of <see cref="SpeedBoostAccelerationModifiers"/>. Speed boost is not a multiplier, but override the value directly.</param>
    public sealed record AccelerationModifiers(double SpeedMultiplier, double EngineForwardUpTimeModifiers, double EngineBackwardUpTimeModifiers, double EngineForwardForsageMaxSpeedModifier, double EngineBackwardForsageMaxSpeedModifier, double EngineForwardForsagePowerModifier, double EngineBackwardForsagePowerModifier);

    /// <summary>
    /// Record that holds all the speed boost modifiers.
    /// </summary>
    /// <param name="SpeedBoostEngineForwardForsageMaxSpeedOverride">Value from speed boost that override the actual engineForwardForsageMaxSpeed. Set to 0 if not present.</param>
    /// <param name="SpeedBoostEngineBackwardEngineForsagOverride">Value from speed boost that override the actual engineBackwardForsageMaxSpeed. Set to 0 if not present.</param>
    /// <param name="ForwardEngineForsagOverride">Value from speed boost that override the actual forwardEngineForsag. Set to 0 if not present.</param>
    /// <param name="BackwardEngineForsagOverride">Value from speed boost that override the actual backwardEngineForsag. Set to 0 if not present.</param>
    public sealed record SpeedBoostAccelerationModifiers(double SpeedBoostEngineForwardForsageMaxSpeedOverride, double SpeedBoostEngineBackwardEngineForsagOverride, double ForwardEngineForsagOverride, double BackwardEngineForsagOverride);
}
