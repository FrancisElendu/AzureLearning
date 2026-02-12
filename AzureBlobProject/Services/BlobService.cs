
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

            //generate a sas token for the container, because we need to add the container sas token to the blob sas token, because the blob sas token only contains the blob sas signature, and we need the container sas signature to access the blob.
            string sasContainerSignature = "";
            if (blobContainerClient.CanGenerateSasUri)
            {
                BlobSasBuilder blobSasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobContainerClient.Name,
                    Resource = "c",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };
                blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                //Uri sasUri = blobContainerClient.GenerateSasUri(blobSasBuilder);
                //sasContainerSignature = sasUri.Query;
                sasContainerSignature = blobContainerClient.GenerateSasUri(blobSasBuilder).AbsoluteUri.Split('?')[1].ToString();
            }




            await foreach (BlobItem blob in blobs)
            {
                var blobClient = blobContainerClient.GetBlobClient(blob.Name);

                BlobProperties properties = await blobClient.GetPropertiesAsync();

                //generate a sas token for the blob, because we need to add the blob sas token to the blob uri, because the blob uri only contains the blob uri, and we need the blob sas token to access the blob.
                if (blobClient.CanGenerateSasUri)
                {
                    BlobSasBuilder blobSasBuilder = new BlobSasBuilder
                    {
                        //BlobContainerName = containerName,
                        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                        BlobName = blob.Name,
                        Resource = "b",
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                    };
                    blobSasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
                    Uri sasUri = blobClient.GenerateSasUri(blobSasBuilder);
                    // Generate a SAS URI for the blob with read permissions and an expiration time of 1 hour
                    //Uri sasUri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
                    blobList.Add(new BlobModel
                    {
                        Title = properties.Metadata.ContainsKey("Title") ? properties.Metadata["Title"] : string.Empty,
                        Comment = properties.Metadata.ContainsKey("Comment") ? properties.Metadata["Comment"] : string.Empty,
                        //Uri = sasUri.AbsoluteUri //this is for the blob sas uri.
                        Uri = sasUri.AbsoluteUri //+ "?" + sasContainerSignature // we have to add the container sas signature to the blob sas uri, because the blob sas uri only contains the blob sas signature, and we need the container sas signature to access the blob.
                    });
                }
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
