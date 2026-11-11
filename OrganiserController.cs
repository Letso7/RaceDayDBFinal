using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _factory;
        public OrganiserController(IHttpClientFactory factory) { _factory = factory; }
        private bool IsOrganiser() => HttpContext.Session.GetInt32("UserId") != null && HttpContext.Session.GetString("UserRole") == "Organiser";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = _factory.CreateClient("RaceDayApi");
            var all = await client.GetFromJsonAsync<List<EventVM>>("api/Events") ?? new();
            var mine = all.Where(e => e.OrganiserId == userId).ToList();
            var enrols = await client.GetFromJsonAsync<List<EnrolmentVM>>("api/Enrolments") ?? new();
            foreach (var ev in mine) ev.TotalEnrolments = enrols.Count(x => x.EventId == ev.EventId);
            ViewBag.TotalEvents = mine.Count;
            ViewBag.TotalEnrolments = enrols.Count(x => mine.Any(m => m.EventId == x.EventId));
            ViewBag.Upcoming = mine.Where(e => e.EventDate >= DateTime.Today).OrderBy(e => e.EventDate).ToList();
            return View(mine);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(EventVM model, IFormFile? bannerImage)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            var client = _factory.CreateClient("RaceDayApi");
            model.OrganiserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (bannerImage != null)
            {
                try
                {
                    using var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(bannerImage.OpenReadStream()), "file", bannerImage.FileName);
                    var up = await client.PostAsync("api/Events/upload-banner", content);
                    if (up.IsSuccessStatusCode)
                    {
                        var d = await up.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        if (d != null && d.ContainsKey("imageUrl")) model.ImageUrl = d["imageUrl"];
                    }
                }
                catch { /* Banner is optional - continue without it */ }
            }
            var res = await client.PostAsJsonAsync("api/Events", model);
            if (!res.IsSuccessStatusCode) { ViewBag.Error = "Could not create event"; return View(model); }
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Enrolments(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            var client = _factory.CreateClient("RaceDayApi");
            var all = await client.GetFromJsonAsync<List<EnrolmentVM>>("api/Enrolments") ?? new();
            ViewBag.EventId = id;
            return View(all.Where(e => e.EventId == id).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> EnterResults(int eventId, int userId, string finishTime, int position)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            if (!TimeSpan.TryParse(finishTime, out var parsedTime)) return RedirectToAction("Enrolments", new { id = eventId });
            var client = _factory.CreateClient("RaceDayApi");
            var all = await client.GetFromJsonAsync<List<EnrolmentVM>>("api/Enrolments") ?? new();
            var enrol = all.FirstOrDefault(e => e.EventId == eventId && e.UserId == userId);
            if (enrol == null) return RedirectToAction("Enrolments", new { id = eventId });
            await client.PostAsJsonAsync("api/Results", new { EnrolmentId = enrol.EnrolmentId, EventId = eventId, UserId = userId, FinishTime = parsedTime, Position = position });
            return RedirectToAction("Enrolments", new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string categoryName, string description)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            var client = _factory.CreateClient("RaceDayApi");
            await client.PostAsJsonAsync("api/Categories", new { CategoryName = categoryName, Description = description, EventId = eventId });
            return RedirectToAction("Dashboard");
        }
    }
}