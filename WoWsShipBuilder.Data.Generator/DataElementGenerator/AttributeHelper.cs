﻿using Microsoft.CodeAnalysis;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator;

public static class AttributeHelper
{
    public const string AttributeNamespace = "WoWsShipBuilder.DataElements.DataElementAttributes";

    public const string DataElementTypesEnumName = "DataElementTypes";

    // language=csharp
    public const string DataElementTypesEnum = $$"""
                                                 // <auto-generated />
                                                 #nullable enable
                                                 namespace {{AttributeNamespace}};

                                                 [global::System.Flags]
                                                 internal enum {{DataElementTypesEnumName}}
                                                 {
                                                     KeyValue = 1,
                                                     KeyValueUnit = 2,
                                                     Value = 4,
                                                     Grouped = 8,
                                                     Tooltip = 16,
                                                     FormattedText = 32,
                                                 }
                                                 """;

    public const string DataContainerAttributeName = "DataContainerAttribute";

    // language=csharp
    public const string DataContainerAttribute = $$"""
                                                   // <auto-generated />
                                                   #nullable enable
                                                   namespace {{AttributeNamespace}};

                                                   [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                                                   internal class {{DataContainerAttributeName}} : global::System.Attribute
                                                   {
                                                   }
                                                   """;

    public const string DataElementTypeAttributeName = "DataElementTypeAttribute";

    // language=csharp
    public const string DataElementTypeAttribute = $$"""
                                                     // <auto-generated />
                                                     #nullable enable
                                                     namespace {{AttributeNamespace}};

                                                     [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
                                                     internal class {{DataElementTypeAttributeName}} : global::System.Attribute
                                                     {
                                                         public {{DataElementTypeAttributeName}}(global::{{AttributeNamespace}}.DataElementTypes type)
                                                         {
                                                             this.Type = type;
                                                         }

                                                         /// <summary>
                                                         /// Gets the type of the DataElement for the property marked by this attribute. />
                                                         /// </summary>
                                                         public global::{{AttributeNamespace}}.DataElementTypes Type { get; }

                                                         /// <summary>
                                                         /// Gets or sets the unit localization key for the property marked by this attribute. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.KeyValueUnit"/> and <see cref="DataElementTypes.Tooltip"/>.
                                                         /// </summary>
                                                         public string? UnitKey { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets the property name localization key for the property marked by this attribute. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.Grouped"/>, <see cref="DataElementTypes.KeyValue"/>, <see cref="DataElementTypes.KeyValueUnit"/> and <see cref="DataElementTypes.Tooltip"/>.
                                                         /// </summary>
                                                         public string? NameLocalizationKey { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets the tooltip localization key for the property marked by this attribute. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.Tooltip"/>.
                                                         /// </summary>
                                                         public string? TooltipKey { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets the group localization key and identifier for the property marked by this attribute. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.Grouped"/>.
                                                         /// </summary>
                                                         public string? GroupKey { get; set; }

                                                         /// <summary>
                                                         /// Gets or set the name of the property containing the list of values that will replace the placeholder. Requires the value of the property marked by this attribute to follow the <see cref="string.Format(string,object[])"/> specifications. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.FormattedText"/>.
                                                         /// </summary>
                                                         public string? ValuesPropertyName { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets if the value of the property marked by this attribute is a localization key. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.Value"/>, <see cref="DataElementTypes.KeyValue"/> and <see cref="DataElementTypes.FormattedText"/>
                                                         /// </summary>
                                                         public bool IsValueLocalizationKey { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are localization keys. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.FormattedText"/>
                                                         /// </summary>
                                                         public bool ArePropertyNameValuesKeys { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets if the value of the property marked by this attribute is an app localization key. <br/>
                                                         /// Only valid for <see cref="DataElementTypes.Value"/>, <see cref="DataElementTypes.KeyValue"/> and <see cref="DataElementTypes.FormattedText"/>
                                                         /// </summary>
                                                         public bool IsValueAppLocalization { get; set; }

                                                         /// <summary>
                                                         /// Gets or sets if the values indicated by <see cref="ValuesPropertyName"/> are app localization keys.<br/>
                                                         /// Only valid for <see cref="DataElementTypes.FormattedText"/>
                                                         /// </summary>
                                                         public bool IsPropertyNameValuesAppLocalization { get; set; }
                                                     }
                                                     """;

    public const string DataElementFilteringAttributeName = "DataElementFilteringAttribute";

    // language=csharp
    public const string DataElementFilteringAttribute = $$"""
                                                          // <auto-generated />
                                                          #nullable enable
                                                          namespace {{AttributeNamespace}};

                                                          [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
                                                          internal class {{DataElementFilteringAttributeName}} : global::System.Attribute
                                                          {
                                                              public {{DataElementFilteringAttributeName}}(bool enableFilterVisibility, string filterMethodName = "")
                                                              {
                                                                  this.EnableFilterVisibility = enableFilterVisibility;
                                                                  this.FilterMethodName = filterMethodName;
                                                              }

                                                              public bool EnableFilterVisibility { get; }

                                                              public string FilterMethodName { get; }
                                                          }
                                                          """;

    public static void GenerateAttributes(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("DataElementTypes.g.cs", DataElementTypesEnum);
        context.AddSource("DataContainerAttribute.g.cs", DataContainerAttribute);
        context.AddSource("DataElementTypeAttribute.g.cs", DataElementTypeAttribute);
        context.AddSource("DataElementFilteringAttribute.g.cs", DataElementFilteringAttribute);
    }
}
