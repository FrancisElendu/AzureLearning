
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobProject.Models;

namespace AzureBlobProject.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;

        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }
        public async Task<bool> CreateBlob(string Name, IFormFile file, string containerName, BlobModel blobModel)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(Name);
            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            if (result != null)
            {
                // you can also save the blob metadata here, for example, you can save the blobModel to a database, and link it with the blob uri. We have to inject the repository to save to Db, but for simplicity, I will just comment it out here.
                //blobModel.Id = Guid.NewGuid();
                //blobModel.Name = name;
                //blobModel.ContainerName = containerName;
                //blobModel.BlobUri = blobClient.Uri.ToString();
                //blobModel.ContentType = file.ContentType;
                //blobModel.Size = file.Length;
                //blobModel.UploadedAt = DateTime.UtcNow;

                //await _blobRepository.AddAsync(blobModel);
                //await _blobRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteBlob(string Name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(Name);
            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetAllBlobs(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();
            List<string> blobNames = new List<string>();
            await foreach (BlobItem blobItem in blobs)
            {
                blobNames.Add(blobItem.Name);
            }
            return blobNames;

        }

        public async Task<List<BlobModel>> GetAllBlobsWithUri(string containerName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetBlob(string Name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(Name);
            if (!blobClient.Exists())
            {
                throw new Exception("Blob not found");
            }
            return blobClient.Uri.AbsoluteUri;
        }
    }
}
