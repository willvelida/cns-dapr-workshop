using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TasksTracker.WebPortal.Frontend.Ui.Pages.Tasks.Models;

namespace TasksTracker.WebPortal.Frontend.Ui.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DaprClient _daprClient;

        public List<TaskModel>? TasksList { get; set; }

        [BindProperty]
        public string? TasksCreatedBy { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory, DaprClient daprClient)
        {
            _httpClientFactory = httpClientFactory;
            _daprClient = daprClient;
        }

        public async Task OnGetAsync()
        {
            TasksCreatedBy = Request.Cookies["TasksCreatedByCookie"];
            // direct svc to svc http request
            //var httpClient = _httpClientFactory.CreateClient("BackEndApiExternal");
            //TasksList = await httpClient.GetFromJsonAsync<List<TaskModel>>($"api/tasks?createdBy={TasksCreatedBy}");

            // Invoke via DaprSDK (Invoke HTTP services using DaprClient)
            TasksList = await _daprClient.InvokeMethodAsync<List<TaskModel>>(HttpMethod.Get, "tasksmanager-backend-api", $"api/tasks?createdBy={TasksCreatedBy}");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            // direct svc to svc http request
            //var httpClient = _httpClientFactory.CreateClient("BackEndApiExternal");
            //var result = await httpClient.DeleteAsync($"api/tasks/{id}");

            //Dapr SideCar Invocation
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, "tasksmanager-backend-api", $"api/tasks/{id}");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteAsync(Guid id)
        {
            // direct svc to svc http request
            //var httpClient = _httpClientFactory.CreateClient("BackEndApiExternal");
            //var result = await httpClient.PutAsync($"api/tasks/{id}/markcomplete", null);

            //Dapr SideCar Invocation
            await _daprClient.InvokeMethodAsync(HttpMethod.Put, "tasksmanager-backend-api", $"api/tasks/{id}/markcomplete");

            return RedirectToPage();
        }
    }
}
