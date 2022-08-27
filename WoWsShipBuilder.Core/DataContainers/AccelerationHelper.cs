using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public static class AccelerationHelper
{
    public static List<AccelerationPoints> CalculateAcceleration(
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

        // calculate other stats
        double maxForwardSpeed = baseShipSpeed * speedMultiplier;
        double maxReverseSpeed = ((baseShipSpeed / 4) + 4.9) * speedMultiplier;

        double powerRatio = horsepower / tonnage;
        double maxPowerForward = Math.Pow(powerRatio, 0.42) * Math.Pow(speedMultiplier, 2);
        double maxPowerBackwards = (maxPowerForward / GetPfToPbRatio(shipClass)) * Math.Pow(speedMultiplier, 2);

        double timeForward = (fullPowerForwardTime / timeConstant) * engineForwardUpTimeModifiers;
        double timeBackward = (fullPowerBackwardTime / timeConstant) * engineBackwardUpTimeModifiers;

        double forsageForward = forwardEngineForsag * engineForwardForsagePowerModifier;
        double forsageBackwards = backwardEngineForsag * engineBackwardForsagePowerModifier;

        double forsageForwardMaxSpeed = forwardEngineForsagMaxSpeed * engineForwardForsageMaxSpeedModifier;
        double forsageBackwardsMaxSpeed = backwardEngineForsagMaxSpeed * engineBackwardForsageMaxSpeedModifier;

        // begin the pain, aka the math
        const double dt = 0.01;

        double speed = 0;
        double time = 0;
        double power = 0;

        // throttle goes from -1 to 4, depending on the gear.
        double throttle = 4;

        int isDecelerating = 0;

        result.Add(new(speed, time));

        double speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);

        // we go forward!
        while (speed < maxForwardSpeed - 0.1)
        {
            int acc;
            if (speedLimit > speed)
            {
                power = shipIndex.Equals("PASB110") || shipIndex.Equals("PFSD210") ? maxPowerForward : Math.Min(Math.Max(power, 0) + (dt * maxPowerForward / timeForward), maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating));

                acc = 1;
            }
            else if (speedLimit < speed)
            {
                power = Math.Max(Math.Min(power, 0) - (dt * maxPowerBackwards / timeBackward), -maxPowerBackwards);
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

            // apply mods
            if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
            {
                acceleration = (maxPowerForward * forsageForward) - drag;
            }

            if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
            {
                acceleration = (-maxPowerBackwards * forsageBackwards) + drag;
            }

            double previousSpeed = speed;

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

        result.Add(new(maxForwardSpeed, time += dt));

        // and now we sail for a bit and then stop!
        bool decelerate = false;
        if (decelerate)
        {
            const int sailingTime = 3;
            time += sailingTime;
            result.Add(new (speed, time));
            throttle = 0;
            speedLimit = GetSpeedLimit(throttle, maxForwardSpeed, maxReverseSpeed);
        }

        while (speed > 0 && decelerate)
        {
            if (time > 360)
            {
                break;
            }

            int acc;
            if (speedLimit > speed)
            {
                power = Math.Min(Math.Max(power, 0) + (dt * maxPowerForward / timeForward), maxPowerForward * Math.Pow(Math.Pow(throttle / 4, 2), isDecelerating));
                acc = 1;
            }
            else if (speedLimit < speed)
            {
                power = Math.Max(Math.Min(power, 0) - (dt * maxPowerBackwards / timeBackward), -maxPowerBackwards);
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

            // apply mods
            if (speed < forsageForwardMaxSpeed && speed >= 0 && power > 0)
            {
                acceleration = (maxPowerForward * forsageForward) - drag;
            }

            if (speed > -forsageBackwardsMaxSpeed && speed <= 0 && power < 0)
            {
                acceleration = (-maxPowerBackwards * forsageBackwards) + drag;
            }

            double previousSpeed = speed;

            speed += Math.Round(dt * acceleration, 1);

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
    private static double GetDrag(double speed, double maxForwardSpeed, double maxPowerForward, double maxReverseSpeed, double maxPowerBackwards)
    {
        // drag=@(x) (-x.*abs(x)/(Vmax)^2*PF).*(x>0) + (-x.*abs(x)/(Vmin)^2*PB).*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (speed > 0)
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxForwardSpeed, 2);
            result = speedRatio * maxPowerForward;
        }
        else
        {
            double speedRatio = (-speed * Math.Abs(speed)) / Math.Pow(maxReverseSpeed, 2);
            result = -speedRatio * maxPowerBackwards;
        }

        return result;
    }

    private static double GetSpeedLimit(double throttle, double maxForwardSpeed, double maxReverseSpeed)
    {
        // speed_limit=@(x) x/4*Vmax*(x>=0)-Vmin*(x<0);
        // first part if x > 0, second if x < 0
        double result;
        if (throttle >= 0)
        {
            result = throttle / 4 * maxForwardSpeed;
        }
        else
        {
            result = -maxReverseSpeed;
        }

        return result;
    }

    public sealed record AccelerationPoints(double Speed, double Time);
}
