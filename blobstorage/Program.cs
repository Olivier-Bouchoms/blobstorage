using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace blobstorage
{
    class Program
    {
        private static string connectionString;

        static async Task Main(string[] args)
        {
            // Douw hier je connectionstring van de azure app storage service in
            connectionString = "";

            var containerClient = await CreateContainerIfNotExists();
            var filePath = await CreateMockFile();
            
            BlobClient blobClient = containerClient.GetBlobClient(filePath);

            await UploadBlob(blobClient, filePath);

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine(blobItem.Name);
                
                string localPath = "/Users/olivier/download";

                var path = Path.Combine(localPath, $"{blobItem.Name.Split('/').LastOrDefault()}");
                
                var dirInfo = new DirectoryInfo(localPath);

                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                using FileStream downloadFileStream = File.OpenWrite(path);
                await download.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }
        }

        static async Task<BlobContainerClient> CreateContainerIfNotExists()
        {
            var containerName = "blobstorageolivier";
            var blobServiceClient = new BlobServiceClient(connectionString);
            
            var containers = blobServiceClient.GetBlobContainers().ToList();

            if (containers.FirstOrDefault(c => c.Name.Equals(containerName)) == null)
            {
                var response = await blobServiceClient.CreateBlobContainerAsync(containerName);
                return response.Value;
            }

            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        static async Task<string> CreateMockFile()
        {
            string localPath = "/Users/olivier/temp";
            
            var dirInfo = new DirectoryInfo(localPath);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            
            string fileName = "quickstart" + Guid.NewGuid() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);

            await File.WriteAllTextAsync(localFilePath, "Hello, World!");

            return localFilePath;
        }

        static async Task UploadBlob(BlobClient blobClient, string filePath)
        {
            await using FileStream uploadFileStream = File.OpenRead(filePath);
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
        }
    }
}