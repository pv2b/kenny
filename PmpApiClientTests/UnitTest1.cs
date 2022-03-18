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
        ApiResponse<IEnumerable<ResourceSummary>>? response = await PmpApiClient.GetAllResourceSummaryApiResponseAsync();
        Assert.NotNull(response);
#pragma warning disable CS8602
        ApiResponse<IEnumerable<ResourceSummary>>.ApiOperation operation = response.Operation;
#pragma warning restore CS8602
        Assert.Equal("GET RESOURCES", operation.Name);
        Assert.Equal("Success", operation.Result.Status);
        Assert.Equal("Resources fetched successfully", operation.Result.Message);
        Assert.Equal(3, operation.TotalRows);
    }

    [Fact]
    public async void TestResourceSummary()
    {
        List<ResourceSummary> resourceSummaries = (await PmpApiClient.GetAllResourceSummaryAsync()).ToList();
        Assert.NotNull(resourceSummaries);

        Assert.Equal(3, resourceSummaries.Count);

        Assert.Equal("CentOS Machine", resourceSummaries[0].Description);
        Assert.Equal("CentOS Machine", resourceSummaries[0].Name);
        Assert.Equal("301", resourceSummaries[0].Id);
        Assert.Equal("Linux", resourceSummaries[0].Type);
        Assert.Equal(3, resourceSummaries[0].NoOfAccounts);

        Assert.Equal("Cisco IOS Device", resourceSummaries[1].Description);
        Assert.Equal("Cisco IOS Device", resourceSummaries[1].Name);
        Assert.Equal("302", resourceSummaries[1].Id);
        Assert.Equal("Cisco IOS", resourceSummaries[1].Type);
        Assert.Equal(2, resourceSummaries[1].NoOfAccounts);

        Assert.Equal("Weblogic Data Source Password", resourceSummaries[2].Description);
        Assert.Equal("WebLogic Server", resourceSummaries[2].Name);
        Assert.Equal("303", resourceSummaries[2].Id);
        Assert.Equal("WebLogic Server", resourceSummaries[2].Type);
        Assert.Equal(2, resourceSummaries[2].NoOfAccounts);

    }

    [Fact]
    public async void TestResourceDetails()
    {
        ResourceDetails resourceDetails = await PmpApiClient.GetResourceDetailsAsync("303");

        Assert.Equal("MS SQL server", resourceDetails.Type);
        Assert.Equal("sqlserver-1", resourceDetails.DnsName);
        Assert.Equal("http://sqlserver-1/", resourceDetails.Url);

        var account = resourceDetails?.Accounts?.First();
        Assert.Equal("308", account?.Id);
    }

    [Fact]
    public async void TestAccountPassword()
    {
        AccountPassword accountPassword = await PmpApiClient.GetAccountPasswordAsync("303", "307");
        string password = accountPassword.Password;

        Assert.Equal("fqxdB7ded@4", password);
    }

    [Fact]
    public async void TestResourceAssociatedGroups()
    {
        List<ResourceGroupSummary> groups = (await PmpApiClient.GetResourceAssociatedGroupsAsync("1234")).ToList();
        Assert.Single(groups);
        Assert.Equal(301, groups[0].Id);
        Assert.Equal("Default Group", groups[0].Name);
    }
}