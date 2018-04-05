using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Durwella.UrlShortening
{
    public class UrlShortener
    {
        public IHashScheme HashScheme { get; private set; }
        public IAliasRepository Repository { get; private set; }
        public IUrlUnwrapper UrlUnwrapper { get; private set; }
        public IList<string> ProtectedPaths { get; set; }

        public const int MaximumHashAttempts = 500;

        public UrlShortener(IAliasRepository repository, IHashScheme hashScheme, IUrlUnwrapper urlUnwrapper)
        {
            Repository = repository;
            HashScheme = hashScheme;
            UrlUnwrapper = urlUnwrapper;
            ProtectedPaths = new string[]{};
        }

        public Task<string> Shorten(string url)
        {
            var directUrl = UrlUnwrapper.GetDirectUrl(url);
            return ShortenDirect(directUrl);
        }

        public async Task<string> ShortenWithCustomHash(string url, string customHash)
        {
            if (String.IsNullOrWhiteSpace(customHash))
                throw new ArgumentException("The custom short URL cannot be empty.");
            if (customHash.Length > 100)
                throw new ArgumentException("The custom short URL must be no longer than 100 characters.");
            var unreserved = new Regex(@"[a-z]|[A-Z]|[0-9]|\-|\.|_|\~");
            if (customHash.EndsWith(".") || !customHash.All(c => unreserved.IsMatch(c.ToString()) ))
                throw new ArgumentException(
                    "The custom short URL must only contain letters A ... Z, numbers 0 ... 9 or " + 
                    "dash (-), underscore (_), dot(.), or tilde (~)");
            if (NotAllowed.Contains(customHash))
                throw new ArgumentException("That custom short URL is not available.");
            if (await Repository.ContainsKey(customHash))
                throw new ArgumentException("The given custom short URL is already in use.");

            var directUrl = UrlUnwrapper.GetDirectUrl(url);
            await Repository.Add(customHash, directUrl);

            return customHash;
        }

        private async Task<string> ShortenDirect(string url)
        {
            if (await Repository.ContainsValue(url))
                return await Repository.GetKey(url);
            
            var key = HashScheme.GetKey(url);
            int permutation = 0;
            while (await Repository.ContainsKey(key))
            {
                if (permutation == MaximumHashAttempts)
                    throw new Exception(String.Format("Failed to find a unique hash for url <{0}> after {1} attempts", url, permutation));
                key = HashScheme.GetKey(url, ++permutation);
            }
            await Repository.Add(key, url);

            return key;
        }

        private IList<string> NotAllowed
        {
            get
            {
                return ProtectedPaths.Select(p => 
                        p.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                        .First())
                    .Distinct()
                    .ToList();
            }
        }
    }
}
