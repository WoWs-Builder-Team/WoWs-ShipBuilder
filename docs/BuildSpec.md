# Build-string format for ShipBuilder

ShipBuilder offers two build string formats where the old format is named v1 and the new one v2.
Both formats have the same features.
It is required to always maintain backwards compatibility when there are changes to the build string system meaning that the generation of strings in one format can be marked as obsolete but the deserialization of a string format must never be marked as obsolete.

## Build string v1

In v1, a build string is a json representation of the build class which is then converted into bytes and compressed using DeflateStream with compression level "Optional".
After compression, the result is then encoded into a base64 string.

## Build string v2

In v2, a build string is a simplified representation of the build class matching the following regex:

`^(?<shipIndex>[^;]*);(?<modules>[A-Z0-9,]*);(?<upgrades>[A-Z0-9,]*);(?<captain>[A-Z0-9]+);(?<skills>[0-9,]*);(?<consumables>[A-Z0-9,]*);(?<signals>[A-Z0-9,]*);(?<version>\d+)(;(?<buildName>[^;]*))?$`

Note that in order for this regex to work, named capture groups (`(?<name>...)`) have to be supported.

The build name is optional and defaults to an empty string when omitted or empty. Other parts of the string must not be missing.

An example for a build string using this format is the following string: `PASC020;PAUA901,PAUH901,PAUS901,PAUE902;;PCW001;;PCY009,PCY011,PCY020,PCY010;;2;test-build`
In this case, the modules are the default ship modules which can be omitted entirely: `PASC020;;;PCW001;;;;2;test-build`

However, the captain must never be omitted even if it is the default captain.

## Build string v3

In v3, the build string keeps the format of v2, however, skill names are reduced to their index.
Old builds are automatically converted upon loading a build.

## Build string v4

No structural changes compared to v2, however, module names are reduced to their index.
Old build are automatically converted upon loading a build.