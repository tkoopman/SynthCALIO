﻿using Common;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

using Newtonsoft.Json;

namespace SynthCALIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonOutfit (string editorID)
    {
        public List<string[]> AllItems { get; set; } = [];

        /// <summary>
        ///     List of NPC records to update the DefaultOutfit for if outfit added. List by FormID
        ///     or EditorID.
        /// </summary>
        [JsonProperty]
        public string[] DefaultOutfit { get; set; } = [];

        [JsonProperty(propertyName: "Name", Required = Required.Always)]
        public string EditorID { get; } = editorID.IsValidEditorID() ? editorID : throw new ArgumentException($"Invalid EditorID: {editorID}", nameof(EditorID));

        /// <summary>
        ///     Number of items, that should be added, but are missing, that will cause Outfit to be
        ///     skipped?
        ///     - 1 (Default): Skip if any item is missing.
        ///     - 2+: Number of missing items to trigger skip. If larger than number of items in
        ///     list then will never skip.
        ///     - 0: Never Skip
        ///     - -1: Skip only if all items are missing
        /// </summary>
        [JsonProperty]
        public int SkipIfMissing { get; set; } = SynthCALIO.SkipIfMissing.Any;

        /// <summary>
        ///     List of NPC records to update the SleepingOutfit for if outfit added. List by FormID
        ///     or EditorID.
        /// </summary>
        [JsonProperty]
        public string[] SleepingOutfit { get; set; } = [];

        /// <summary>
        ///     This is the list of SPID entries, starting from the StringFilters, that will be
        ///     added to the INI file. Everything before StringFilters will be filled in
        ///     automatically. As such it must contain 5 pipe (|) characters if provided.
        ///     Outfit=FormID|StringFilters|FormFilters|LevelFilters|TraitFilters|CountOrPackageIndex|Chance
        ///
        ///     Will only add SPID entries if the outfit was added to the mod.
        /// </summary>
        [JsonProperty]
        public string[] SPID { get; set; } = [];

        internal string? FromFile { get; } = JsonConfig.CurrentFile;

        [JsonProperty]
        private string[] Items { set => AllItems.Add(value); }

        /// <summary>
        ///     Basic checks to ensure the outfit record looks valid. Doesn't actually check if
        ///     items referenced are valid, just that they have a valid FormID or EditorID format.
        /// </summary>
        public void BasicChecks ()
        {
            if (AllItems.Count == 0 && SkipIfMissing != SynthCALIO.SkipIfMissing.Never)
                throw new InvalidDataException($"Outfit {EditorID} from {FromFile}, contains no items");

            foreach (string[] group in AllItems)
            {
                foreach (string item in group)
                {
                    if (SynthCommon.TryConvertToSkyrimID(item, out _, out _) == SkyrimIDType.Invalid)
                        throw new InvalidDataException($"Outfit {EditorID} from {FromFile}, contains invalid item: {item}");
                }
            }

            foreach (string outfit in DefaultOutfit)
            {
                if (SynthCommon.TryConvertToSkyrimID(outfit, out _, out _) == SkyrimIDType.Invalid)
                    throw new InvalidDataException($"Outfit {EditorID} from {FromFile}, contains invalid DefaultOutfit entry: {outfit}");
            }

            foreach (string outfit in SleepingOutfit)
            {
                if (SynthCommon.TryConvertToSkyrimID(outfit, out _, out _) == SkyrimIDType.Invalid)
                    throw new InvalidDataException($"Outfit {EditorID} from {FromFile}, contains invalid SleepingOutfit entry: {outfit}");
            }

            // This is just to confirm all SPIDs provided can be parsed before actually starting
            foreach (string item in SPID)
                _ = Program.formatSPID(item, FormKey.Null, "Test", FromFile);
        }

        public Outfit? ToOutfit ()
        {
            var outfit = new Outfit(new FormKey(Program.State.PatchMod.ModKey, Program.GetFormID(EditorID)), Program.State.GameRelease.ToSkyrimRelease())
            {
                EditorID = EditorID,
            };
            bool first = true;
            foreach (string[] group in AllItems)
            {
                if (!first)
                    Console.WriteLine($"Previous item group skipped, processing next item group for {outfit.EditorID}");
                else
                    first = false;

                outfit.Items ??= [];
                int skipped = 0;

                foreach (string item in group)
                {
                    if (!Program.TryGetRecord<IOutfitTargetGetter>(item, out var record))
                    {
                        if (++skipped == SkipIfMissing)
                        {
                            Console.WriteLine($"Skipping adding {item} to Outfit: {EditorID}. Record not found. Config file: {FromFile}.");
                            outfit.Items = null;
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Skipping adding {item} to Outfit: {EditorID}. Record not found. Config file: {FromFile}.");
                        }
                    }
                    else
                    {
                        outfit.Items.Add(record.ToLinkGetter());
                    }
                }

                if (outfit.Items is not null && (SkipIfMissing == SynthCALIO.SkipIfMissing.Never || outfit.Items.Count != 0))
                    break;
            }

            if (outfit.Items is null || (SkipIfMissing != SynthCALIO.SkipIfMissing.Never && outfit.Items.Count == 0))
            {
                Console.WriteLine($"Skipping Outfit: {EditorID}. No valid items found. Config file: {FromFile}.");
                return null;
            }

            return outfit;
        }
    }

    internal class JsonOutfitKey : IEqualityComparer<JsonOutfit>
    {
        public bool Equals (JsonOutfit? x, JsonOutfit? y) => string.Equals(x?.EditorID, y?.EditorID, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode (JsonOutfit obj) => obj.EditorID.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}