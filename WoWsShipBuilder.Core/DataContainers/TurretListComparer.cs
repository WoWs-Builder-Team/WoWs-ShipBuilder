using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Ship.Components;

namespace WoWsShipBuilder.Core.DataContainers
{
    public class TurretListComparer : IComparer<IGun>
    {
        public int Compare(IGun? firstGun, IGun? secondGun)
        {
            if (firstGun == null || secondGun == null)
            {
                throw new InvalidOperationException("Cannot process null values");
            }

            if (firstGun.VerticalPosition < 3)
            {
                return firstGun.VerticalPosition.CompareTo(secondGun.VerticalPosition);
            }

            return secondGun.VerticalPosition.CompareTo(firstGun.VerticalPosition);
        }
    }
}
