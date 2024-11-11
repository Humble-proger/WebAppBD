using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAppBD2.Models;
using Microsoft.AspNetCore.Html;

namespace WebAppBD2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Database_Queries ServerBD;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            ServerBD = new Database_Queries();
            ServerBD.ConnectToBD();
        }

        private string? GetOptionString()
        {
            List<string>? towns = this.ServerBD.GetListTownPost();
            if (towns is null) return null;
            string line = "";
            foreach (string town in towns)
            {
                line += $"<option value=\"{town}\">{town}</option>";
            }
            return line;
        }
        public IActionResult ResultTask(string data) {
			ViewBag.results = data;
			return View();
        }
        public IActionResult Index()
        {

            string? result = GetOptionString();
            if (result is null)
            {
                ViewBag.TownPost = new HtmlString("<input type=\"text\" name=\"town_task_1\" />");
            }
            else
            {
                ViewBag.TownPost = new HtmlString($"<select name=\"town_task_1\">{result}</select>");
            }
            object? min_data = this.ServerBD.ExecuteScalarCommand("select MIN(date_post) from spj1");
            object? max_data = this.ServerBD.ExecuteScalarCommand("select MAX(date_post) from spj1");
            result = "<input type=\"date\" name=\"time_start\" ";
            if (min_data is not null)
            {
                result += $"value=\"{((DateTime)min_data).ToString("yyyy-MM-dd")}\" min=\"{((DateTime)min_data).ToString("yyyy-MM-dd")}\" ";
            }
            if (max_data is not null)
            {
                result += $"max=\"{((DateTime)max_data).ToString("yyyy-MM-dd")}\" ";
            }
            result += "/>";
            ViewBag.time_start = new HtmlString(result);
            result = "<input type=\"date\" name=\"time_end\" ";
            if (min_data is not null)
            {
                result += $"min=\"{((DateTime)min_data).ToString("yyyy-MM-dd")}\" ";
            }
            if (max_data is not null)
            {
                result += $"value=\"{((DateTime)max_data).ToString("yyyy-MM-dd")}\" max=\"{((DateTime)max_data).ToString("yyyy-MM-dd")}\" ";
            }
            result += "/>";
            ViewBag.time_end = new HtmlString(result);
            return View();
        }
        [HttpPost]
        public RedirectToActionResult Index(string town_task_1, DateTime time_start, DateTime time_end) {
            string result = $"Town: {town_task_1}; Post On: {time_start}; Post To: {time_end}";
            return RedirectToAction("ResultTask", "Home", new { data = result });
        }

        [HttpPost]
        public RedirectToActionResult Privacy(string name_post, int reiting, string town) {
            object? data = ServerBD.ExecuteScalarCommand("SELECT MAX(n_post) FROM s");
            int max_val = 0;
            if (data is not null)
                max_val = int.Parse(data.ToString()[1..]);
            string n_post = "S" + (max_val + 1);
            int num = this.ServerBD.ExecuteNonQueryCommand($"INSERT INTO s VALUES ('{n_post}','{name_post}',{reiting},'{town}');");
            if (num > 0)
            {
                return RedirectToAction("ResultTask", "Home", new { data = $"Добавлена {num} запись в таблицу 's'." });
            }
            else {
                return RedirectToAction("ResultTask", "Home", new { data = $"Добавлено {num} записей в таблицу 's'." });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        ~HomeController() {
            ServerBD.CloseConnectToBD();
        }
    }
}