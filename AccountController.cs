using Microsoft.AspNetCore.Mvc;
namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _factory;
        public AccountController(IHttpClientFactory factory) { _factory = factory; }
        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _factory.CreateClient("RaceDayApi");
            var res = await client.PostAsJsonAsync("api/Auth/login", new { Username = username, Password = password });
            if (!res.IsSuccessStatusCode) { ViewBag.Error = "Invalid username or password"; return View(); }
            var data = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (data == null) { ViewBag.Error = "Login failed, try again"; return View(); }
            // API may return role/Role and userId/UserId - handles both casings
            var role = data.ContainsKey("role") ? data["role"]?.ToString() : data.ContainsKey("Role") ? data["Role"]?.ToString() : "";
            var userIdStr = data.ContainsKey("userId") ? data["userId"]?.ToString() : data.ContainsKey("UserId") ? data["UserId"]?.ToString() : "0";
            HttpContext.Session.SetInt32("UserId", int.TryParse(userIdStr, out var uid) ? uid : 0);
            HttpContext.Session.SetString("UserRole", role ?? "");
            HttpContext.Session.SetString("Username", username);
            if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
            return RedirectToAction("Index", "Participant");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string fullName, string role)
        {
            var client = _factory.CreateClient("RaceDayApi");
            var res = await client.PostAsJsonAsync("api/Auth/register", new { Username = username, Email = email, Password = password, FullName = fullName, Role = role });
            if (!res.IsSuccessStatusCode) { ViewBag.Error = "Username exists, try another"; return View(); }
            return RedirectToAction("Login");
        }
        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }
    }
}