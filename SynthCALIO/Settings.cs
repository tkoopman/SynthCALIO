using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace SynthCALIO
{
    public class Settings
    {
        [Tooltip("JSON config files location. {SkyrimData} or {SynthesisData} valid dynamic values to start folder with. {SynthesisData} can be used alone, but for {SkyrimData} you should add a sub-folder.")]
        public string Folder { get; set; } = "{SkyrimData}\\SynthCALIO";

        [Tooltip("Add EditorID comments to each SPID entry written to INI file.")]
        public bool IncludeEditorID { get; set; } = false;

        [Tooltip("Output folder for the SPID ini. Leave empty for Skyrim data folder.")]
        public string Output { get; set; } = "";

        [Tooltip("""
            As output files can change, only way to clear up old output files is to delete files from the Output folder.
            As long as you have set this to a dedicated output mod folder this is safe.
            Will delete all files in the output folder that could match configured outputs.
            Even if enabled this will not happen if output is left blank or set to your Skyrim's data folder.
            """)]
        public bool OutputClear { get; set; } = true;

        [Tooltip("Updates cache from existing version of this mod, to make sure any existing outfits keep the same FormID.\nThis happens after loading the outfit's cache from the Synthesis data folder.")]
        [SettingName("Update cache from existing mod")]
        public bool UpdateCache { get; set; } = true;
    }
}