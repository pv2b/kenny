using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Text.Json.Serialization;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(j => {
    j.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    j.JsonSerializerOptions.WriteIndented = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ApiKeyring, ApiKeyring>()
    .ConfigurePrimaryHttpMessageHandler(sp => 
        new SocketsHttpHandler() {
            MaxConnectionsPerServer = 4
        }
    )
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
    .AddPolicyHandler(
        HttpPolicyExtensions
            .HandleTransientHttpError()
            //.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
    );
builder.Services.AddSingleton<ApiKeyring, ApiKeyring>();
builder.Services.AddSingleton<PmpApiService, PmpApiService>();
builder.Services.AddSingleton<CrawlerCache, CrawlerCache>();
builder.Services.AddHostedService<PmpCrawlerService>();

builder.Host.UseWindowsService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
