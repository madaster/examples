﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using example.client;

namespace example
{
    class Program
    {
        const string environmentUrl = "https://api.madaster.com";

        const string token = "-- REPLACE ME --";

        const string buildingId = "-- REPLACE ME --";

        static async Task Main(string[] args)
        {
            using (var httpClient = new HttpClient() { BaseAddress = new Uri(environmentUrl) })
            {
                httpClient.DefaultRequestHeaders.Add("X-API-Key", token);

                await CallAccountAsync(httpClient);

                await CreateAndUploadFile(httpClient);
            }
        }

        /// <summary>
        /// Call the account API, retrieves account information
        /// </summary>
        static async Task CallAccountAsync(HttpClient httpClient)
        {
            var client = new AccountClient(httpClient);
            var account = await client.GetAccountAsync();
            Console.WriteLine($"Current account: {account.Name}");
        }

        /// <summary>
        /// Call the building file API: 
        /// - creates a new file
        /// - uploads an existing file from disk
        /// - waits for the import to finish
        /// </summary>
        private static async Task CreateAndUploadFile(HttpClient httpClient)
        {
            var fileClient = new BuildingFileClient(httpClient);

            var id = Guid.Parse(buildingId);

            // Create a new file, with the nl-sfb classification which will use material/product matching against the 
            // Madaster database.
            var file = await fileClient.AddFileAsync(id, new BuildingFileRequest()
            {
                Name = "small.ifc",
                Type = BuildingRequestFileType.Source,
                PreferredDatabaseIds = new[] { Guid.Empty },
                ClassificationTypeId = Guid.Parse("88eb09b8-d3f5-4cb1-a732-eb64281a585c") 
            });
            Console.WriteLine($"  - Created file");

            // Open the file from disk as a stream and upload using the API
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "small.ifc");
            if (!System.IO.File.Exists(filePath)) { throw new Exception("The file 'small.ifc' is missing."); }

            using var stream = System.IO.File.OpenRead(filePath);
            await fileClient.UploadFileAsync(file.BuildingId, file.Id, "small.ifc", stream);
            Console.Write($"  - File uploaded");


            // After the file is uploaded, the Madaster platform start importing the file. 
            // Using a poll mechanism we can determine if this process is finished.
            BuildingFileStatuses status = BuildingFileStatuses.Creating;

            var currentStatus = await fileClient.GetStatusByIdAsync(file.BuildingId, file.Id);
            while (currentStatus.Status != BuildingFileStatuses.Mapped)
            {
                if (status != currentStatus.Status) {
                    status = currentStatus.Status;
                    Console.Write(Environment.NewLine);
                    Console.Write($"  => {status}:");
                }

                Console.Write($".");

                await Task.Delay(5000);
                currentStatus = await fileClient.GetStatusByIdAsync(file.BuildingId, file.Id);
            }

            Console.Write(Environment.NewLine);
            Console.Write($"  - File imported and refined");
        }
    }
}
