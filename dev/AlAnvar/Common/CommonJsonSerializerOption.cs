using System.Text.Json.Serialization;
using AlAnvar.Models;

namespace AlAnvar.Common;

[JsonSourceGenerationOptions()]
[JsonSerializable(typeof(JsonTranslationFile))]
internal partial class JsonTranslationFileSerializerOption : JsonSerializerContext
{
}
