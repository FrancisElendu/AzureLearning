
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
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

            //adds metatdata to the blob, you can add any metadata you want, but keep in mind that the metadata key must be a string, and the value must also be a string. So if you want to save complex data, you can serialize it to a string before saving it to the metadata.
            IDictionary<string, string> metadata = new Dictionary<string, string>
            {
                { "Title", blobModel.Title },
                { "Comment", blobModel.Comment }
            };
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders, metadata);

            ////removes metadata from the blob, you can use this method to remove specific metadata or all metadata from the blob. If you want to remove specific metadata, you can use the Remove method of the IDictionary interface, and pass the key of the metadata you want to remove. If you want to remove all metadata, you can just pass an empty dictionary to the SetMetadataAsync method.
            
            //IDictionary<string, string> emptyMetadata = new Dictionary<string, string>();
            //await blobClient.SetMetadataAsync(emptyMetadata);

            ////or if you want to remove specific metadata, you can do it like this:
            //IDictionary<string, string> metadataToRemove = new Dictionary<string, string>
            //{
            //    { "Title", blobModel.Title }
            //};
            //await blobClient.SetMetadataAsync(metadataToRemove);

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
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();
            List<BlobModel> blobList = new List<BlobModel>();
            string sasContainerSignature = string.Empty;

            if (blobContainerClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobContainerClient.Name,
                    Resource = "C",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                sasContainerSignature = blobContainerClient.GenerateSasUri(sasBuilder).AbsoluteUri.Split('?')[1].ToString();
            }



            await foreach (var blob in blobs)
            {
                var blobClient = blobContainerClient.GetBlobClient(blob.Name);
                var blobModel = new BlobModel()
                {
                    Uri = blobClient.Uri.AbsoluteUri + "?" + sasContainerSignature,
                };

                //if (blobClient.CanGenerateSasUri)
                //{
                //    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                //    {
                //        //BlobContainerName = containerName,
                //        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                //        BlobName = blobClient.Name,
                //        Resource = "b",
                //        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                //    };
                //    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                //    //Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                //    //blobModel.Uri = sasUri.AbsoluteUri;
                //    blobModel.Uri = blobClient.GenerateSasUri(sasBuilder).AbsoluteUri;
                //}

                BlobProperties properties = await blobClient.GetPropertiesAsync();
                if (properties.Metadata.ContainsKey("title"))
                {
                    blobModel.Title = properties.Metadata["title"];
                }
                if (properties.Metadata.ContainsKey("comment"))
                {
                    blobModel.Title = properties.Metadata["comment"];
                }
                blobList.Add(blobModel);
            }
            return blobList;
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
