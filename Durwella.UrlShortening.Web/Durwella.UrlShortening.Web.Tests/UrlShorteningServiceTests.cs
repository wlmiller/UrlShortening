using Durwella.UrlShortening.Web.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Durwella.UrlShortening.Web.ServiceInterface;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;

namespace Durwella.UrlShortening.Web.Tests
{
    public class UrlShorteningServiceTest
    {
        class FakeAliasRepository : MemoryAliasRepository { }
        class FakeHashScheme : IHashScheme
        {
            public int LengthPreference { get; set; }
            public string GetKey(string value) { return "123"; }
            public string GetKey(string value, int permutation) { throw new NotImplementedException(); }
        }
        class FakeUrlUnwrapper : IUrlUnwrapper
        {
            public string GetDirectUrl(string url)
            {
                return url.Replace("ex.ampl", "example.com");
            }
        }

        class FakeProtectedPaths : IProtectedPathList
        {
            public IList<string> ProtectedPaths
            {
                get
                {
                    return new[] { "/shorten", "/auth/{how}" };
                }
            }
        }

        private readonly IUrlShorteningService _service;
        public UrlShorteningServiceTest()
        {
           _service = new UrlShorteningService(
               new FakeAliasRepository(), new FakeProtectedPaths(), new FakeHashScheme(), new FakeUrlUnwrapper());
            
        }

        [Fact]
        public async Task ShouldCreateNewShortUrl()
        {
            var givenUrl = "http://ex.ampl/one";

            var shortened = await _service.Shorten(new ShortUrlRequest
            {
                Url = givenUrl
            });

            shortened.Should().Be("123");
        }

        [Fact]
        public async Task ShouldCreateCustomShortUrl()
        {
            var givenUrl = "http://ex.ampl/two";

            var shortened = await _service.Shorten(new ShortUrlRequest
            {
                Url = givenUrl,
                CustomPath = "2"
            });

            shortened.Should().Be("2");
        }

        [Fact]
        public async Task ShouldNotAllowReservedPaths()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.Shorten(new ShortUrlRequest
                {
                    Url = "http://example.com",
                    CustomPath = "shorten"
                }));
        }
    }
}
