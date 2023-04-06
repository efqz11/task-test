using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestTask;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}

public class SearchProviderOneService : ISearchService
{
    private static readonly List<ProviderOneRoute> _tmpData = new List<ProviderOneRoute>()
    {
        new ProviderOneRoute {
            From = "Moscow" ,
            To = "Sochi",
            Price=350,
            DateFrom = new DateTime(2023, 04, 10, 09, 05, 0),
            DateTo = new DateTime(2023, 04, 10, 16, 10, 0),
            TimeLimit = new DateTime(2023, 04, 10)
        },
        new ProviderOneRoute {
            From = "Moscow" ,
            To = "Sochi",
            Price=550,
            DateFrom = new DateTime(2023, 04, 10, 15, 15, 0),
            DateTo = new DateTime(2023, 04, 10, 19, 45, 0),
            TimeLimit = new DateTime(2023, 04, 10)
        },
        new ProviderOneRoute {
            From = "St. Petersburg",
            To = "Sochi",
            Price=450,
            DateFrom = new DateTime(2023, 04, 10, 08, 15, 0),
            DateTo = new DateTime(2023, 04, 10, 15, 45, 0),
            TimeLimit = new DateTime(2023, 04, 10)
        },

        new ProviderOneRoute {
            From = "Moscow" ,
            To = "Sochi",
            Price= 600,
            DateFrom = new DateTime(2023, 04, 11, 13, 45, 0),
            DateTo = new DateTime(2023, 04, 11, 16, 15, 0),
            TimeLimit = new DateTime(2023, 04, 11)
        },
        new ProviderOneRoute {
            From = "St. Petersburg",
            To = "Sochi",
            Price=750,
            DateFrom = new DateTime(2023, 04, 11, 08, 15, 0),
            DateTo = new DateTime(2023, 04, 11, 13, 45, 0),
            TimeLimit = new DateTime(2023, 04, 11)
        },
        new ProviderOneRoute {
            From = "St. Petersburg",
            To = "Yakutia",
            Price=1100,
            DateFrom = new DateTime(2023, 04, 12, 20, 40, 0),
            DateTo = new DateTime(2023, 04, 13, 06, 10, 0),
            TimeLimit = new DateTime(2023, 04, 12)
        }
    };

    private CacheService _cacheService;
    public SearchProviderOneService(CacheService cacheService)
    {
        _cacheService = cacheService;
    }


    public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // clean filters
        request.Destination = request.Destination.ToLower();
        request.Origin = request.Origin.ToLower();

        // perform search on mandatory fields
        var query = _tmpData.Where(
            t => t.From.ToLower().Equals(request.Origin)
            && t.To.ToLower().Equals(request.Destination)
            && t.DateFrom.Date == request.OriginDateTime.Date
         );



        // if using cache, populate filter paramter again (if not null)
        if (request.Filters != null && request.Filters.OnlyCached.GetValueOrDefault() == true && _cacheService.GetCachedFilters() != null)
            request.Filters = _cacheService.GetCachedFilters();


        // perform search on optional filters
        if (request.Filters != null) {
            if (request.Filters.DestinationDateTime.HasValue)
                query = query.Where(t => t.DateTo.Date == request.Filters.DestinationDateTime.Value.Date);

            if (request.Filters.MaxPrice.GetValueOrDefault() > 0)
                query = query.Where(t => t.Price <= request.Filters.MaxPrice.Value);

            if (request.Filters.MinTimeLimit.HasValue)
            {
                double totalHours = (TimeSpan.FromMinutes(request.Filters.MinTimeLimit.Value.Minute) + TimeSpan.FromHours(request.Filters.MinTimeLimit.Value.Hour)).TotalHours;

                query = query.Where(t => (t.DateTo - t.DateFrom).TotalHours < totalHours);
            }

            // add filters to cache
            _cacheService.CacheFilters(request.Filters);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // save to cache

        // prepare response
        try
        {
            return new SearchResponse
            {
                MinPrice = query.Select(t => t.Price).Min(),
                MaxPrice = query.Select(t => t.Price).Max(),
                MaxMinutesRoute = query.Select(t => Convert.ToInt32((t.DateTo - t.DateFrom).TotalMinutes)).Max(),
                MinMinutesRoute = query.Select(t => Convert.ToInt32((t.DateTo - t.DateFrom).TotalMinutes)).Min(),
                Routes = query.Select(t => new Route
                {
                    Destination = t.To,
                    Origin = t.From,
                    DestinationDateTime = t.DateTo,
                    OriginDateTime = t.DateFrom,
                    Price = t.Price,
                    TimeLimit = t.TimeLimit,
                }).ToArray()
            };
        }
        catch (Exception ex)
        {
            return default(SearchResponse);
        }
    }


    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new Random().Next(0, 10) % 2 == 0;
    }
}


public class SearchProviderTwoService : ISearchService
{
    private static readonly List<ProviderTwoRoute> _tmpData = new List<ProviderTwoRoute>()
    {
        new ProviderTwoRoute {
            Departure = new ProviderTwoPoint{ Point="Moscow", Date = new DateTime(2023, 04, 13, 15, 0, 0) } ,
            Arrival = new ProviderTwoPoint{ Point="Sochi", Date = new DateTime(2023, 04, 13, 19, 40, 0) },
            Price=350,
            TimeLimit = new DateTime(2023, 04, 13)
        },
        new ProviderTwoRoute {
            Departure = new ProviderTwoPoint{ Point="St. Petersburg", Date = new DateTime(2023, 04, 16, 06, 40, 0) } ,
            Arrival = new ProviderTwoPoint{ Point="Sochi", Date = new DateTime(2023, 04, 16, 11, 20, 0) },
            Price=550,
            TimeLimit = new DateTime(2023, 04, 16)
        },
        new ProviderTwoRoute {
            Departure = new ProviderTwoPoint{ Point="St. Petersburg", Date = new DateTime(2023, 04, 17, 21, 50, 0) } ,
            Arrival = new ProviderTwoPoint{ Point="Yakutia", Date = new DateTime(2023, 04, 18, 08, 20, 0) },
            Price=800,
            TimeLimit = new DateTime(2023, 04, 17)
        }
    };

    private CacheService _cacheService;
    public SearchProviderTwoService(CacheService cacheService)
    {
        _cacheService = cacheService;
    }




    public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // clean filters
        request.Destination = request.Destination.ToLower();
        request.Origin = request.Origin.ToLower();

        // perform search on mandatory fields
        var query = _tmpData.Where(
            t => t.Departure.Point.ToLower().Equals(request.Origin)
            && t.Arrival.Point.ToLower().Equals(request.Destination)
            && t.Departure.Date.Date == request.OriginDateTime.Date
         );


        // if using cache, populate filter paramter again (if not null)
        if (request.Filters != null && request.Filters.OnlyCached.GetValueOrDefault() == true && _cacheService.GetCachedFilters() != null)
            request.Filters = _cacheService.GetCachedFilters();



        // perform search on optional filters
        if (request.Filters != null)
        {
            if (request.Filters.DestinationDateTime.HasValue)
                query = query.Where(t => t.Arrival.Date.Date == request.Filters.DestinationDateTime.Value.Date);

            if (request.Filters.MaxPrice.GetValueOrDefault() > 0)
                query = query.Where(t => t.Price <= request.Filters.MaxPrice.Value);

            if (request.Filters.MinTimeLimit.HasValue)
            {
                double totalHours = (TimeSpan.FromDays(request.Filters.MinTimeLimit.Value.Day) + TimeSpan.FromHours(request.Filters.MinTimeLimit.Value.Hour)).TotalHours;

                query = query.Where(t => (t.Arrival.Date - t.Departure.Date).TotalHours < totalHours);
            }


            // add filters to cache
            _cacheService.CacheFilters(request.Filters);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // prepare response
        try
        {
            return new SearchResponse
            {
                MinPrice = query.Select(t => t.Price).Min(),
                MaxPrice = query.Select(t => t.Price).Max(),
                MaxMinutesRoute = query.Select(t => Convert.ToInt32((t.Arrival.Date - t.Departure.Date).TotalMinutes)).Max(),
                MinMinutesRoute = query.Select(t => Convert.ToInt32((t.Arrival.Date - t.Departure.Date).TotalMinutes)).Min(),
                Routes = query.Select(t => new Route
                {
                    Id = Guid.NewGuid(),
                    Destination = t.Arrival.Point,
                    Origin = t.Departure.Point,
                    DestinationDateTime = t.Departure.Date,
                    OriginDateTime = t.Arrival.Date,
                    Price = t.Price,
                    TimeLimit = t.TimeLimit,
                }).ToArray()
            };
        }
        catch (Exception ex)
        {
            return default(SearchResponse);
        }
    }


    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new Random().Next(0, 10) % 2 == 0;
    }
}


public class SearchRequest
{
    // Mandatory
    // Start point of route, e.g. Moscow 
    public string Origin { get; set; }
    
    // Mandatory
    // End point of route, e.g. Sochi
    public string Destination { get; set; }
    
    // Mandatory
    // Start date of route
    public DateTime OriginDateTime { get; set; }
    
    // Optional
    public SearchFilters? Filters { get; set; }
}

public class SearchFilters
{
    // Optional
    // End date of route
    public DateTime? DestinationDateTime { get; set; }
    
    // Optional
    // Maximum price of route
    public decimal? MaxPrice { get; set; }
    
    // Optional
    // Minimum value of timelimit for route
    public DateTime? MinTimeLimit { get; set; }
    
    // Optional
    // Forcibly search in cached data
    public bool? OnlyCached { get; set; }
}

public class SearchResponse
{
    // Mandatory
    // Array of routes
    public Route[] Routes { get; set; }
    
    // Mandatory
    // The cheapest route
    public decimal MinPrice { get; set; }
    
    // Mandatory
    // Most expensive route
    public decimal MaxPrice { get; set; }
    
    // Mandatory
    // The fastest route
    public int MinMinutesRoute { get; set; }
    
    // Mandatory
    // The longest route
    public int MaxMinutesRoute { get; set; }
}

public class Route
{
    // Mandatory
    // Identifier of the whole route
    public Guid Id { get; set; }
    
    // Mandatory
    // Start point of route
    public string Origin { get; set; }
    
    // Mandatory
    // End point of route
    public string Destination { get; set; }
    
    // Mandatory
    // Start date of route
    public DateTime OriginDateTime { get; set; }
    
    // Mandatory
    // End date of route
    public DateTime DestinationDateTime { get; set; }
    
    // Mandatory
    // Price of route
    public decimal Price { get; set; }
    
    // Mandatory
    // Timelimit. After it expires, route became not actual
    public DateTime TimeLimit { get; set; }
}