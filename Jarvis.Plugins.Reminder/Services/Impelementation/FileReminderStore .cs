using Jarvis.Plugins.Reminder.Models;
using Jarvis.Plugins.Reminder.Services.Interface;
using System.Reflection.Emit;
using System.Text.Json;

namespace Jarvis.Plugins.Reminder.Services.Impelementation
{
    public class FileReminderStore : IReminderStore
    {
        #region Fields

        private readonly string _path;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _json = new() { WriteIndented = true };
        private List<ReminderItemModel> _items = new();

        #endregion

        #region Ctor

        public FileReminderStore(string path)
        {
            _path = path;
            if (File.Exists(_path))
            {
                try
                {
                    var txt = File.ReadAllText(_path);
                    _items = JsonSerializer.Deserialize<List<ReminderItemModel>>(txt) ?? new();
                }
                catch { _items = new(); }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            }
        }

        #endregion

        public async Task AddAsync(ReminderItemModel item, CancellationToken ct = default)
        {
            lock (_lock)
            {
                _items.Add(item);
                Persist();
            }
        }

        public async Task<IReadOnlyList<ReminderItemModel>> GetDueAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            lock (_lock)
            {
                var due = _items.Where(i => !i.Fired && i.DueAt > now).ToList();
                return due;
            }
        }

        public async Task MarkFiredAsync(string id, CancellationToken ct = default)
        {
            lock (_lock)
            {
                var it = _items.FirstOrDefault(x => x.Id == id);
                if(it is not null)
                {
                    it.Fired = true;
                    Persist();
                }
            }
        }

        #region Utilities

        private void Persist()
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(_items, _json));
        }

        #endregion
    }
}