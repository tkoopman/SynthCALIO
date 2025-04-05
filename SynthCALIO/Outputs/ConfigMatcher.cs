using SynthCALIO.Matcher;

namespace SynthCALIO.Outputs
{
    public class ConfigMatcher : Config, IMatcher
    {
        public const string SETTING_REGEX = "Regex";

        public IMatcher Matcher;

        public ConfigMatcher (IMatcher matcher, string? formID, IncompleteAction incompleteAction, bool? keepHeader, string? rename) : base(formID, incompleteAction, keepHeader, rename) => Matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));

        public ConfigMatcher (string wildcard, string? formID, string? incompleteAction, string? regex, string? keepHeader, string? rename) : base(formID, incompleteAction, keepHeader, rename) => Matcher = string.IsNullOrWhiteSpace(regex) ? new WildCardMatcher(wildcard) : new RegexMatcher(regex);

        public bool IsMatch (string filePath) => Matcher.IsMatch(filePath);
    }
}