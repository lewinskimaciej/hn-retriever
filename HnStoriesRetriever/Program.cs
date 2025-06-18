using HnStoriesRetriever.Cron;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ideally use other distributed cache later on but In Memory is fine for now
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient<IHnHttpClient, HnHttpClient>((services, client) =>
{
  var baseAddress = services.GetRequiredService<IConfiguration>().GetValue<string>("HackerNewsApiBaseAddress");
  if (baseAddress is null)
  {
    throw new ApplicationException("HackerNewsApiBaseAddress is required in configuration.");
  }

  client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddSingleton<IHnService, HnService>();

builder.Services.AddHostedService<HnCacheRefreshingJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.MapGet("/best-stories", async (IHnService hnService, [Range(1, 500)]int? topCount) =>
{
  return Results.Ok(hnService.Get(topCount));
});

app.UseHttpsRedirection();

app.Run();