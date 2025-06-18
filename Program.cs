using HnStoriesRetriever.HackerNews;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ideally use other distributed cache later on but In Memory is fine for now
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient<HnHttpClient>(opt =>
{
    opt.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();