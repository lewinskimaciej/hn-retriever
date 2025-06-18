# HackerNews Best Stories Retriever API

This is a basic .NET9 API that retrieves the best stories from HackerNews using the official API.

## How to run
- Clone the repository
- With .NET 9 SDK installed, run the following command in the root directory:
```bash
dotnet run --project ./HnStoriesRetriever/HnStoriesRetriever.csproj
```
- The API will be available at `http://localhost:5107`

## Configuration
Default appsettings are provided and contain opinionated set of values:
```json5
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "HnApiMaxConcurrentRequests": 100, // maximum amount of concurrent requests to HackerNews API
  "SlidingStoryCacheTimeInSeconds": 30, // sliding cache time for single stories
  "AbsoluteStoryCacheTimeInSeconds": 120, // maximum cache time for single stories
  "HackerNewsApiBaseAddress": "https://hacker-news.firebaseio.com" // base HackerNews API address
}

```

## Retrieving Best Stories
Available endpoints
 - GET http://localhost:5107/best-stories
   - accepts an optional topCount query parameter that limits number of results.
 - GET http://localhost:5107/openapi/v1.json

## Assumptions & choices
 - I have limited concurrent requests to HackerNews API to 100, as it is the maximum number of concurrent requests. I don't know actual value but it can be freely adjusted.
 - Rate limiting on HN API is done via simple Semaphore. With distributed services we could move towards distributed lock like Redlock to achieve this across multiple instances.
 - I have chosen cache times that seemed reasonable. I don't know how often Best Stories really change, nor how quickly comment counts update.
 - I used In Memory Distributed Cache for caching stories shortly, to reduce amount of calls to HN API. Distributed allows for easier replacement with f.e. Redis.
 - I have defaulted HN API errors to empty array for Best Stories list and null for single Items. This is a decision and could be changed to re-throw an error instead.
 - I warm-up the cache on application start, but it could very well be a list that is refreshed by a background worker so that API requests can always serve cached version.