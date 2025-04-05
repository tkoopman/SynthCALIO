using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Noggog;

namespace SynthCALIO.Outputs
{
    public partial class Config
    {
        public const string SETTING_FORMID = "FormID";
        public const string SETTING_KEEPHEADER = "KeepHeader";
        public const string SETTING_ONINCOMPLETE = "OnIncomplete";
        public const string SETTING_RENAME = "Rename";

        public const string VARIABLE_0ID = "{0id}";
        public const string VARIABLE_ID = "{id}";
        public const string VARIABLE_MOD = "{mod}";
        public const string VARIABLE_MOD_ESP = "{mod.esp}";

        private static readonly string[] TRUE =
        [
            "y",
            "yes",
            "true"
        ];

        private string? formID;
        private IncompleteAction incompleteAction;
        private bool? keepHeader;
        private string? rename;

        public Config (string? formID, IncompleteAction incompleteAction, bool? keepHeader, string? rename)
        {
            FormID = formID;
            this.incompleteAction = incompleteAction;
            this.keepHeader = keepHeader;
            Rename = rename;
        }

        public Config (string? formID, string? incompleteAction, string? keepHeader, string? rename)
        {
            FormID = formID;
            setIncompleteAction(incompleteAction);
            setKeepHeader(keepHeader);
            Rename = rename;
        }

        private Config ()
        {
        }

        public string? FormID
        {
            get => formID;

            private set => formID
                = value.IsNullOrWhitespace()
                || (!value.Contains(VARIABLE_MOD, StringComparison.OrdinalIgnoreCase) && !value.Contains(VARIABLE_MOD_ESP, StringComparison.OrdinalIgnoreCase))
                || (!value.Contains(VARIABLE_ID, StringComparison.OrdinalIgnoreCase) && !value.Contains(VARIABLE_0ID, StringComparison.OrdinalIgnoreCase))
                ? value is null ? null : string.Empty
                : value.Trim();
        }

        public IncompleteAction IncompleteAction => incompleteAction == IncompleteAction.None ? IncompleteAction.RemoveLine : incompleteAction;

        public bool KeepHeader => keepHeader ?? true;

        public string? Rename { get => rename; private set => rename = value?.Trim(); }

        /// <summary>
        ///     Should rename only if rename has some content. Rename is null then we using default
        ///     value which is not to rename. Rename is "" then that = don't rename
        /// </summary>
        [MemberNotNullWhen(true, nameof(Rename))]
        public bool ShouldRename => Rename != null && Rename.Length != 0;

        public static Config Merge (Config highPriority, Config? lowPriority) => lowPriority is null ? highPriority : new()
        {
            FormID = highPriority.FormID is null ? lowPriority.FormID : highPriority.FormID,
            incompleteAction = highPriority.incompleteAction == IncompleteAction.None ? lowPriority.incompleteAction : highPriority.incompleteAction,
            keepHeader = highPriority.keepHeader is null ? lowPriority.keepHeader : highPriority.keepHeader,
            Rename = highPriority.Rename is null ? lowPriority.Rename : highPriority.Rename
        };

        public static bool TryFrom1Liner (string? line, [NotNullWhen(true)] out Config? config)
        {
            config = null;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var m = OneLinerRegex().Match(line);
            if (m == null || m.Groups.Count != 2)
                return false;

            config = new Config();
            var keyValuePairs = KeyValuePairRegex().Matches(m.Groups[1].Value);
            foreach (Match kvp in keyValuePairs)
                _ = config.tryUpdate(kvp.Groups["key"].Value, kvp.Groups["value"].Value);

            return keyValuePairs.Count > 0;
        }

        public override bool Equals (object? obj) => Equals(obj as Config);

        public bool Equals (Config? other)
            => other is not null
            && FormID == other.FormID
            && IncompleteAction == other.IncompleteAction
            && KeepHeader == other.KeepHeader
            && ((string.IsNullOrEmpty(Rename) && string.IsNullOrEmpty(Rename)) || string.Equals(Rename, other.Rename));

        public override int GetHashCode ()
        {
            var hash = new HashCode();
            hash.Add(FormID);
            hash.Add(IncompleteAction);
            hash.Add(KeepHeader);

            return hash.ToHashCode();
        }

        [MemberNotNullWhen(true, nameof(FormID))]
        public bool IsValid () => !string.IsNullOrEmpty(FormID);

        public bool TryReplaceText (string contents, string source, FileName modFilename, IReadOnlyDictionary<string, uint> editorIDs, [NotNullWhen(true)] out string? outputContents, [NotNullWhen(true)] out string outputFilePath)
        {
            outputContents = null;
            outputFilePath = source;

            if (string.IsNullOrWhiteSpace(contents) || !IsValid())
                return false;

            var replace = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [VARIABLE_MOD] = modFilename.NameWithoutExtension,
                [VARIABLE_MOD_ESP] = modFilename.String,
            };

            if (ShouldRename)
            {
                if (!tryReplaceText(Rename, replace, out string? outputName, out _))
                {
                    Console.WriteLine($"Failed to replace all variables in rename value: {Rename}");
                    return false;
                }

                string? sourcePath = Path.GetDirectoryName(source);

                outputFilePath = sourcePath is not null
                    ? Path.Combine(sourcePath, outputName)
                    : outputName ?? source;
            }

            foreach (var (editorID, formID) in editorIDs)
            {
                replace[VARIABLE_ID] = formID.ToString("X");
                replace[VARIABLE_0ID] = formID.ToString("X6");
                replace['{' + editorID + '}'] = replaceText(FormID, replace);
            }

            if (IncompleteAction == IncompleteAction.RemoveLine)
            {
                StringReader reader = new(contents);
                StringWriter writer = new();

                string? line;
                int total = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (tryReplaceText(line, replace, out string? replacedLine, out int count))
                    {
                        total += count;
                        writer.WriteLine(replacedLine);
                    }
                }

                outputContents = writer.ToString();
                return total > 0;
            }

            return tryReplaceText(contents, replace, out outputContents, out int totalCount) && totalCount > 0;
        }

        [GeneratedRegex(@"\b(?'key'\w+)\s*=\s*(?'value'[^;]*)[;\s]*")]
        private static partial Regex KeyValuePairRegex ();

        [GeneratedRegex(@"^(?:;|#|\/\/)[\s-;]*SynthCALIO[ ;-]+(.*)$", RegexOptions.IgnoreCase)]
        private static partial Regex OneLinerRegex ();

        [GeneratedRegex(@"(\{[\w \.]+\})")]
        private static partial Regex VariableRegex ();

        private string replaceText (string contents, Dictionary<string, string> replace)
            => VariableRegex().Replace(contents, m => replace.TryGetValue(m.Value, out string? value) ? value : m.Value);

        private void setIncompleteAction (string? value)
                    => incompleteAction
            = string.IsNullOrWhiteSpace(value) || !Enum.TryParse(value, ignoreCase: true, out IncompleteAction ia)
            ? incompleteAction = IncompleteAction.None
            : ia;

        private void setKeepHeader (string? value)
            => keepHeader
            = string.IsNullOrWhiteSpace(value)
            ? null
            : TRUE.Contains(value, StringComparer.OrdinalIgnoreCase);

        private bool tryReplaceText (string contents, Dictionary<string, string> replace, [NotNullWhen(true)] out string? result, out int count)
        {
            result = null;
            count = 0;
            if (!IsValid())
                return false;

            int c = 0;
            bool skipped = false;

            result = VariableRegex().Replace(contents, m =>
            {
                if (replace.TryGetValue(m.Value, out string? value))
                {
                    c++;
                    return value;
                }

                skipped = true;
                return m.Value;
            });

            count = c;
            return !skipped;
        }

        private bool tryUpdate (string key, string? value)
        {
            if (key.Equals(SETTING_FORMID, StringComparison.OrdinalIgnoreCase))
                FormID = value;
            else if (key.Equals(SETTING_ONINCOMPLETE, StringComparison.OrdinalIgnoreCase))
                setIncompleteAction(value);
            else if (key.Equals(SETTING_KEEPHEADER, StringComparison.OrdinalIgnoreCase))
                setKeepHeader(value);
            else if (key.Equals(SETTING_RENAME, StringComparison.OrdinalIgnoreCase))
                Rename = value;
            else
                return false;

            return true;
        }
    }
}