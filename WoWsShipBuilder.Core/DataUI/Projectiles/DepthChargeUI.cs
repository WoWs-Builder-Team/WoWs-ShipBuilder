using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.DataUI.Projectiles
{
    public record DepthChargeUI : ProjectileUI, IDataUi
    {
        public static BombUI? FromChargesName(string name, List<(string name, float value)> modifiers)
        {
            return null;
        }
    }
}
