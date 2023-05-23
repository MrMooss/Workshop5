using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Workshop5.Models;

namespace Workshop5.Controllers
{
    public class HomeController : Controller
    {
        BlobServiceClient blobServiceClient;
        BlobContainerClient blobContainerClient;

        public HomeController()
        {
            // Azure Blob Storage beállítások
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=foodstorage;AccountKey=pwRqBqaCacaLeBbxWGixBuh57eTVtAjypDdeahc5axwdR4CtZNEJeuutMCZms75ludDAudPq6Uby+AStTEZP2Q==;EndpointSuffix=core.windows.net";
            string containerName = "foods";

            // Blob Storage inicializálása
            blobServiceClient = new BlobServiceClient(connectionString);
            blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<IActionResult> Index()
        {
            List<Food> foods = new List<Food>();

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                using (MemoryStream stream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(stream);
                    stream.Position = 0;

                    string json = Encoding.UTF8.GetString(stream.ToArray());
                    Food food = JsonConvert.DeserializeObject<Food>(json);

                    foods.Add(food);
                }
            }

            return View(foods);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Food food, [FromForm] IFormFile imageFile)
        {

            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = food.Id + ".json";
                string json = JsonConvert.SerializeObject(food);

                BlobClient blobClient = blobContainerClient.GetBlobClient(food.Id);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                food.ImageUrl = blobClient.Uri.ToString();
            }

            return RedirectToAction("Index");
        }
    }

}