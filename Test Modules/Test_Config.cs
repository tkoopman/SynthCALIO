using SynthCALIO.Matcher;
using SynthCALIO.Outputs;

namespace Test_Modules
{
    public class ConfigParserTestCases : TheoryData<string, Config?>
    {
        public ConfigParserTestCases ()
        {
            var spidConfig = new Config("0x{id}~{mod.esp}", IncompleteAction.RemoveLine, true, "{mod}_DISTR.ini");
            var skyPatcherConfig1 = new Config("{mod.esp}|{id}", IncompleteAction.RemoveLine, true, "");
            var skyPatcherConfig2 = new Config("{mod.esp}|{id}", IncompleteAction.RemoveLine, true, "{mod.esp}.ini");

            // With forward slashes
            Add("file.ini", null);
            Add("dir/file.ini", null);
            Add("file_DISTR.ini", spidConfig);
            Add("dir/file_DISTR.ini", null);

            Add("SKSE/Plugins/SkyPatcher/file.ext", null);
            Add("SKSE/Plugins/SkyPatcher/file.ini", skyPatcherConfig1);
            Add("SKSE/Plugins/SkyPatcher/file.esp.ini", skyPatcherConfig2);

            Add("SKSE/Plugins/SkyPatcher/dir1/file.ext", null);
            Add("SKSE/Plugins/SkyPatcher/dir1/file.ini", skyPatcherConfig1);
            Add("SKSE/Plugins/SkyPatcher/dir1/file.esp.ini", skyPatcherConfig2);

            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.ext", null);
            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.ini", skyPatcherConfig1);
            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.esp.ini", skyPatcherConfig2);

            Add("SSKSE/Plugins/SkyPatcher/dir1/dir2/file.ext", null);
            Add("SSKSE/Plugins/SkyPatcher/dir1/dir2/file.ini", null);
            Add("SSKSE/Plugins/SkyPatcher/dir1/dir2/file.esp.ini", null);

            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.extz", null);
            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.iniz", null);
            Add("SKSE/Plugins/SkyPatcher/dir1/dir2/file.esp.iniz", null);
        }
    }

    public class FullTestCases : TheoryData<string, string?, Config?>
    {
        public FullTestCases ()
        {
            var spidConfig = new Config("0x{id}~{mod.esp}", IncompleteAction.RemoveLine, true, "{mod}_DISTR.ini");
            var skyPatcherConfig1 = new Config("{mod.esp}|{id}", IncompleteAction.RemoveLine, true, "");
            var skyPatcherConfig2 = new Config("{mod.esp}|{id}", IncompleteAction.RemoveLine, true, "{mod.esp}.ini");

            // With forward slashes
            Add("file.ini", "random words = 123", null);
            Add("file_DISTR.ini", ";SynthCALIO;keepHeader=no;", new Config("0x{id}~{mod.esp}", IncompleteAction.RemoveLine, false, "{mod}_DISTR.ini"));
            Add("dir/file_DISTR.ini", "; SynthCALIO - Rename={mod}_DISTR.ini;OnIncomplete=RemoveLines;KeepHeader=true;FormID=0x{id}~{mod.esp}", spidConfig);

            Add("SKSE/Plugins/SkyPatcher/file.ext", "// SynthCALIO - Rename=;OnIncomplete=RemoveLines;KeepHeader=true;FormID={mod.esp}|{id}", skyPatcherConfig1);
            Add("SKSE/Plugins/SkyPatcher/file.ini", "; synthCalio - Rename={mod.esp}.ini", skyPatcherConfig2);
        }
    }

    public class Test_Config
    {
        [Theory]
        [ClassData(typeof(ConfigParserTestCases))]
        public void ConfigParserTests (string filePath, Config? expected)
        {
            var config = ConfigParser.FromString(ConfigParser.DEFAULTINI);
            Assert.Equal(expected, config.FindConfig(filePath));
        }

        [Fact]
        public void Default_File ()
        {
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            var _config = ConfigParser.FromFile(tmp);
            File.Delete(tmp);

            Assert.NotNull(_config);
            Assert.Equal(3, _config.ConfigEntries.Count);

            var entry = _config.ConfigEntries[0];
            Assert.NotNull(entry);
            Assert.Equal(typeof(WildCardMatcher), entry.Matcher.GetType());
            Assert.Equal("{mod}_DISTR.ini", entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Equal("0x{id}~{mod.esp}", entry.FormID);

            entry = _config.ConfigEntries[1];
            Assert.NotNull(entry);
            Assert.Equal(typeof(WildCardMatcher), entry.Matcher.GetType());
            Assert.Null(entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Equal("{mod.esp}|{id}", entry.FormID);

            entry = _config.ConfigEntries[2];
            Assert.NotNull(entry);
            Assert.Equal(typeof(RegexMatcher), entry.Matcher.GetType());
            Assert.Equal("{mod.esp}.ini", entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Null(entry.FormID);
        }

        [Fact]
        public void Default_String ()
        {
            var _config = ConfigParser.FromString(ConfigParser.DEFAULTINI);

            Assert.NotNull(_config);
            Assert.Equal(3, _config.ConfigEntries.Count);

            var entry = _config.ConfigEntries[0];
            Assert.NotNull(entry);
            Assert.Equal(typeof(WildCardMatcher), entry.Matcher.GetType());
            Assert.Equal("{mod}_DISTR.ini", entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Equal("0x{id}~{mod.esp}", entry.FormID);

            entry = _config.ConfigEntries[1];
            Assert.NotNull(entry);
            Assert.Equal(typeof(WildCardMatcher), entry.Matcher.GetType());
            Assert.Null(entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Equal("{mod.esp}|{id}", entry.FormID);

            entry = _config.ConfigEntries[2];
            Assert.NotNull(entry);
            Assert.Equal(typeof(RegexMatcher), entry.Matcher.GetType());
            Assert.Equal("{mod.esp}.ini", entry.Rename);
            Assert.Equal(IncompleteAction.RemoveLine, entry.IncompleteAction);
            Assert.True(entry.KeepHeader);
            Assert.Null(entry.FormID);
        }

        [Theory]
        [ClassData(typeof(FullTestCases))]
        public void FullTests (string filePath, string? oneLiner, Config? expected)
        {
            var config = ConfigParser.FromString(ConfigParser.DEFAULTINI);
            Assert.Equal(expected, config.FindConfig(filePath, oneLiner));
        }

        [Fact]
        public void OneLiner ()
        {
            Assert.False(Config.TryFrom1Liner("kjfbdkjbvjkbfkjfkew", out var config));
            Assert.Null(config);

            Assert.False(Config.TryFrom1Liner("; SynthCALIO ", out config));
            Assert.NotNull(config);
            Assert.False(config.IsValid());

            Assert.True(Config.TryFrom1Liner("; SynthCALIO formID=test", out config));
            Assert.NotNull(config);
            Assert.False(config.IsValid());
            Assert.Equal(string.Empty, config.FormID);

            Assert.True(Config.TryFrom1Liner("; SynthCALIO formID={mod}|{id} ; ", out config));
            Assert.NotNull(config);
            Assert.True(config.IsValid());
            Assert.Equal("{mod}|{id}", config.FormID);
            Assert.Null(config.Rename);

            Assert.True(Config.TryFrom1Liner("; SynthCALIO rename=;formID={mod}|{0id}; ", out config));
            Assert.NotNull(config);
            Assert.True(config.IsValid());
            Assert.Equal("{mod}|{0id}", config.FormID);
            Assert.Equal(string.Empty, config.Rename);

            Assert.True(Config.TryFrom1Liner("; SynthCALIO ;formID={mod.esp}|{id};rename=", out config));
            Assert.NotNull(config);
            Assert.True(config.IsValid());
            Assert.Equal("{mod.esp}|{id}", config.FormID);
            Assert.Equal(string.Empty, config.Rename);

            Assert.True(Config.TryFrom1Liner("; synthcalio formID = {0id}|{id}  ; RENAME={mod}.ini;onIncomplete=removeLines   ;  KEEPHeader  = no   ;", out config));
            Assert.NotNull(config);
            Assert.False(config.IsValid());
            Assert.Equal(string.Empty, config.FormID);
            Assert.True(config.ShouldRename);
            Assert.Equal("{mod}.ini", config.Rename);
            Assert.False(config.KeepHeader);
        }

        [Theory]
        [ClassData(typeof(Test_ReplaceTestCases))]
        public void Test_Replace (string filename, string? oneLiner, string modName, string contents, string? expected)
        {
            var configs = ConfigParser.FromString(ConfigParser.DEFAULTINI);

            var editorIDs = new Dictionary<string, uint>()
            {
                ["test1"] = 0x1,
                ["test2"] = 0x21,
                ["test3"] = 0x321,
                ["test4"] = 0x4321,
            };

            var config = configs.FindConfig(filename, oneLiner);
            Assert.NotNull(config);

            bool r = config.TryReplaceText(contents, "something.ini", modName, editorIDs, out string? result, out _);

            Assert.Equal(expected != null, r);

            if (r)
                Assert.Equal(expected, result);
        }
    }

    public class Test_ReplaceTestCases : TheoryData<string, string?, string, string, string?>
    {
        public Test_ReplaceTestCases ()
        {
            Add("SPID test_DISTR.ini", null, "SynthCALIO.esp",
                """
                ; blah blah
                abc={test1},def
                {notValid}
                abc={test4},def
                """,
                """
                ; blah blah
                abc=0x1~SynthCALIO.esp,def
                abc=0x4321~SynthCALIO.esp,def

                """);
            Add("SPID test_DISTR.ini", "; SynthCALIO - onincomplete=skipfile", "SynthCALIO.esp",
                """
                ; blah blah
                abc={test1},def
                {notValid}
                abc={test4},def
                """,
                null);
            Add("SPID test_DISTR.ini", "; SynthCALIO - formid={0id}|{mod}", "SynthCALIO.esp",
                """
                ; blah blah
                abc={test1},def
                {notValid}
                abc={test4},def
                """,
                """
                ; blah blah
                abc=000001|SynthCALIO,def
                abc=004321|SynthCALIO,def

                """);
        }
    }
}