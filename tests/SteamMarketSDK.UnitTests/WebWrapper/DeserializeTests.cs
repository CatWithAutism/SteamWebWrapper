using FluentAssertions;
using SteamMarketSDK.Contracts.Entities.Inventory;
using SteamMarketSDK.Contracts.Entities.Market.BuyOrderStatus;
using SteamMarketSDK.Contracts.Entities.Market.ItemOrdersActivity;
using SteamMarketSDK.Contracts.Entities.Market.MyHistory;
using SteamMarketSDK.Contracts.Entities.Market.MyListings;
using SteamMarketSDK.Contracts.Entities.Market.PriceHistory;
using System.Text.Json;
using Xunit;

namespace SteamMarketSDK.UnitTests.WebWrapper;

public class DeserializeTests
{
	[Fact]
	public async Task DeserializeBuyOrderStatusResponseTest()
	{
		const int buyOrderCount = 57;
		const string dataPath = "Data/GetBuyOrderStatusResponse.json";

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var marketHistoryResponse = JsonSerializer.Deserialize<BuyOrderStatusResponse>(data);
		marketHistoryResponse.Should().NotBeNull();
		marketHistoryResponse.Success.Should().Be(1);
		marketHistoryResponse.Purchases.Count.Should().Be(buyOrderCount);
	}

	[Fact]
	public async Task DeserializeInventoryResponseTest()
	{
		const int assetCount = 87;
		const int descriptionCount = 71;
		const string dataPath = "Data/InventoryItemsResponse.json";

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var inventoryResponse = JsonSerializer.Deserialize<InventoryResponse>(data);
		inventoryResponse.Should().NotBeNull();
		inventoryResponse.Assets.Should().NotBeNullOrEmpty();
		inventoryResponse.Assets.Count.Should().Be(assetCount);
		inventoryResponse.Descriptions.Count.Should().Be(descriptionCount);
	}


	[Fact]
	public async Task DeserializeItemOrdersActivityResponseTest()
	{
		const int expectedActivityCount = 13;
		const string dataPath = "Data/GetOrderItemActivityResponse.json";

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var marketHistoryResponse = JsonSerializer.Deserialize<ItemOrdersActivityResponse>(data);
		marketHistoryResponse.Should().NotBeNull();
		marketHistoryResponse.Activity.Count.Should().Be(expectedActivityCount);
	}

	[Fact]
	public async Task DeserializeMarketHistoryResponseTest()
	{
		const int assetCount = 135;
		const int eventCount = 150;
		const int purchaseCount = 110;
		const string dataPath = "Data/MarketHistoryResponse.json";

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var marketHistoryResponse = JsonSerializer.Deserialize<MyHistoryResponse>(data);
		marketHistoryResponse.Should().NotBeNull();
		marketHistoryResponse.Assets.Should().NotBeNullOrEmpty();
		marketHistoryResponse.Assets.Count.Should().Be(assetCount);
		marketHistoryResponse.Purchases.Count.Should().Be(purchaseCount);
		marketHistoryResponse.Listings.Count.Should().Be(assetCount);
		marketHistoryResponse.Events.Count.Should().Be(eventCount);
	}

	[Fact]
	public async Task DeserializeMyListingsResponseTest()
	{
		const int buyOrderCount = 3;
		const int listingCount = 1;
		const int assetCount = 1;
		const string dataPath = "Data/MyListingsResponse.json";

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var marketHistoryResponse = JsonSerializer.Deserialize<MyListingsResponse>(data);
		marketHistoryResponse.Should().NotBeNull();
		marketHistoryResponse.Assets.Should().NotBeNullOrEmpty();
		marketHistoryResponse.Assets.Count.Should().Be(assetCount);
		marketHistoryResponse.Listings.Count.Should().Be(listingCount);
		marketHistoryResponse.BuyOrders.Count.Should().Be(buyOrderCount);
	}

	[Fact]
	public async Task DeserializePriceHistoryResponseTest()
	{
		const string dataPath = "Data/PriceHistoryResponse.json";
		const int countPeriodPrices = 4306;

		var data = await File.ReadAllTextAsync(dataPath);
		data.Should().NotBeNullOrEmpty();

		var marketHistoryResponse = JsonSerializer.Deserialize<PriceHistoryResponse>(data);
		marketHistoryResponse.Should().NotBeNull();
		marketHistoryResponse.PeriodPrices.Count.Should().Be(countPeriodPrices);
	}
}