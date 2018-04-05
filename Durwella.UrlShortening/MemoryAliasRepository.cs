using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Durwella.UrlShortening
{
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Not relevant")]
    public class MemoryAliasRepository : IAliasRepository
    {
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();
        public Task<string> GetValue(string key)
        {
            return Task.FromResult(_dict[key]);
        }

        public Task Add(string key, string value)
        {
            _dict.Add(key, value);
            return Task.CompletedTask;
        }

        public Task<bool> Remove(string key)
        {
            return Task.FromResult(_dict.Remove(key));
        }

        public Task<bool> ContainsKey(string key)
        {
            return Task.FromResult(_dict.ContainsKey(key));
        }

        public Task<bool> ContainsValue(string value)
        {
            return Task.FromResult(_dict.ContainsValue(value));
        }

        public Task<string> GetKey(string value)
        {
            return Task.FromResult(_dict.Single(t => t.Value == value).Key);
        }
    }
}
