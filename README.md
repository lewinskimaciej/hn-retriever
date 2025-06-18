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
 - I do not cache Best Stories list itself, call is quick enough and I don't know how often it changes.
 - I used In Memory Distributed Cache for caching stories shortly, to reduce amount of calls to HN API. Distributed allows for easier replacement with f.e. Redis.
 - I have defaulted HN API errors to empty array for Best Stories list and null for single Items. This is a decision and could be changed to re-throw an error instead.
 - I warm-up the cache on application start, but it could very well be a list that is refreshed by a background worker so that API requests can always serve cached version.
 

To further improve performance we could:
   - Refresh the cache in background through a background worker
   - Cache the list of best stories itself shortly, but consider cache length that wouldn't affect functionality too much
   - Set `ThreadPool.SetMinThreads(Int32, Int32)` to a reasonable amount to allow creation of threads without default 500ms delay - this would especially help with bursty traffic.
   - Potentially cache directly in memory without IDistributedCache, but that doesn't scale horizontally
   - Remove/optimize LINQ calls which I kept for simplicity

## k6.io
I have added a simple k6.io test to check performance.
It increases load in stages, 100 Virtual Users(equivalent to max concurrent calls) every 15s up to 500.

Build and start the API:  
```
docker compose up --build
```
Run the k6 test (in another terminal):  
```
k6 run ./k6/k6-best-stories.js
```

Results of a run on MBP M3 Pro:
```bash
❯ k6 run --out json=report.json ./k6/k6-best-stories.js

         /\      Grafana   /‾‾/  
    /\  /  \     |\  __   /  /   
   /  \/    \    | |/ /  /   ‾‾\ 
  /          \   |   (  |  (‾)  |
 / __________ \  |_|\_\  \_____/ 

     execution: local
        script: ./k6/k6-best-stories.js
        output: json (report.json)

     scenarios: (100.00%) 1 scenario, 500 max VUs, 1m45s max duration (incl. graceful stop):
              * default: Up to 500 looping VUs for 1m15s over 5 stages (gracefulRampDown: 30s, gracefulStop: 30s)


     ✓ status is 200
     ✓ body is not empty

     checks.........................: 100.00% 76152 out of 76152
     data_received..................: 2.1 GB  28 MB/s
     data_sent......................: 4.0 MB  53 kB/s
     http_req_blocked...............: avg=11.99µs  min=0s       med=4µs      max=1.39ms   p(90)=8µs      p(95)=10µs  
     http_req_connecting............: avg=5.77µs   min=0s       med=0s       max=1.18ms   p(90)=0s       p(95)=0s    
     http_req_duration..............: avg=396.94ms min=131.49ms med=300.67ms max=2.5s     p(90)=787.34ms p(95)=1.04s 
       { expected_response:true }...: avg=396.94ms min=131.49ms med=300.67ms max=2.5s     p(90)=787.34ms p(95)=1.04s 
     http_req_failed................: 0.00%   0 out of 38076
     http_req_receiving.............: avg=790.43µs min=9µs      med=151µs    max=213.52ms p(90)=890µs    p(95)=1.62ms
     http_req_sending...............: avg=14.39µs  min=2µs      med=11µs     max=1.86ms   p(90)=24µs     p(95)=30µs  
     http_req_tls_handshaking.......: avg=0s       min=0s       med=0s       max=0s       p(90)=0s       p(95)=0s    
     http_req_waiting...............: avg=396.13ms min=131.42ms med=300.52ms max=2.5s     p(90)=782.67ms p(95)=1.04s 
     http_reqs......................: 38076   502.092706/s
     iteration_duration.............: avg=497.4ms  min=232.02ms med=401.04ms max=2.6s     p(90)=887.74ms p(95)=1.14s 
     iterations.....................: 38076   502.092706/s
     vus............................: 499     min=7              max=499
     vus_max........................: 500     min=500            max=500


running (1m15.8s), 000/500 VUs, 38076 complete and 0 interrupted iterations
default ✓ [======================================] 000/500 VUs  1m15s

```