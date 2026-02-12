using AzureBlobProject.Models;
using AzureBlobProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobProject.Controllers
{
    public class BlobController : Controller
    {
        private readonly IBlobService _blobService;

        public BlobController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(string containerName)
        {
            var allBlobs = await _blobService.GetAllBlobs(containerName);
            return View(allBlobs);
        }

        [HttpGet]
        public async Task<IActionResult> AddFile(string containerName)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(string containerName, IFormFile file, BlobModel blobModel)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a file to upload.";
                return View();
            }
            var fileName = Path.GetFileNameWithoutExtension(file.FileName)+"_"+Guid.NewGuid()+Path.GetExtension(file.FileName);
            var result = await _blobService.CreateBlob(fileName, file, containerName, blobModel);// { Name = fileName, ContainerName = containerName });
            //var result = await _blobService.CreateBlob(fileName, file, containerName, new BlobModel());// { Name = fileName, ContainerName = containerName });
            if (result)
                //return RedirectToAction("Index", "Container");
                return RedirectToAction("Manage", new
                {
                    containerName
                });
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewFile(string name, string containerName)
        {
            return Redirect(await _blobService.GetBlob(name, containerName));
        }

        public async Task<IActionResult> DeleteFile(string name, string containerName)
        {
            await _blobService.DeleteBlob(name, containerName);
            return RedirectToAction("Manage", new
            {
                containerName
            });
        }
    }
}
