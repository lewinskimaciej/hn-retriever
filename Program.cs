var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ideally use other distributed cache later on but In Memory is fine for now
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient<HnHttpClient>((services, client) =>
{
  var baseAddress = services.GetRequiredService<IConfiguration>().GetValue<string>("HackerNewsApiBaseAddress");
  if (baseAddress is null)
  {
    throw new ApplicationException("HackerNewsApiBaseAddress is required in configuration.");
  }

  client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddSingleton<IHnService, HnService>();

// possibly use assembly scanning to register queries and services like Scrutor, manual is fine for now
builder.Services.AddTransient<IGetBestStoriesQuery, GetBestStoriesQuery>();
builder.Services.AddTransient<IGetItemQuery, GetItemQuery>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.MapGet("/best-stories", async (IHnService hnService, [Range(1, 500)]int? topCount) =>
{
  var stories = await hnService.GetBestStoriesAsync(topCount);
  return Results.Ok(stories);
});

app.UseHttpsRedirection();

// trigger retrieval to warm up cache
var hnService = app.Services.GetRequiredService<IHnService>();
_ = hnService.GetBestStoriesAsync();

app.Run();