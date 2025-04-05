using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;

namespace SynthCALIO.Outputs
{
    public class ConfigParser
    {
        public const string DEFAULTINI = """
            [*_DISTR.ini]
            Rename={mod}_DISTR.ini
            OnIncomplete=RemoveLine
            KeepHeader=true
            FormID=0x{id}~{mod.esp}

            [SKSE/Plugins/SkyPatcher/*/*.ini]
            Rename=
            OnIncomplete=RemoveLine
            KeepHeader=true
            FormID={mod.esp}|{id}

            [SkyPatcher]
            Regex=^SKSE/Plugins/SkyPatcher/.*.es[mpl].ini$
            Rename={mod.esp}.ini
            """;

        public readonly IReadOnlyList<ConfigMatcher> ConfigEntries;

        private ConfigParser (IniData ini)
        {
            List<ConfigMatcher> entries = [];

            foreach (var section in ini.Sections)
            {
                string name = section.SectionName;
                string? formID = get(section.Keys, Config.SETTING_FORMID);
                string? onIncomplete = get(section.Keys, Config.SETTING_ONINCOMPLETE);
                string? regex = get(section.Keys, ConfigMatcher.SETTING_REGEX);
                string? keepHeader = get(section.Keys, Config.SETTING_KEEPHEADER);
                string? rename = get(section.Keys, Config.SETTING_RENAME);

                entries.Add(new ConfigMatcher(name, formID, onIncomplete, regex, keepHeader, rename));
            }

            ConfigEntries = entries.AsReadOnly();
        }

        public static ConfigParser FromFile (string configFile)
        {
            if (!Path.Exists(configFile))
                File.WriteAllText(configFile, DEFAULTINI);

            var iniConfig = new IniParserConfiguration()
            {
                AllowKeysWithoutSection = false,
                AllowCreateSectionsOnFly = false,
                CaseInsensitive = true,
            };

            var iniParser = new FileIniDataParser(new IniDataParser(iniConfig));
            var ini = iniParser.ReadFile(configFile);

            return new ConfigParser(ini);
        }

        public static ConfigParser FromString (string config)
        {
            var iniConfig = new IniParserConfiguration()
            {
                AllowKeysWithoutSection = false,
                AllowCreateSectionsOnFly = false,
                CaseInsensitive = true,
            };

            var iniParser = new IniDataParser(iniConfig);
            var ini = iniParser.Parse(config);

            return new ConfigParser(ini);
        }

        public bool CouldMatch (string relativeFilename)
        {
            relativeFilename = relativeFilename.Trim().Replace("\\", "/");
            var matches = ConfigEntries.Where(x => x.IsMatch(relativeFilename));
            return matches.Any();
        }

        public Config? FindConfig (string relativeFilename, string? oneLiner = null)
        {
            relativeFilename = relativeFilename.Trim().Replace("\\", "/");
            var matches = ConfigEntries.Where(x => x.IsMatch(relativeFilename));

            Config? config = null;

            if (matches.Any())
            {
                foreach (var entry in matches)
                    config = Config.Merge(entry, config);
            }

            if (Config.TryFrom1Liner(oneLiner, out var olConfig))
                config = config == null ? olConfig : Config.Merge(olConfig, config);

            return config;
        }

        private static string? get (KeyDataCollection keys, string key) => keys.ContainsKey(key) ? string.IsNullOrWhiteSpace(keys[key]) ? null : keys[key] : null;
    }
}