using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        
        public async Task<IActionResult> Index(string? search, string? locationFilter, string? dateFilter)
        {
            // Uses the named "RaceDayApi" client from Program.cs  https://localhost:7001/
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            List<EventVM> events = new List<EventVM>();

            try
            {
            
                var response = await client.GetAsync("api/Events");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                   
                    events = JsonSerializer.Deserialize<List<EventVM>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EventVM>();
                }
            }
            catch
            {
                // If the API is not running, show empty list so the page still loads for the demo.
                events = new List<EventVM>();
            }

            
            // Search filters by event name or location.
            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Location filter filters by exact location/city
            if (!string.IsNullOrWhiteSpace(locationFilter))
            {
                events = events.Where(e => e.Location.Equals(locationFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Date filter filters by event date
            if (!string.IsNullOrWhiteSpace(dateFilter) && DateTime.TryParse(dateFilter, out var filterDate))
            {
                events = events.Where(e => e.EventDate.Date == filterDate.Date).ToList();
            }

            // Passes the filtered list to the View, and keeps the filter values so the search boxes stay filled.
            ViewBag.Search = search;
            ViewBag.LocationFilter = locationFilter;
            ViewBag.DateFilter = dateFilter;
            return View(events);
        }

      
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page - shows RequestId when something goes wrong
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}