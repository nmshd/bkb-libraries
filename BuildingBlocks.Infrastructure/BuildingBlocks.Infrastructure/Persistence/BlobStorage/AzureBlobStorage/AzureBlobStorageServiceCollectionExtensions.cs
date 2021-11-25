using System;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.Persistence.BlobStorage;
using Enmeshed.BuildingBlocks.Infrastructure.Persistence.BlobStorage.AzureBlobStorage;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureBlobStorageServiceCollectionExtensions
    {
        public static void AddBlobStorage(this IServiceCollection services, Action<BlobStorageOptions> setupOptions)
        {
            var options = new BlobStorageOptions();
            setupOptions.Invoke(options);

            services.AddBlobStorage(options);
        }


        public static void AddBlobStorage(this IServiceCollection services, BlobStorageOptions options)
        {
            services.AddSingleton(_ =>
            {
                var containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
                containerClient.CreateIfNotExists();

                try
                {
                    containerClient.SetAccessPolicy(PublicAccessType.Blob);
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return containerClient;
            });

            services.AddScoped<IBlobStorage, AzureBlobStorage>();
        }
    }

    public class BlobStorageOptions
    {
#pragma warning disable CS8618
        public string ConnectionString { get; init; }
        public string ContainerName { get; init; }
#pragma warning restore CS8618
    }
}