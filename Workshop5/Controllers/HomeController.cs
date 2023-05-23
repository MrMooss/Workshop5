using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
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
            return View();
        }


        public async Task<IActionResult> ListFood()
        {
            List<Food> foods = new List<Food>();

            var blobItem = blobContainerClient.GetBlobClient("foods.json");

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

            using (MemoryStream stream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    foods = JsonConvert.DeserializeObject<List<Food>>(json);
                }
            }

            return View(foods);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Food food, [FromForm] IFormFile imageFile)
        {

            List<Food> foods = new List<Food>();

            if (imageFile != null && imageFile.Length > 0)
            {
                string imageName = food.Id + "__" + imageFile.Name;

                BlobClient imageBlobClient = blobContainerClient.GetBlobClient(imageName);

                using (var imageStream = imageFile.OpenReadStream())
                {
                    await imageBlobClient.UploadAsync(imageStream);
                }

                food.ImageUrl = imageBlobClient.Uri.ToString();

                
                BlobClient blobClient = blobContainerClient.GetBlobClient("foods.json");

                if (blobClient.Exists())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await blobClient.DownloadToAsync(stream);
                        stream.Position = 0;

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string foodsJson = reader.ReadToEnd();
                            foods = JsonConvert.DeserializeObject<List<Food>>(foodsJson);

                        }
                    }
                }
                foods.Add(food);
                string json = JsonConvert.SerializeObject(foods);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }

            return RedirectToAction("Index");
        }
    }

}