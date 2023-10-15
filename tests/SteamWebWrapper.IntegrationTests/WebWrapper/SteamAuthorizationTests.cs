using FluentAssertions;
using Newtonsoft.Json;
using SteamWebWrapper.Core.Implementations;
using SteamWebWrapper.Implementations;
using SteamWebWrapper.IntegrationTests.Fixtures;
using SteamWebWrapper.Interfaces;
using Xunit;

namespace SteamWebWrapper.IntegrationTests.WebWrapper;

public class SteamAuthorizationTests : IClassFixture<SteamHttpClientFixture>
{
    private readonly IMarketWrapper _marketWrapper;

    public SteamAuthorizationTests(SteamHttpClientFixture marketWrapper)
    {
        _marketWrapper = new MarketWrapper(marketWrapper.SteamHttpClient);
    }

    [Fact]
    public async Task GetMarketHistoryTest()
    {
        const long offset = 0;
        const long count = 150;
        var marketHistory = await _marketWrapper.GetMarketHistoryAsync(offset, count);
        
        marketHistory.Should().NotBeNull();
        marketHistory.Assets.Should().NotBeNullOrEmpty();
        marketHistory.Assets.Should().NotBeNullOrEmpty();
        marketHistory.Purchases.Should().NotBeNullOrEmpty();
        marketHistory.Listings.Should().NotBeNullOrEmpty();
        marketHistory.Events.Should().NotBeNullOrEmpty();
    }
}