using System.Text.RegularExpressions;

namespace SynthCALIO.Matcher
{
    public class RegexMatcher (string pattern) : Regex(pattern), IMatcher
    {
    }
}