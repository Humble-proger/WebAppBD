using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAppBD2.Models;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace WebAppBD2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Database_Queries ServerBD;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            ServerBD = new Database_Queries(_logger);
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

        public static string ConvertToHtmlTable(Dictionary<string, List<string?>> data)
        {
            StringBuilder html = new();

            html.AppendLine("<table class='table'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");

            // Заголовки таблицы
            foreach (var header in data.Keys)
                html.AppendLine($"<th>{header}</th>");
            
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            // Определяем максимальное количество строк
            int maxRows = 0;
            foreach (var list in data.Values)
                if (list.Count > maxRows)
                    maxRows = list.Count;

            // Заполнение строк таблицы
            for (int i = 0; i < maxRows; i++)
            {
                html.AppendLine("<tr>");
                foreach (var key in data.Keys)
                {
                    var value = i < data[key].Count ? data[key][i] : "";
                    html.AppendLine($"<td>{(value ?? "NULL")}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            return html.ToString();
        }

        public IActionResult ResultTask(string data) {
			ViewBag.results = new HtmlString(data);
			return View();
        }
        public IActionResult Index()
        {
            _logger.LogInformation("Connecting to Server: students.ami.nstu.ru");
            ServerBD.ConnectToBD();
            _logger.LogInformation("Start processing GET-request for page: {}", "Task 1");
            _logger.LogInformation("Formation of Option List");
            string? result = GetOptionString();
            if (result == null)
            {
                _logger.LogWarning("Option List not Formed. Forming the input field...");
                ViewBag.TownPost = new HtmlString("<input class=\"text-field__input\" type=\"text\" name=\"town_task_1\" />");
            }
            else
            {
                _logger.LogInformation("Option List Formed.");
                ViewBag.TownPost = new HtmlString($"<select name=\"town_task_1\">{result}</select>");
            }
            _logger.LogInformation("Forming date input fields");
            _logger.LogInformation("Getting the smallest date from the database.");
            object? min_data = this.ServerBD.ExecuteScalarCommand("select MIN(date_post) from spj1");
            _logger.LogInformation("Getting the largest date from the database.");
            object? max_data = this.ServerBD.ExecuteScalarCommand("select MAX(date_post) from spj1");
            _logger.LogInformation("Start forming the first date input field");
            result = "<input class=\"text-field__input\" type=\"date\" name=\"time_start\" ";
            if (min_data != null)
                result += $"value=\"{(DateTime)min_data:yyyy-MM-dd}\" min=\"{(DateTime)min_data:yyyy-MM-dd}\" ";
            else
                _logger.LogWarning("Lower limit missed...");
            if (max_data != null)
                result += $"max=\"{(DateTime)max_data:yyyy-MM-dd}\" ";
            else
                _logger.LogWarning("Upper limit is missing");
            result += "/>";
            ViewBag.time_start = new HtmlString(result);
            _logger.LogInformation("The first date input field has been generated successfully.");
            _logger.LogInformation("Start forming the second date input field");
            result = "<input class=\"text-field__input\" type=\"date\" name=\"time_end\" ";
            if (min_data != null)
                result += $"min=\"{(DateTime)min_data:yyyy-MM-dd}\" ";
            else
                _logger.LogWarning("Lower limit missed...");
            if (max_data != null)
                result += $"value=\"{(DateTime)max_data:yyyy-MM-dd}\" max=\"{(DateTime)max_data:yyyy-MM-dd}\" ";
            else
                _logger.LogWarning("Upper limit is missing");
            result += "/>";
            ViewBag.time_end = new HtmlString(result);
            _logger.LogInformation("The second date input field has been generated successfully.");
            _logger.LogInformation("Get request processing for the page {} completed", "Task 1");
            _logger.LogInformation("The connection is closing...");
            ServerBD.CloseConnectToBD();
            return View();
        }
        [HttpPost]
        public RedirectToActionResult Index(string town_task_1, DateTime time_start, DateTime time_end) {
            _logger.LogInformation("Start processing POST-request for page: {}", "Task 1");
            _logger.LogInformation("Getting the results of task 1...");
            if (time_end >= time_start)
            {
                _logger.LogInformation("Connecting to Server: students.ami.nstu.ru");
                ServerBD.ConnectToBD();
                Dictionary<string, List<string?>>? result = this.ServerBD.ExecuteReaderCommand($"SELECT DISTINCT s.* FROM spj1 JOIN s ON spj1.n_post = s.n_post WHERE date_post >= to_date('{time_start:dd-MM-yyyy}','dd-mm-yyyy') and date_post <= to_date('{time_end:dd-MM-yyyy}','dd-mm-yyyy') and spj1.n_det IN(SELECT DISTINCT p.n_det FROM p WHERE p.town = '{town_task_1.Trim()}')");
                _logger.LogInformation("The connection is closing...");
                ServerBD.CloseConnectToBD();
                if (result == null)
                {
                    _logger.LogWarning("No results found. Generating page with results.");
                    _logger.LogInformation("POST-request processing for the page {} completed", "Task 1");
                    return RedirectToAction("ResultTask", "Home", new { data = "<p>Результаты не найденыю.</p>" });
                }
                _logger.LogInformation("Generating page with results.");
                _logger.LogInformation("POST-request processing for the page {} completed", "Task 1");
                return RedirectToAction("ResultTask", "Home", new { data = ConvertToHtmlTable(result) });
            }
            else {
                _logger.LogError("time_end < time_start. Incorrect data entry.");
                _logger.LogInformation("POST-request processing for the page {} completed", "Task 1");
                return RedirectToAction("ResultTask", "Home", new { data = "<p>Данные введены некорректно.</p>" });
            }
        }

        [HttpPost]
        public RedirectToActionResult Privacy(string name_post, int reiting, string town) {
            _logger.LogInformation("Connecting to Server: students.ami.nstu.ru");
            ServerBD.ConnectToBD();
            _logger.LogInformation("Start processing POST-request for page: {}", "Task 2");
            _logger.LogInformation("Getting the maximum supplier number...");
            object? data = ServerBD.ExecuteScalarCommand("SELECT MAX(n_post) FROM s");
            int max_val = 0;
            if (data != null)
                max_val = int.Parse(data.ToString()[1..]);
            else
                _logger.LogInformation("The table with suppliers is empty");
            string n_post = "S" + (max_val + 1);
            _logger.LogInformation("Getting the results of task 2...");
            int num = this.ServerBD.ExecuteNonQueryCommand($"INSERT INTO s VALUES ('{n_post}','{name_post}',{reiting},'{town}');");
            _logger.LogInformation("The connection is closing...");
            ServerBD.CloseConnectToBD();
            if (num > 0)
            {
                _logger.LogInformation("The entry was added successfully.");
                _logger.LogInformation("POST-request processing for the page {} completed", "Task 2");
                return RedirectToAction("ResultTask", "Home", new { data = $"<p>Добавлена {num} запись в таблицу 's'.</p>" });
            }
            else {
                _logger.LogWarning("The entry has not been added.");
                _logger.LogInformation("POST-request processing for the page {} completed", "Task 2");
                return RedirectToAction("ResultTask", "Home", new { data = $"<p>Добавлено {num} записей в таблицу 's'.</p>" });
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