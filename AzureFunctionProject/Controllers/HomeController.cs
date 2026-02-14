using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunctionProject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureFunctionProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BlobServiceClient _blobServiceClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _blobServiceClient = blobServiceClient;
        }

        public IActionResult Index()
        {
            var model = new SalesRequest
            {
                CreatedDate = DateTime.Now
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(SalesRequest salesRequest, IFormFile file)
        {
            salesRequest.Id= Guid.NewGuid().ToString();
            //salesRequest.CreatedDate = DateTime.UtcNow;
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost:7246/api/");
            using (var content = new StringContent(JsonConvert.SerializeObject(salesRequest), System.Text.Encoding.UTF8, "application/json"))
            {
                var response = await httpClient.PostAsync("OnSalesUploadWriteToQueue", content);
                var returnValue = await response.Content.ReadAsStringAsync();

            }

            if (file != null)
            {
                var fileName = salesRequest.Id + Path.GetExtension(file.FileName);
                var containClient = _blobServiceClient.GetBlobContainerClient("functionsalesrep");
                var blobClient = containClient.GetBlobClient(fileName);

                var httpHeaders = new BlobHttpHeaders()
                {
                    ContentType = "application/json"
                };

                await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            }

            return RedirectToAction(nameof(Index));
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
    }
}
