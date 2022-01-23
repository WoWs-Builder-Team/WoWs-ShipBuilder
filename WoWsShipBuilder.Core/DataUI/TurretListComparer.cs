using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public class TurretListComparer : IComparer<Gun>
    {
        public int Compare(Gun? firstGun, Gun? secondGun)
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
