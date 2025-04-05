using System.Text.RegularExpressions;

namespace SynthCALIO.Matcher
{
    public partial class WildCardMatcher : IMatcher
    {
        public readonly string pattern;
        private static readonly char[] directorySeparators = ['/', '\\'];
        private static readonly string re_filename;
        private static readonly string re_path;
        private readonly Regex regex;

        static WildCardMatcher ()
        {
            re_path = "[^" + Regex.Escape(string.Join(string.Empty, Path.GetInvalidFileNameChars().Where(c => !directorySeparators.Contains(c)))) + "]";
            re_filename = "[^" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]";
        }

        public WildCardMatcher (string wcPattern)
        {
            ArgumentNullException.ThrowIfNull(wcPattern);

            wcPattern = wcPattern.Trim();

            if (wcPattern.Length == 0 || directorySeparators.Contains(wcPattern[0]))
                throw new ArgumentException("Invalid wildcard pattern. Must not be: empty string or start with either / or \\.", nameof(wcPattern));

            string? dirPattern = Path.GetDirectoryName(wcPattern)?.Replace('\\', '/');
            string filePattern = Path.GetFileName(wcPattern).Replace('\\', '/');

            filePattern = wildCardToRegex(filePattern, re_filename);

            if (string.IsNullOrEmpty(dirPattern))
            {
                pattern = $"^{filePattern}$";
            }
            else
            {
                dirPattern = wildCardToRegex(dirPattern + "/", re_path);
                pattern = $"^{dirPattern}{filePattern}$";
            }

            regex = new Regex(pattern);
        }

        public bool IsMatch (string relativePath) => regex.IsMatch(relativePath.Replace('\\', '/'));

        [GeneratedRegex(@"((?:^\*/)|(?:/\*/)|\*)")]
        private static partial Regex RE_WildCard ();

        private static string wildCardToRegex (string value, string wildcard)
                            => RE_WildCard().Replace(value, s => s.Value switch
                {
                    "/*/" => $"/(?:{wildcard}*/)?",
                    "*/" => $"(?:{wildcard}*/)?",
                    "*" => $"{re_filename}*",
                    _ => s.Value,
                });
    }
}