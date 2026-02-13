
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobProject.Services
{
    public class ContainerService : IContainerService
    {
        private readonly BlobServiceClient _blobClient;

        public ContainerService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }
        public async Task CreateContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            //await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);  // use this if you want to make the container public, so that the blobs inside it can be accessed without authentication.

        }

        public async Task DeleteContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            await blobContainerClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetAllContainer()
        {
            List<string> containerNames = new List<string>();
            await foreach (BlobContainerItem blobContainerItem in _blobClient.GetBlobContainersAsync())
            {
                containerNames.Add(blobContainerItem.Name);
            }
            return containerNames;
        }

        public async Task<List<string>> GetAllContainerAndBlobs()
        {
            List<string> containerAndBlobName = new List<string>();
            containerAndBlobName.Add("-------------------Account Name: " + _blobClient.AccountName+"--------------------");
            containerAndBlobName.Add("----------------------------------------------------------------------------------");




            await foreach (BlobContainerItem blobContainerItem in _blobClient.GetBlobContainersAsync())
            {
                containerAndBlobName.Add("-------------"+blobContainerItem.Name);
                BlobContainerClient blobContainer = _blobClient.GetBlobContainerClient(blobContainerItem.Name);
                await foreach (BlobItem blobItem in blobContainer.GetBlobsAsync())
                {
                    //get blob metadata
                    var blobClient = blobContainer.GetBlobClient(blobItem.Name);
                    BlobProperties properties = await blobClient.GetPropertiesAsync();
                    string tempBlobToAdd = blobItem.Name;
                    //if(properties.Metadata.Count > 0)
                    //{
                    //    tempBlobToAdd += " (Metadata: ";
                    //    foreach (var metadata in properties.Metadata)
                    //    {
                    //        tempBlobToAdd += metadata.Key + "=" + metadata.Value + "; ";
                    //    }
                    //    tempBlobToAdd += ")";
                    //}
                    if(properties.Metadata.ContainsKey("title"))
                    {
                        tempBlobToAdd += " (Title: " + properties.Metadata["title"] + ")";
                    }

                    containerAndBlobName.Add("--" + blobItem.Name);
                    //containerAndBlobName.Add("-------------" + blobContainerItem.Name + " / " + blobItem.Name);
                }
                containerAndBlobName.Add("----------------------------------------------------------------------------------");
            }
            return containerAndBlobName;
        }
    }
}
