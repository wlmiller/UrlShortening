using System.Threading.Tasks;

namespace Durwella.UrlShortening
{
    public interface IAliasRepository
    {
        Task Add(string key, string value);
        Task<bool> Remove(string key);
        Task<bool> ContainsKey(string key);
        Task<bool> ContainsValue(string value);
        Task<string> GetKey(string value);
        Task<string> GetValue(string key);
    }
}
