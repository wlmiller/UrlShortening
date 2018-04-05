using System.Net;
using FluentAssertions;
using Moq;
using Xunit;

namespace Durwella.UrlShortening.Tests
{
    public class WebClientUrlUnwrapperTest
    {
        [Fact]
        public void ShouldGetResourceLocation()
        {
            var wrappedUrl = "http://goo.gl/mSkqOi";
            var config = new Mock<IConfigSettings>();
            config.Setup(c => c.ResolveUrls).Returns(true);

            var subject = new WebClientUrlUnwrapper(config.Object);

            var directUrl = subject.GetDirectUrl(wrappedUrl);

            directUrl.Should().Be("http://example.com/");
        }

        [Fact]
        public void ShouldReturnGivenLocationIfAuthenticationRequired()
        {
            var givenUrl = "http://durwella.com/testing/does-not-exist";
            var config = new Mock<IConfigSettings>();
            config.Setup(c => c.ResolveUrls).Returns(true);
            config.Setup(c => c.IgnoreErrorCodes).Returns(new[] { HttpStatusCode.NotFound });

            var subject = new WebClientUrlUnwrapper(config.Object);

            var directUrl = subject.GetDirectUrl(givenUrl);

            directUrl.Should().Be(givenUrl);
        }
    }
}
