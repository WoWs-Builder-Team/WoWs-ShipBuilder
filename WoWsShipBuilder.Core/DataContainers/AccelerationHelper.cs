using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    public static List<AccelerationPoints> CalculateAcceleration(
        double baseShipSpeed,
        double speedMultiplier,
        double horsepower,
        double tonnage,
        ShipClass shipClass,
        double forwardEngineForsag,
        double backwardEngineForsag,
        double fullPowerForwardTime,
        double engineForwardForsageMaxSpeed = 1,
        double engineBackwardForsageMaxSpeed = 1,
        double engineForwardForsagePower = 1,
        double engineBackwardForsagePower = 1,
        double engineBackwardUpTime = 1)
    {
        var result = new List<AccelerationPoints>();

        var forwardSpeed = baseShipSpeed * speedMultiplier;
        var reverseSpeed = ((baseShipSpeed / 4) + 4.9) * speedMultiplier;

        var powerRatio = horsepower / tonnage;
        var powerForward = Math.Pow(powerRatio, 0.4) * Math.Pow(speedMultiplier, 2);
        var powerBackwards = (powerForward / GetPfToPbRatio(shipClass)) * Math.Pow(speedMultiplier, 2);

        var TF = fullPowerForwardTime / ((double)Constants.TimeScale) / engineBackwardUpTime / engineBackwardUpTime;

        var forsageForward = forwardEngineForsag * engineForwardForsageMaxSpeed;
        var forsageBackwards = backwardEngineForsag * engineBackwardForsageMaxSpeed;

        // begin the pain, aka the math

        var dt = 0.01;

        double speed = 0;
        double time = 0;
        double power = 0;
        // throttle goes from -1 to 4, depending on the gear.
        double throttle = 4;

        result.Add(new(speed, time));

        while (speed < forwardSpeed)
        {
            var acc = 0;

            if (GetSpeedLimit(throttle, forwardSpeed, reverseSpeed) > speed)
            {
                power = Math.Min(Math.Max(power, 0) + (dt * powerForward / TF), powerForward);
                acc = 1;
            }
            else if (GetSpeedLimit(throttle, forwardSpeed, reverseSpeed) < speed)
            {
                power = Math.Max(Math.Min(power, 0) - (dt * powerBackwards / TF * 2), -powerBackwards);
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

            //apply mods
            if (speed < forsageForward && speed >= 0 && power > 0)
            {
                acceleration = (powerForward * forwardEngineForsag * engineForwardForsagePower) - drag;
            }

            if (speed > -forsageBackwards && speed <= 0 && power < 0)
            {
                acceleration = (-powerBackwards * backwardEngineForsag * engineBackwardForsagePower) + drag;
            }

            var previousSpeed = speed;

            speed += dt * acceleration;

            var speedLimit = GetSpeedLimit(throttle, forwardSpeed, reverseSpeed);
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
            result = -speed * Math.Abs(speed) / Math.Pow(forwardSpeed, 2 * powerForward);
        }
        else
        {
            result = -speed * Math.Abs(speed) / Math.Pow(reverseSpeed, 2 * powerBackwards);
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
