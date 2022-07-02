using DnetIndexedDb;

namespace WoWsShipBuilder.Web.Data;

public record GameDataDto([property:IndexDbKey(AutoIncrement = false)] string Path, string Content);
