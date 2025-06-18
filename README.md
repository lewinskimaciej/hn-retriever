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
I originally wrote a service that retrieves Best Stories and then retrieves each Item either from a short lived cache or HN API.
I decided to rework this approach to a background worker that simply retrieves everything once every minute and stores that in an array. This improved performance immensely at the cost of being a 'semi-realtime'.

I would assume that Best Stories and comment counts don't change that often and we don't need an update on exact moment it changes so performance gain is worth the minor delay.

 - I have limited concurrent requests to HackerNews API to 100, as it is the maximum number of concurrent requests. I don't know actual value but it can be freely adjusted.
 - Rate limiting on HN API is done via simple Semaphore. With distributed services we could move towards distributed lock like Redlock to achieve this across multiple instances.
 - We could use a DistributedCache to scale horizontally but just storing an array gives better performance and we know that there won't be too much data to store in memory.
 - I have defaulted HN API errors to empty array for Best Stories list and null for single Items. This is a decision and could be changed to re-throw an error instead.
 - I warm-up the cache on application start to avoid returning empty list on first calls. Downside is that the startup takes a bit longer.
 

Potential changes:
   - Set `ThreadPool.SetMinThreads(Int32, Int32)` to a reasonable amount to allow creation of threads without default 500ms delay - this would especially help with bursty traffic.
   - Use different caching(f.e. distributed redis cache) to allow horizontal scaling, would go along with Redlock for distributed rate limiting instead of a simple Semaphore
   - Remove/optimize LINQ calls which I kept for simplicity
   - Potential retry policies for Http calls
   - Consider updating Best Stories more often than once a minute

## k6.io
I have added a simple k6.io test to check performance.
It increases load over one minute, up to 10_000 VUs(Virtual Users, essentially - concurrent calls).

Build and start the API:  
```
docker compose up --build
```
Run the k6 test (in another terminal):  
```
k6 run ./k6/k6-best-stories.js
```

Results of a run on MBP M3 Pro for 10k concurrent calls:
```bash
❯ k6 run ./k6/k6-best-stories.js

         /\      Grafana   /‾‾/  
    /\  /  \     |\  __   /  /   
   /  \/    \    | |/ /  /   ‾‾\ 
  /          \   |   (  |  (‾)  |
 / __________ \  |_|\_\  \_____/ 

     execution: local
        script: ./k6/k6-best-stories.js
        output: -

     scenarios: (100.00%) 1 scenario, 10000 max VUs, 1m30s max duration (incl. graceful stop):
              * default: Up to 10000 looping VUs for 1m0s over 1 stages (gracefulRampDown: 30s, gracefulStop: 30s)

WARN[0082] Request Failed                                error="Get \"http://localhost:5107/best-stories?topCount=122\": request timeout"

     ✗ status is 200
      ↳  99% — ✓ 1527187 / ✗ 1
     ✗ body is not empty
      ↳  99% — ✓ 1527187 / ✗ 1

     checks.........................: 99.99%  3054374 out of 3054376
     data_received..................: 303 MB  3.4 MB/s
     data_sent......................: 160 MB  1.8 MB/s
     http_req_blocked...............: avg=9.08µs   min=0s       med=1µs      max=174.54ms p(90)=3µs      p(95)=3µs     
     http_req_connecting............: avg=6.97µs   min=0s       med=0s       max=174.46ms p(90)=0s       p(95)=0s      
     http_req_duration..............: avg=98.04ms  min=53µs     med=1.91ms   max=1m0s     p(90)=205.02ms p(95)=601.76ms
       { expected_response:true }...: avg=98ms     min=53µs     med=1.91ms   max=57.41s   p(90)=205.02ms p(95)=601.76ms
     http_req_failed................: 0.00%   1 out of 1527188
     http_req_receiving.............: avg=29.01µs  min=0s       med=6µs      max=101.56ms p(90)=18µs     p(95)=43µs    
     http_req_sending...............: avg=72.04µs  min=1µs      med=3µs      max=56.1ms   p(90)=50µs     p(95)=128µs   
     http_req_tls_handshaking.......: avg=0s       min=0s       med=0s       max=0s       p(90)=0s       p(95)=0s      
     http_req_waiting...............: avg=97.94ms  min=46µs     med=1.87ms   max=1m0s     p(90)=204.81ms p(95)=601.7ms 
     http_reqs......................: 1527188 16957.564317/s
     iteration_duration.............: avg=199.55ms min=100.07ms med=102.33ms max=1m0s     p(90)=306.09ms p(95)=701.63ms
     iterations.....................: 1527188 16957.564317/s
     vus............................: 3       min=3                  max=9950 
     vus_max........................: 10000   min=10000              max=10000


running (1m30.1s), 00000/10000 VUs, 1527188 complete and 3 interrupted iterations
default ✓ [======================================] 00002/10000 VUs  1m0s

```