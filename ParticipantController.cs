using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _factory;
        public ParticipantController(IHttpClientFactory factory) { _factory = factory; }
        private bool IsParticipant() => HttpContext.Session.GetInt32("UserId") != null && HttpContext.Session.GetString("UserRole") == "Participant";

        public async Task<IActionResult> Index(string? search, string? eventType)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var client = _factory.CreateClient("RaceDayApi");
            var events = await client.GetFromJsonAsync<List<EventVM>>("api/Events") ?? new();
            if (!string.IsNullOrEmpty(search)) events = events.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(eventType)) events = events.Where(e => e.EventType == eventType).ToList();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var client = _factory.CreateClient("RaceDayApi");
            var ev = await client.GetFromJsonAsync<EventVM>($"api/Events/{id}");
            if (ev == null) return RedirectToAction("Index");
            var cats = await client.GetFromJsonAsync<List<CategoryVM>>("api/Categories") ?? new();
            ViewBag.Categories = cats.Where(c => c.EventId == id).ToList();
            return View(ev);
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = _factory.CreateClient("RaceDayApi");
            await client.PostAsJsonAsync("api/Enrolments", new { UserId = userId, EventId = eventId, CategoryId = categoryId, Status = "Confirmed", EnrolmentDate = DateTime.Now });
   
            return RedirectToAction("MyEvents");
        }

        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = _factory.CreateClient("RaceDayApi");
            var all = await client.GetFromJsonAsync<List<EnrolmentVM>>("api/Enrolments") ?? new();
            var events = await client.GetFromJsonAsync<List<EventVM>>("api/Events") ?? new();
            foreach (var e in all) e.EventName = events.FirstOrDefault(x => x.EventId == e.EventId)?.EventName ?? "";
            return View(all.Where(e => e.UserId == userId).ToList());
        }

        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = _factory.CreateClient("RaceDayApi");
            var results = await client.GetFromJsonAsync<List<ResultVM>>($"api/Results/user/{userId}") ?? new();
            return View(results);
        }

        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = _factory.CreateClient("RaceDayApi");
            var user = await client.GetFromJsonAsync<UserVM>($"api/Users/{userId}");
            return View(user);
        }
    }
}