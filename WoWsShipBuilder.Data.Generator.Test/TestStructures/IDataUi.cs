using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.DataElements;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public interface IDataUi
{
    List<IDataElement> DataElements { get; }
}
