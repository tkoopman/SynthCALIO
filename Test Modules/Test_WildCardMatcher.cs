using SynthCALIO.Matcher;

namespace TestModules
{
    public class Test_WildCardMatcher
    {
        [Fact]
        public void WC_Null_Test () => Assert.Throws<ArgumentNullException>(() => new WildCardMatcher(null!));

        [Fact]
        public void WC_StartBS_Test () => Assert.Throws<ArgumentException>(() => new WildCardMatcher("\\*\\*"));

        [Fact]
        public void WC_StartFS_Test () => Assert.Throws<ArgumentException>(() => new WildCardMatcher("/*/*"));

        [Theory]
        [ClassData(typeof(TestCases))]
        public void WC_Test (string wc, string filePath, bool expected) => Assert.Equal(expected, new WildCardMatcher(wc).IsMatch(filePath));

        [Fact]
        public void WC_WhiteSpace_Test () => Assert.Throws<ArgumentException>(() => new WildCardMatcher("    "));

        public class TestCases : TheoryData<string, string, bool>
        {
            public TestCases ()
            {
                // With forward slashes
                Add("*", "file.ext", true);
                Add("*", "dir/file.ext", false);

                Add("*.ext", "file.ext", true);
                Add("*.ext", "file.ext2", false);
                Add("*.ext", "dir/file.ext", false);

                Add("*/*", "file.ext", true);
                Add("*/*", "dir/file.ext", true);
                Add("*/*", "dir1/dir2/file.ext", true);

                Add("*/*.ext", "file.ext", true);
                Add("*/*.ext", "dir/file.ext", true);
                Add("*/*.ext", "dir1/dir2/file.ext", true);
                Add("*/*.ext", "file.ext2", false);
                Add("*/*.ext", "dir/file.ext2", false);
                Add("*/*.ext", "dir1/dir2/file.ext2", false);

                Add("dir/*/*", "file.ext", false);
                Add("dir/*/*", "dir/file.ext", true);
                Add("dir/*/*", "dir/dir2/file.ext", true);
                Add("dir/*/*", "dir1/dir2/file.ext", false);

                Add("dir*/*", "file.ext", false);
                Add("dir*/*", "dir/file.ext", true);
                Add("dir*/*", "dir1/file.ext", true);
                Add("dir*/*", "adir/file.ext", false);
                Add("dir*/*", "dir/dir2/file.ext", false);

                // With back slashes in pattern
                Add("*\\*", "file.ext", true);
                Add("*\\*", "dir/file.ext", true);
                Add("*\\*", "dir1/dir2/file.ext", true);

                Add("*\\*.ext", "file.ext", true);
                Add("*\\*.ext", "dir/file.ext", true);
                Add("*\\*.ext", "dir1/dir2/file.ext", true);
                Add("*\\*.ext", "file.ext2", false);
                Add("*\\*.ext", "dir/file.ext2", false);
                Add("*\\*.ext", "dir1/dir2/file.ext2", false);

                Add("dir\\*\\*", "file.ext", false);
                Add("dir\\*\\*", "dir/file.ext", true);
                Add("dir\\*\\*", "dir/dir2/file.ext", true);
                Add("dir\\*\\*", "dir1/dir2/file.ext", false);

                Add("dir*\\*", "file.ext", false);
                Add("dir*\\*", "dir/file.ext", true);
                Add("dir*\\*", "dir1/file.ext", true);
                Add("dir*\\*", "adir/file.ext", false);
                Add("dir*\\*", "dir/dir2/file.ext", false);

                // With back slashes in filenames
                Add("*", "dir\\file.ext", false);

                Add("*.ext", "dir\\file.ext", false);

                Add("*/*", "dir\\file.ext", true);
                Add("*/*", "dir1\\dir2\\file.ext", true);

                Add("*/*.ext", "dir\\file.ext", true);
                Add("*/*.ext", "dir1\\dir2\\file.ext", true);
                Add("*/*.ext", "dir\\file.ext2", false);
                Add("*/*.ext", "dir1\\dir2\\file.ext2", false);

                Add("dir/*/*", "dir\\file.ext", true);
                Add("dir/*/*", "dir\\dir2\\file.ext", true);
                Add("dir/*/*", "dir1\\dir2\\file.ext", false);

                Add("dir*/*", "dir\\file.ext", true);
                Add("dir*/*", "dir1\\file.ext", true);
                Add("dir*/*", "adir\\file.ext", false);
                Add("dir*/*", "dir\\dir2\\file.ext", false);

                // With bad chars
                Add("*/*", "dir/dir</file.ext", false);
                Add("*/*", "dir/dir\t/file.ext", false);

                // Extra tests
                Add("SKSE/Plugins/SkyPatcher/*/*.ini", "SKSE/Plugins/SkyPatcher/armor/[ELLE] Tavern Maid.esp.ini", true);
            }
        }
    }
}