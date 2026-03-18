using System.Text.Json;

namespace ContactsManager.Infrastructure.Data.Helpers;

public static class DbInializerHelper
{
    public static List<T> GetJsonFileData<T>(string jsonFile)
    {
        var path = Path.Combine(AppContext.BaseDirectory, jsonFile);
        if (!File.Exists(path))
            return new List<T>();

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<List<T>>(json);
        return data ?? new List<T>();
    }
}
