using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatMemoryBot
{
    public class MemoryStore
    {
        private readonly string _path;
        private Dictionary<string, List<string>> _mem = new();

        public MemoryStore(string path = "memory.json")
        {
            _path = path;
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                _mem = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                       ?? new Dictionary<string, List<string>>();
            }
        }

        public List<string> GetAll(string userId)
            => _mem.TryGetValue(userId, out var list) ? list : new List<string>();

        public void Add(string userId, string text)
        {
            if (!_mem.ContainsKey(userId)) _mem[userId] = new List<string>();
            _mem[userId].Add(text);
            File.WriteAllText(_path, JsonSerializer.Serialize(
                _mem, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
