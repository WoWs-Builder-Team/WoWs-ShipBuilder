using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    public static List<AccelerationPoints> CalculateAcceleration(
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
        var sailingTime = 3;

        // get starting stats
        var baseShipSpeed = decimal.ToDouble((1 + engine.SpeedCoef) * hull.MaxSpeed);
        var horsepower = decimal.ToDouble(hull.EnginePower);
        var tonnage = hull.Tonnage;
        var fullPowerForwardTime = decimal.ToDouble(engine.ForwardEngineUpTime);
        var fullPowerBackwardTime = decimal.ToDouble(engine.BackwardEngineUpTime);
        var timeConstant = decimal.ToDouble(Constants.TimeScale);
        var forwardEngineForsag = decimal.ToDouble(engine.ForwardEngineForsag);
        var backwardEngineForsag = decimal.ToDouble(engine.BackwardEngineForsag);
        var forwardEngineForsagMaxSpeed = decimal.ToDouble(engine.ForwardEngineForsagMaxSpeed);
        var backwardEngineForsagMaxSpeed = decimal.ToDouble(engine.BackwardEngineForsagMaxSpeed);

        // calculate other stats
        var forwardSpeed = baseShipSpeed * speedMultiplier;
        var reverseSpeed = ((baseShipSpeed / 4) + 4.9) * speedMultiplier;

        var powerRatio = horsepower / tonnage;
        var powerForward = Math.Pow(powerRatio, 0.4) * Math.Pow(speedMultiplier, 2);
        var powerBackwards = (powerForward / GetPfToPbRatio(shipClass)) * Math.Pow(speedMultiplier, 2);

        var timeForward = (fullPowerForwardTime / timeConstant) * engineForwardUpTimeModifiers;
        var timeBackward = (fullPowerBackwardTime / timeConstant) * engineBackwardUpTimeModifiers;

        var forsageForward = forwardEngineForsag * engineForwardForsagePowerModifier;
        var forsageBackwards = backwardEngineForsag * engineBackwardForsagePowerModifier;

        var forsageForwardMaxSpeed = forwardEngineForsagMaxSpeed * engineForwardForsageMaxSpeedModifier;
        var forsageBackwardsMaxSpeed = backwardEngineForsagMaxSpeed * engineBackwardForsageMaxSpeedModifier;

        // begin the pain, aka the math
        var dt = 0.5;

        double speed = 0;
        double time = 0;
        double power = 0;

        // throttle goes from -1 to 4, depending on the gear.
        double throttle = 4;

        int isDecelerating = 0;

        result.Add(new(speed, time));

        // we go forward!
        while (speed < forwardSpeed)
        {
            var acc = 0;

            var speedLimit = GetSpeedLimit(throttle, forwardSpeed, reverseSpeed);
            if (speedLimit > speed)
            {
                var firstPower = Math.Max(power, 0);
                var powerTimeRatio = powerForward / timeForward;
                var secondPower = dt * powerTimeRatio;
                var coeff = Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating);
                power = Math.Min(firstPower + secondPower, powerForward * coeff);
                acc = 1;
            }
            else if (speedLimit < speed)
            {
                power = Math.Max(Math.Min(power, 0) - (dt * powerBackwards / timeBackward), -powerBackwards);
                acc = -1;
            }
            else
            {
                if (speed > 0)
                {
                    power = powerForward * Math.Pow(throttle / 4, 2);
                }
                else
                {
                    power = -powerBackwards;
                }

                acc = 0;
            }

            var drag = GetDrag(speed, forwardSpeed, powerForward, reverseSpeed, powerBackwards);
            var acceleration = (power + drag) * Math.Abs(acc);

            // apply mods
            if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
            {
                acceleration = (powerForward * forsageForward) - drag;
            }

            if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
            {
                acceleration = (-powerBackwards * forsageBackwards) + drag;
            }

            var previousSpeed = speed;

            speed += dt * acceleration;

            if (speedLimit < speed && acc == 1 && power * previousSpeed > 0)
            {
                speed = speedLimit;
            }

            if (speedLimit > speed && acc == -1 && power * previousSpeed > 0)
            {
                speed = speedLimit;
            }

            time += dt;
            result.Add(new (speed, time));
        }

        time += sailingTime;

        // and now we sail for a bit and then stop!
        result.Add(new (speed, time));
        throttle = 0;
        while (speed > 0)
        {
            var acc = 0;

            var speedLimit = GetSpeedLimit(throttle, forwardSpeed, reverseSpeed);
            if (speedLimit > speed)
            {
                power = Math.Min(Math.Max(power, 0) + (dt * powerForward / timeForward), powerForward * Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating));
                acc = 1;
            }
            else if (speedLimit < speed)
            {
                power = Math.Max(Math.Min(power, 0) - (dt * powerBackwards / timeBackward), -powerBackwards);
                acc = -1;
            }
            else
            {
                if (speed > 0)
                {
                    power = powerForward * Math.Pow(throttle / 4, 2);
                }
                else
                {
                    power = -powerBackwards;
                }

                acc = 0;
            }

            var drag = GetDrag(speed, forwardSpeed, powerForward, reverseSpeed, powerBackwards);
            var acceleration = (power + drag) * Math.Abs(acc);

            // apply mods
            if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
            {
                acceleration = (powerForward * forsageForward) - drag;
            }

            if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
            {
                acceleration = (-powerBackwards * forsageBackwards) + drag;
            }

            var previousSpeed = speed;

            speed += dt * acceleration;

            if (speedLimit < speed && acc == 1 && power * previousSpeed > 0)
            {
                speed = speedLimit;
            }

            if (speedLimit > speed && acc == -1 && power * previousSpeed > 0)
            {
                speed = speedLimit;
            }

            time += dt;
            result.Add(new (speed, time));
        }

        return result;
    }

    // this ratio is about 4 for battle (exception is 3 for Caracciolo), 3 for cruiser, and 2 for expulsion.
    // add exception for caraccioulo
    private static int GetPfToPbRatio(ShipClass shipClass)
    {
        switch (shipClass)
        {
            case ShipClass.Battleship:
                return 4;
            case ShipClass.Cruiser:
                return 3;
            case ShipClass.Destroyer:
                return 2;
            default:
                return 4;
        }
    }

    // Calculate water resistance
    private static double GetDrag(double speed, double forwardSpeed, double powerForward, double reverseSpeed, double powerBackwards)
    {
        // drag=@(x) (-x.*abs(x)/(Vmax)^2*PF).*(x>0) + (-x.*abs(x)/(Vmin)^2*PB).*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (speed > 0)
        {
            var speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(forwardSpeed, 2);
            result = speedRatio * powerForward;
        }
        else
        {
            var speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(reverseSpeed, 2);
            result = -speedRatio * powerBackwards;
        }

        return result;
    }

    private static double GetSpeedLimit(double throttle, double forwardSpeed, double reverseSpeed)
    {
        // speed_limit=@(x) x/4*Vmax*(x>=0)-Vmin*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (throttle >= 0)
        {
            result = throttle / 4 * forwardSpeed;
        }
        else
        {
            result = -reverseSpeed;
        }

        return result;
    }

    public sealed record AccelerationPoints(double Speed, double Time);
}
