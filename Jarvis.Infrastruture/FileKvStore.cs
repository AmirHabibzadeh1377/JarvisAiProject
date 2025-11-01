using System.Text.Json;

namespace Jarvis.Infrastruture
{
    public sealed class FileKvStore
    {
        #region Fields

        private readonly string _path;
        private readonly Dictionary<string, string> _mem = new();

        #endregion

        #region Ctor

        public FileKvStore(string path)
        {
            _path = path;
            if (File.Exists(path))
            {
                _mem = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_path)) ?? new();
            }
        }

        #endregion

        public string? Get(string key)
        {
            _mem.TryGetValue(key, out var value);
            return value ?? string.Empty;
        }

        public void Set(string key, string value)
        {
            _mem[key] = value;
            File.WriteAllText(_path, JsonSerializer.Serialize(_mem, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}