using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Durwella.UrlShortening.Tests
{
    public class UrlShortenerTest
    {
        private UrlShortener _subject;
        private readonly Mock<IUrlUnwrapper> _mockUnwrapper;
        private readonly Mock<IAliasRepository> _mockRepository;

        public Mock<IHashScheme> MockHashScheme { get; set; }

        public UrlShortenerTest()
        {
            MockHashScheme = new Mock<IHashScheme>();
            _mockRepository = new Mock<IAliasRepository>();
            _mockUnwrapper = new Mock<IUrlUnwrapper>();
            _mockUnwrapper.Setup(u => u.GetDirectUrl(It.IsAny<string>())).Returns<string>(s => s);
            _subject = new UrlShortener(_mockRepository.Object, MockHashScheme.Object, _mockUnwrapper.Object);
        }

        private void SetupWithMemoryRepository()
        {
            _subject = new UrlShortener(new MemoryAliasRepository(), MockHashScheme.Object, _mockUnwrapper.Object);
        }

        [Fact]
        public async Task ShouldSaveAndReturnHash()
        {
            var url = "http://example.com/foo/bar";
            var hash = "123abc";
            MockHashScheme.Setup(h => h.GetKey(url)).Returns(hash);

            var shortened = await _subject.Shorten(url);

            shortened.Should().Be("123abc");
            _mockRepository.Verify(r => r.Add(hash, url));
        }

        [Fact]
        public async Task ShouldReturnExistingHash()
        {
            var url = "http://example.com/a/b/c";
            var hash = "abc";
            _mockRepository.Setup(r => r.ContainsValue(url)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.GetKey(url)).ReturnsAsync(hash);

            var shortened = await _subject.Shorten(url);

            shortened.Should().Be("abc");
            _mockRepository.Verify(r => r.Add(hash, url), Times.Never());
            MockHashScheme.Verify(h => h.GetKey(url), Times.Never());
        }

        [Fact]
        public async Task HandleHashCollision()
        {
            var url = "http://example.com/hash/collision";
            var hash1 = "aaaa";
            var hash2 = "bbbb";
            var hash3 = "cccc";
            MockHashScheme.Setup(h => h.GetKey(url)).Returns(hash1);
            MockHashScheme.Setup(h => h.GetKey(url, 0)).Returns(hash1);
            MockHashScheme.Setup(h => h.GetKey(url, 1)).Returns(hash2);
            MockHashScheme.Setup(h => h.GetKey(url, 2)).Returns(hash3);
            _mockRepository.Setup(r => r.ContainsValue(url)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.ContainsKey(hash1)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.ContainsKey(hash2)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.ContainsKey(hash3)).ReturnsAsync(false);

            var shortened = await _subject.Shorten(url);

            shortened.Should().Be("cccc");
            _mockRepository.Verify(r => r.Add(hash3, url));
        }

        [Fact]
        public async Task ShouldUnwrapUrlBeforeShortening()
        {
            var givenUrl = "http://t.co/123";
            var url = "http://example.com/abc";
            var hash = "foo";
            _mockUnwrapper.Setup(u => u.GetDirectUrl(givenUrl)).Returns(url);
            MockHashScheme.Setup(h => h.GetKey(url)).Returns(hash);

            var shortened = await _subject.Shorten(givenUrl);

            shortened.Should().Be("foo");
            _mockRepository.Verify(r => r.Add(hash, url));
        }

        [Fact]
        public async Task CanShortenWithCustomUrl()
        {
            SetupWithMemoryRepository();
            var url = "http://example.com/1/2/3";
            var hash = "asdf";
            MockHashScheme.Setup(h => h.GetKey(url)).Returns(hash);
            await _subject.Shorten(url);
            var customHash = "123";

            string customShortened = await _subject.ShortenWithCustomHash(url, customHash);

            // This is a change from the original version.
            // Replacing an old hash with a new custom one will break existing links.
            // So if a link is created without a custom link and then with one,
            // just allow two entries in the repo.
            (await _subject.Repository.ContainsKey(hash)).Should().BeTrue();
            (await _subject.Repository.GetValue(customHash)).Should().Be(url);
            customShortened.Should().Be("123");
        }

        [Fact]
        public async Task ShouldThrowIfCustomAlreadyInUse()
        {
            SetupWithMemoryRepository();
            var url = "http://example.com/1/2/3";
            var hash = "asdf";
            MockHashScheme.Setup(h => h.GetKey(url)).Returns(hash);
            await _subject.Shorten(url);
            var customHash = "123";
            await _subject.Repository.Add(customHash, "existing");
            var thrown = false;

            try
            {
                await _subject.ShortenWithCustomHash(url, customHash);
            }
            catch (ArgumentException exception)
            {
                exception.Message.Should().Contain("already").And.Contain("use");
                thrown = true;
            }

            thrown.Should().BeTrue("Should throw exception for existing key");
            (await _subject.Repository.GetValue(customHash)).Should().Be("existing");
            (await _subject.Repository.GetValue(hash)).Should().Be(url);
        }

        [Fact]
        public async Task UseUnwrappedUrlForSettingCustomHash()
        {
            var givenUrl = "http://t.co/123";
            var url = "http://example.com/abc";
            var hash = "custom";
            _mockUnwrapper.Setup(u => u.GetDirectUrl(givenUrl)).Returns(url);

            var shortened = await _subject.ShortenWithCustomHash(givenUrl, hash);

            shortened.Should().Be("custom");
            _mockRepository.Verify(r => r.Add(hash, url));
        }

        [Fact]
        public async Task WhenNoPreviousHashShouldNotTryGetOrRemove()
        {
            SetupWithMemoryRepository();
            var url = "http://example.com/1/2/3";
            var customHash = "T2";

            string customShortened = await _subject.ShortenWithCustomHash(url, customHash);

            // MemoryAliasRepository will throw if we try to GetKey when url not already present
            (await _subject.Repository.GetValue(customHash)).Should().Be(url);
            customShortened.Should().Be("T2");
        }

        [Fact]
        public async Task ThrowWhenInvalidCustomPath()
        {
            // http://www.ietf.org/rfc/rfc3986.txt
            // unreserved  = ALPHA / DIGIT / "-" / "." / "_" / "~"
            await ExpectExceptionForCustomPaths(
                "/", "\\", "!", "@", "#", ":", "$", "%", "^", "&", "*", "(", ")",
                "`", ",", "<", ">", "?",
                "abc?", "\n", "\t", "B.1/23",
                ".", "..", "...", "a.", "a..",
                new String('a', 101)    // Limit to 100 characters. Real limit is around 2k, but why support more than 100?
                );
        }

        [Fact]
        public async Task ThrowWhenProtectedPath()
        {
            _subject.ProtectedPaths = new[]
            {
                "illegal",
                "not.allowed",
                "1/2",
                "/no/{scrubs}",
                "/auth",
                "/auth/basic"
            };
            await ExpectExceptionForCustomPaths("illegal", "no", "auth", "1", "not.allowed");
            await _subject.ShortenWithCustomHash("a", "not");
            await _subject.ShortenWithCustomHash("b", "not.illegal");
            await _subject.ShortenWithCustomHash("c", "1_");
            await _subject.ShortenWithCustomHash("d", "2");
        }

        [Fact]
        public async Task DoNotThrowWhenValidCharactersInCustomPath()
        {
            var custom = new[] { "a", "z", "Z1", "1M", "~~~", ".1", "_-~.abcXYZ" };
            foreach (var c in custom)
                await _subject.ShortenWithCustomHash("http://example.com", c);
        }

        private async Task ExpectExceptionForCustomPaths(params string[] custom)
        {
            foreach (var c in custom)
                await ExpectExceptionForCustomPath(c);
        }

        private async Task ExpectExceptionForCustomPath(string custom)
        {
            try
            {
                await _subject.ShortenWithCustomHash("http://example.com", custom);
            }
            catch (Exception)
            {
                return;
            }
            Assert.True(false, "Expected exception for custom path: " + custom);
        }
    }
}
