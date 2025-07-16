using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using Noggog;

using Xunit.Abstractions;

namespace Test_Modules
{
    public class ValidateExamples
    {
        private readonly ITestOutputHelper Output;
        private readonly string rawSchema;
        private readonly JSchema schema;

        public ValidateExamples (ITestOutputHelper output)
        {
            Output = output;

            var assembly = typeof(SynthCALIO.JsonConfig).Assembly;
            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SynthCALIO.schema.json") ?? throw new Exception();

            TextReader reader = new StreamReader(stream);
            rawSchema = reader.ReadToEnd();

            schema = JSchema.Parse(rawSchema);
        }

        [Theory]
        [ClassData(typeof(TestData_Examples))]
        public void ValidateExamples_Test (string json)
        {
            var config = JObject.Parse(json);
            Assert.NotNull(config);

            bool valid = config.IsValid(schema, out IList<string> errors);

            errors.ForEach(Output.WriteLine);

            Assert.True(valid);
        }
    }
}