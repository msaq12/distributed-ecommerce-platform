using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace func_utilities_dev.Functions;

public class ImageOptimizationFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<ImageOptimizationFunction> _logger;

    public ImageOptimizationFunction(BlobServiceClient blobServiceClient, ILogger<ImageOptimizationFunction> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    [Function(nameof(ImageOptimizationFunction))]
    public async Task Run(
        [BlobTrigger("uploads/{name}", Connection = "BlobStorageConnectionString")] Stream blobStream,
        string name)
    {
        _logger.LogInformation("Processing uploaded blob: {BlobName}", name);

        var targetContainer = _blobServiceClient.GetBlobContainerClient("product-images");
        await targetContainer.CreateIfNotExistsAsync();

        var targetBlob = targetContainer.GetBlobClient(name);
        await targetBlob.UploadAsync(blobStream, overwrite: true);

        _logger.LogInformation("Blob {BlobName} copied to product-images container", name);
    }
}
