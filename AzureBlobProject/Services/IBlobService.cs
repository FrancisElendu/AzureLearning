using AzureBlobProject.Models;

namespace AzureBlobProject.Services
{
    public interface IBlobService
    {
        Task<List<string>> GetAllBlobs(string containerName);
        Task<List<BlobModel>> GetAllBlobsWithUri(string containerName);
        Task<string> GetBlob(string Name, string containerName);
        Task<bool> CreateBlob(string Name, IFormFile file, string containerName, BlobModel blobModel);
        Task<bool> DeleteBlob(string Name, string containerName);
        //Task<bool> ContainerExistsAsync(string containerName);
    }
}
