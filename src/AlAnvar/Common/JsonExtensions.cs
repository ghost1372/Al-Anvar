namespace AlAnvar.Common;
public static class JsonExtensions
{
    public static void SerializeToJson(this object? value, string path)
    {
        var content = JsonConvert.SerializeObject(value, Formatting.Indented);
        File.WriteAllText(path, content);
    }

    public static T DeserializeFromJson<T>(this string path)
    {
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
    }
}
