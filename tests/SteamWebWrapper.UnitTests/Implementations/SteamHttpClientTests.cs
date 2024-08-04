﻿using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using Moq.Protected;
using SteamWebWrapper.Contracts.Entities.Market.BuyOrderStatus;
using SteamWebWrapper.Contracts.Entities.Market.ItemOrdersActivity;
using SteamWebWrapper.Contracts.Entities.Market.MyHistory;
using SteamWebWrapper.Contracts.Entities.Market.MyListings;
using SteamWebWrapper.Contracts.Entities.Market.PriceHistory;
using SteamWebWrapper.Contracts.Entities.Market.Search;
using SteamWebWrapper.Core.Contracts.Interfaces;
using SteamWebWrapper.Core.Implementations;
using SteamWebWrapper.Implementations;
using System.Net;
using Xunit;

namespace SteamWebWrapper.UnitTests.Implementations;

[TestSubject(typeof(SteamHttpClient))]
public class SteamHttpClientTests
{
	public SteamHttpClientTests()
	{
		MockedHandler = new Mock<HttpClientHandler>();
		SteamConvertor = new SteamConvertor();
		SteamHttpClient = new SteamHttpClient(MockedHandler.Object, SteamConvertor);
	}

	private Mock<HttpClientHandler> MockedHandler { get; }
	private SteamConvertor SteamConvertor { get; }
	private ISteamHttpClient SteamHttpClient { get; }

	public static IEnumerable<object[]> SucceedData()
	{
		const string localhost = "https://localhost";
		yield return new object[] { localhost, File.ReadAllText("Data/MarketHistoryResponse.json"), new MyHistoryResponse() };
		yield return new object[] { localhost, File.ReadAllText("Data/GetBuyOrderStatusResponse.json"), new BuyOrderStatusResponse() };
		yield return new object[] { localhost, File.ReadAllText("Data/GetOrderItemActivityResponse.json"), new ItemOrdersActivityResponse() };
		yield return new object[] { localhost, File.ReadAllText("Data/MyListingsResponse.json"), new MyListingsResponse() };
		yield return new object[] { localhost, File.ReadAllText("Data/PriceHistoryResponse.json"), new PriceHistoryResponse() };
		yield return new object[] { localhost, File.ReadAllText("Data/SearchResponse.json"), new SearchResponse() };
	}

	[Theory]
	[MemberData(nameof(SucceedData))]
	public async Task GetObjectAsync_Success<T>(string reqUri, string content, T _) where T : notnull
	{
		var httpResponseMessage = Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(content)
		});

		var httpRequestMessage = new HttpRequestMessage { RequestUri = new Uri(reqUri), Method = HttpMethod.Get };

		MockedHandler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.Is<HttpRequestMessage>(message => message.Equals(httpRequestMessage)),
				ItExpr.IsAny<CancellationToken>())
			.Returns(httpResponseMessage);

		var response = await SteamHttpClient.GetObjectAsync<T>(httpRequestMessage, CancellationToken.None);
		response.Should().NotBeNull();

		MockedHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync",
			Times.Once(),
			ItExpr.Is<HttpRequestMessage>(message => message.Equals(httpRequestMessage)),
			ItExpr.IsAny<CancellationToken>());
	}
}