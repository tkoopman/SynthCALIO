## About

SynthCALIO can create new LeveledItem and Outfit records, and assign them to NPC's Default or Sleeping outfits, and/or add to Spell Perk Item Distributor (SPID) file.  
SynthCALIO keeps track of IDs assigned in previous runs to make sure the same FormID is used each time, even if you add/remove configurations.

## Links
[Nexus](https://www.nexusmods.com/skyrimspecialedition/mods/144763)  
[Documentation](https://tkoopman.github.io/SynthCALIO/)  
[Examples](./Examples/)

## Prerequisites

- [Synthesis](https://github.com/Mutagen-Modding/Synthesis)
- [Spell Perk Item Distributor (SPID)](https://www.nexusmods.com/skyrimspecialedition/mods/36869)

## Using

By default, the JSON configuration files should be added into a sub-folder SynthCALIO in your Skyrim's data folder. This way you can add these configuration files like normal mods in your mod manager of choice.
Location can be overridden in the Synthesis settings of SynthCALIO.

Next you will need a working Synthesis. See there documentation for getting this setup.

You can then add SynthCALIO, however if adding to a Synthesis group that contains other patchers, SynthCALIO MUST be the first patcher in the group. This is so it doesn't get possible FormID conflicts.

## How SynthCALIO maintains FormID's

There are 2 methods used.  
First, at the end of each run it will save a file formIDCache.txt to the Synthesis data folder. This contains a mapping of each known FormID to EditorID. This is then loaded at the start of every run.
As this file will not remove entries just because they may not of been exported this run (maybe you removed a JSON config file), it will keep those IDs available unless you manually clear the file.
You may delete this cache if you need to reset it, but if next method is enabled it will be repopulated with any entries that exist in the current version. So disable it as well to fully reset FormIDs.

Then optionally it will also read the current SynthCALIO's mod for all IDs to EditorID mappings it contains and update/add to the entries loaded from formIDCache.txt.
In most situations this should never be required as the cache file should contain everything, but this is here as backup.
You can disable this in the Synthesis SynthCALIO Settings, but is enabled by default.

This does mean that if you rename an entry in a JSON config file, it will get a new FormID as the EditorID has changed.

## Configuration file format

Please see [documentation](https://tkoopman.github.io/SynthCALIO/) for details on how to create the JSON configuration files.

## Bugs, Requests and Contributions

Please log any bugs or requests you may have via [GitHub Issues](https://github.com/tkoopman/SynthCALIO/issues) or over on the appropriate Nexus page.  
While I make no guarantee to fixing or implementing new requests due to other commitments, I will try, especially fixing bugs.  
Also if you want to contribute please do, even if you don't known how to program, just improving my awful documentation would help others.
