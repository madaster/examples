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

                var productId = await GetOrCreateProductAsync(httpClient);

                var folderTypeId = await GetFolderTypeIdAsync(httpClient);
                if (folderTypeId == null)
                {
                    throw new Exception("No foldertype for Portfolio found");
                }

                var folderId = await GetOrCreateFolderAsync(httpClient, folderTypeId.Value);

                var buildingId = await CreateBuildingAsync(httpClient, folderId);

                await CreateBuildingFileAsync(httpClient, buildingId, productId);

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
        /// Call the database
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        private static async Task<Guid> GetOrCreateProductAsync(HttpClient httpClient)
        {
            var dbClient = new DatabaseClient(httpClient);
            var productClient = new ProductClient(httpClient);

            var databases = await dbClient.GetDatabasesAsync(AcceptLanguage.Nl);
            
            // try to find a database in the account, create if not found
            var db = databases.FirstOrDefault(d => d.Name.Nl == "Example database");
            if (db == null) {
                db = await dbClient.CreateDatabaseAsync(new DatabaseRequest() { Name = new MultiLingualString() { Nl = "Example database" }, InitiallySelectedForEnrichment = true });
            }

            // try to find a product in the database, create if not found
            var products = await productClient.GetProductsAsync(db.Id);
            var product = products.FirstOrDefault(p => p.Name.Nl == "Beglazing_meervoudig");
            if (product == null) 
            {
                product = await productClient.AddProductAsync(db.Id, new ProductRequest() {
                    Name= new MultiLingualString() { Nl = "Beglazing_meervoudig" },
                    ProductType = ProductType.Volume,
                    FunctionalLifetime = 100,
                    TechnicalLifetime = 100
                });

                // Add the search criterea, for matching within the ifc files
                await productClient.AddMatchAsync(db.Id, product.Id, new MatchingCriterion() { LanguageCode = null, MatchType = MatchingCriterionType.Contains, Value = "Beglazing_meervoudig" });
                await productClient.AddMatchAsync(db.Id, product.Id, new MatchingCriterion() { LanguageCode = null, MatchType = MatchingCriterionType.Contains, Value = "Dubbelglas" });
                await productClient.AddMatchAsync(db.Id, product.Id, new MatchingCriterion() { LanguageCode = null, MatchType = MatchingCriterionType.Contains, Value = "Tripleglas" });
                await productClient.AddMatchAsync(db.Id, product.Id, new MatchingCriterion() { LanguageCode = null, MatchType = MatchingCriterionType.Contains, Value = "Isolatieglas" });

                // Add the product decomposition, the material IDs are used from the standard Madaster database. You could use a material client to search for them.
                await productClient.AddChildAsync(db.Id, product.Id, new ProductChild() { ChildId = Guid.Parse("b499c354-fa05-4fc5-8ee4-8df89d09da4a"), Value = 0.6, Circular = new ProductChildCircularInformation() { InheritEfficiencyPercentages = true, InheritEndOfLifePercentages = true, InheritFeedstockPercentages = true } });
                await productClient.AddChildAsync(db.Id, product.Id, new ProductChild() { ChildId = Guid.Parse("e8f9eb37-4cf0-4f75-809a-1a2a2de6825d"), Value = 0.4, Circular = new ProductChildCircularInformation() { InheritEfficiencyPercentages = true, InheritEndOfLifePercentages = true, InheritFeedstockPercentages = true } });
            }

            return product.Id;
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
                    Name = "API portfolio",
                    Owner = "Owner Name"
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
                    Description = $"API portfolio: {DateTime.Now}",
                    Owner = "Owner Name"
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
            var materialClassifications = await settingsClient.GetMaterialClassificationsAsync(AcceptLanguage.Nl);

            var building = await client.AddBuildingAsync(new BuildingRequest()
            {
                FolderId = folderId,
                Name = $"API building: {DateTime.Now}",
                Phase = BuildingPhase.New,
                BuildingUsage = usages.First().Key,
                MaterialClassificationTypeId = materialClassifications.First(mc => mc.Name == "Madaster").Id,
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
        static async Task CreateBuildingFileAsync(HttpClient httpClient, Guid buildingId, Guid productId) {
            var fileClient = new BuildingFileClient(httpClient);
            var elementClient = new BuildingFileElementClient(httpClient);
            var settingsClient = new SystemSettingsClient(httpClient);

            var classifications = await settingsClient.GetClassificationMethodsAsync();

            var file = await fileClient.AddFileAsync(buildingId, new BuildingFileRequest()
            {
                Name = "API ifc file",
                Type = BuildingRequestFileType.Source,
                PreferredDatabaseIds = new[] { Guid.Empty },
                ClassificationTypeId = classifications.First(c => c.Name.Nl == "NL-SfB").Id,
            });

            Console.WriteLine($"  - Created file");

            await fileClient.SetImportingAsync(buildingId, file.Id);
            Console.WriteLine($"  => Enable import");

            await elementClient.AddElementAsync(buildingId, file.Id, new BuildingFileElementRequest()
            {
                Id = "ifcelement1",
                Name = "Window",
                MaterialName = "Dubbelglas",
                MaterialId = productId,
                PhaseLookup = "casco",
                Volume = 10,
                ClassificationLookup = "21.22",
                ElementClass = "ifcwindow",
                TypeName = "windowtype 123"
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