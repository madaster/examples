using System;
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

        static async Task Main(string[] args)
        {
            using (var httpClient = new HttpClient() { BaseAddress = new Uri(environmentUrl) })
            {
                httpClient.DefaultRequestHeaders.Add("X-API-Key", token);

                await CallAccountAsync(httpClient);
                
                var folderTypeId = await GetFolderTypeIdAsync(httpClient);
                if (folderTypeId == null)
                {
                    throw new Exception("No foldertype for Portfolio found");
                }

                var folderId = await GetOrCreateFolderAsync(httpClient, folderTypeId.Value);

                var buildingId = await CreateBuildingAsync(httpClient, folderId);

                await CreateBuildingFileAsync(httpClient, buildingId);

                await CreateMaterialPassportAsync(httpClient, buildingId);
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
        /// Call the account API, retrieves the foldertypes, try to find foldertype with the name 'Portfolio'
        /// </summary>
        static async Task<Guid?> GetFolderTypeIdAsync(HttpClient httpClient)
        {
            var client = new AccountClient(httpClient);
            var folderTypes = await client.GetFolderTypesAsync();
            return folderTypes.FirstOrDefault(ft => ft.Name.Nl == "Portfolio")?.Id;
        }

        /// <summary>
        /// Call the folder API, retrieves all folders, try to find a folder with the name 'API portfolio'. If not exists, create a new folder.
        /// </summary>
        static async Task<Guid> GetOrCreateFolderAsync(HttpClient httpClient, Guid folderTypeId)
        {
            var client = new FolderClient(httpClient);
            var folders = await client.GetFoldersAsync();
            var folder = folders.FirstOrDefault(f => f.Name == "API portfolio");

            if (folder?.Id == null)
            {
                folder = await client.AddFolderAsync(new FolderRequest()
                {
                    FoldertypeId = folderTypeId,
                    Name = "API portfolio"
                });

                Console.WriteLine($"- Created portfolio");
            }
            else
            {
                await client.UpdateFolderAsync(folder.Id, new FolderRequest()
                {
                    FoldertypeId = folderTypeId,
                    ParentId = folder.ParentId,
                    Name = "API portfolio",
                    Description = $"API portfolio: {DateTime.Now}"
                });

                Console.WriteLine($"- Updated portfolio");
            }

            return folder.Id;
        }

        /// <summary>
        /// Call the building API, create a new building with the Madaster material & nl-SFB classification.
        /// </summary>
        static async Task<Guid> CreateBuildingAsync(HttpClient httpClient, Guid folderId)
        {
            var client = new BuildingClient(httpClient);
            var settingsClient = new SystemSettingsClient(httpClient);

            var usages = await settingsClient.GetBuildingUsagesAsync(AcceptLanguage.Nl);
            var classifications = await settingsClient.GetClassificationMethodsAsync();
            var materialClassifications = await settingsClient.GetMaterialClassificationsAsync(AcceptLanguage.Nl);

            var building = await client.AddBuildingAsync(new BuildingRequest()
            {
                FolderId = folderId,
                Name = $"API building: {DateTime.Now}",
                Phase = BuildingPhase.New,
                BuildingUsage = usages.First().Key,
                MaterialClassificationTypeId = materialClassifications.First(mc => mc.Name == "Madaster").Id,
                ClassificationType = classifications.First(c => c.Name.Nl == "NL-SfB").Id,
                LastRenovationDate = DateTimeOffset.UtcNow,
                CompletionDate = DateTimeOffset.UtcNow
            });

            Console.WriteLine($" - Created building");

            return building.Id;
        }

        /// <summary>
        /// Call the building file API, creates a new file:
        /// - set the status to importing 
        /// - creates a new IFC element.
        /// - start refinement (material matching)
        /// </summary>
        static async Task CreateBuildingFileAsync(HttpClient httpClient, Guid buildingId) {
            var fileClient = new BuildingFileClient(httpClient);
            var elementClient = new BuildingFileElementClient(httpClient);

            var file = await fileClient.AddFileAsync(buildingId, new BuildingFileRequest()
            {
                Name = "API ifc file",
                Type = BuildingRequestFileType.Source,
                PreferredDatabaseIds = new[] { Guid.Empty }
            });

            Console.WriteLine($"  - Created file");

            await fileClient.SetImportingAsync(buildingId, file.Id);
            Console.WriteLine($"  => Enable import");

            await elementClient.AddElementAsync(buildingId, file.Id, new BuildingFileElementRequest()
            {
                Id = "ifcelement1",
                Name = "My steel wall",
                MaterialName = "staal",
                PhaseLookup = "casco",
                Volume = 10,
                ClassificationLookup = "21.22",
                ElementClass = "ifcwall",
                TypeName = "walltype 123"
            });
            Console.WriteLine($"   - Created ifc element");

            await fileClient.StartRefinementAsync(buildingId, file.Id);
            Console.Write($"  => Started refinement");

            var status = await fileClient.GetStatusByIdAsync(buildingId, file.Id);
            while (status.Status != BuildingFileStatuses.Mapped)
            {
                Console.Write($".");

                await Task.Delay(5000);
                status = await fileClient.GetStatusByIdAsync(buildingId, file.Id);
            }
            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Call the file API, activates all source files (recalculates the building). After calculation, create a PDF and print the download url
        /// </summary>
        static async Task CreateMaterialPassportAsync(HttpClient httpClient, Guid buildingId)
        {
            var fileClient = new BuildingFileClient(httpClient);
            var buildingClient = new BuildingClient(httpClient);

            var files = await fileClient.GetFilesAsync(buildingId);
            foreach (var file in files.Where(f => f.Type == BuildingFileType.Source && f.IsActive == false))
            {
                await fileClient.SetActiveAsync(buildingId, file.Id, true);
                Console.Write($"  => File {file.FileName} activated, building processing started");
                var building = await buildingClient.GetBuildingByIdAsync(buildingId);
                while (building.IsDirty)
                {
                    Console.Write($".");

                    await Task.Delay(5000);
                    building = await buildingClient.GetBuildingByIdAsync(buildingId);
                }
                Console.Write(Environment.NewLine);
            }
            
            var pdf = await buildingClient.CreatePdfPassportAsync(buildingId, AcceptLanguage.Nl);
            Console.Write($"  => Passport created, processing started");
            while (pdf.Status != BuildingFileStatuses.Uploaded)
            {
                Console.Write($".");

                await Task.Delay(5000);
                pdf = await fileClient.GetFileByIdAsync(buildingId, pdf.Id);
            }
            Console.Write(Environment.NewLine);

            var download = await fileClient.DownloadAsync(buildingId, pdf.Id);
            Console.WriteLine($"Download passport: {download.Url}");
        }
    }
}