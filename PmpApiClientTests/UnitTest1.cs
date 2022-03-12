using Xunit;
using PmpApiClient;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace PmpApiClientTests;

public class UnitTest1
{
    BasePmpApiClient PmpApiClient = new PmpApiClientMock();
    [Fact]
    public async void TestApiResponseSuccess()
    {
        ApiResponse<IEnumerable<Resource>>? response = await PmpApiClient.GetResourcesApiResponseAsync();
        Assert.NotNull(response);
#pragma warning disable CS8602
        ApiResponse<IEnumerable<Resource>>.ApiOperation operation = response.Operation;
#pragma warning restore CS8602
        Assert.Equal("GET RESOURCES", operation.Name);
        Assert.Equal("Success", operation.Result.Status);
        Assert.Equal("Resources fetched successfully", operation.Result.Message);
        Assert.Equal(3, operation.TotalRows);
    }

    [Fact]
    public async void TestResources()
    {
        List<Resource> resources = (await PmpApiClient.GetResourcesAsync()).ToList();
        Assert.NotNull(resources);

        Assert.Equal(3, resources.Count);

        Assert.Equal("CentOS Machine", resources[0].Description);
        Assert.Equal("CentOS Machine", resources[0].Name);
        Assert.Equal("301", resources[0].Id);
        Assert.Equal("Linux", resources[0].Type);
        Assert.Equal(3, resources[0].NoOfAccounts);

        Assert.Equal("Cisco IOS Device", resources[1].Description);
        Assert.Equal("Cisco IOS Device", resources[1].Name);
        Assert.Equal("302", resources[1].Id);
        Assert.Equal("Cisco IOS", resources[1].Type);
        Assert.Equal(2, resources[1].NoOfAccounts);

        Assert.Equal("Weblogic Data Source Password", resources[2].Description);
        Assert.Equal("WebLogic Server", resources[2].Name);
        Assert.Equal("303", resources[2].Id);
        Assert.Equal("WebLogic Server", resources[2].Type);
        Assert.Equal(2, resources[2].NoOfAccounts);

    }

    [Fact]
    public async void TestResourceAccountList()
    {
        ResourceAccountList resourceAccountList = await PmpApiClient.GetResourceAccountListAsync("303");

        Assert.Equal("MS SQL server", resourceAccountList.Type);
        Assert.Equal("sqlserver-1", resourceAccountList.DnsName);
        Assert.Equal("http://sqlserver-1/", resourceAccountList.Url);

        var account = resourceAccountList?.Accounts?.First();
        Assert.Equal("308", account?.Id);
    }

    [Fact]
    public async void TestAccountPassword()
    {
        AccountPassword accountPassword = await PmpApiClient.GetAccountPasswordAsync("303", "307");
        string password = accountPassword.Password;

        Assert.Equal("fqxdB7ded@4", password);
    }}