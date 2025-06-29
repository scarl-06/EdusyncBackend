using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(IConfiguration config)
    {
        var accountName = config["AzureBlobStorage:AccountName"];
        var accountKey = config["AzureBlobStorage:AccountKey"];
        var containerName = config["AzureBlobStorage:ContainerName"];

        var credentials = new StorageSharedKeyCredential(accountName, accountKey);
        var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
        var serviceClient = new BlobServiceClient(blobUri, credentials);
        _containerClient = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        return blobClient.Uri.ToString(); // Save this URL to your Course DB model
    }
}

