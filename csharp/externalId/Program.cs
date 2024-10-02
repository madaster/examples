using System.Net;

using Example.Client.Shared;
using Example.Client.V3;

namespace Example.Client;

internal class Program
{
    private const string environmentUrl = "https://api.madaster.com";
    private const string token = "-- REPLACE ME --";

    private static async Task Main()
    {
        var httpClientV3 = new HttpClient() { BaseAddress = new Uri(environmentUrl) };
        httpClientV3.DefaultRequestHeaders.Add("X-API-Key", token);


        FolderResponse folder = await GetOrCreateFolder(httpClientV3, "folder-1234");

        BuildingResponse building = await GetOrCreateBuilding(httpClientV3, folder.Id, "building-1234");

        await CreateBuildingFile(httpClientV3, building.Id);
    }

    private static async Task<FolderResponse> GetOrCreateFolder(HttpClient httpClientV3, string externalId)
    {
        FolderResponse folder;
        var folderClient = new FolderClient(httpClientV3);

        try
        {
            folder = await folderClient.GetFolderByExternalIdAsync(externalId);
            if (folder.ExternalId != externalId)
            {
                throw new Exception("This should not happen :-(");
            }
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 404)
            {
                folder = await folderClient.AddFolderAsync(new FolderRequest() {
                    ExternalId = externalId,
                    Name = "My folder",
                    FoldertypeId = await GetPortfolioTypeId(httpClientV3)
                });
            }
            else
            {
                throw;
            }
        }

        return folder;
    }

    private static async Task<BuildingResponse> GetOrCreateBuilding(HttpClient httpClientV3, Guid id, string externalId)
    {
        BuildingResponse building;
        var buildingClient = new BuildingClient(httpClientV3);

        try
        {
            building = await buildingClient.GetBuildingByExternalIdAsync(externalId);
            if (building.ExternalId != externalId)
            {
                throw new Exception("This should not happen :-(");
            }
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 404)
            {
                building = await buildingClient.AddBuildingAsync(new BuildingRequest()
                {
                    ExternalId = externalId,
                    Name = "My building",
                    FolderId = id,
                    MaterialClassificationTypeId = "nl_sfb",
                    BuildingUsage = "other-othersub",
                    BuildingUsageOtherDescription = "My own buildingType"
                });
            }
            else
            {
                throw;
            }
        }

        return building;
    }

    private static async Task CreateBuildingFile(HttpClient httpClient, Guid buildingId)
    {
        var fileClient = new BuildingFileClient(httpClient);
        var elementClient = new BuildingFileElementClient(httpClient);

        var file = await fileClient.AddFileAsync(buildingId, new BuildingFileRequest()
        {
            Name = "API ifc file",
            Type = BuildingRequestFileType.Source,
            PreferredDatabaseIds = [
                Guid.Parse("cd2bda71-760b-4fcc-8a0b-3877c10000a8") // cd2bda71-760b-4fcc-8a0b-3877c10000a8 = the Okobaudat 2023 database ID
            ],
            ClassificationTypeId = Guid.Parse("e6bbe656-6722-4f7c-a825-8be526e13189"), // Omniclass
        });

        await fileClient.SetImportingAsync(buildingId, file.Id);

        await elementClient.AddElementAsync(buildingId, file.Id, new BuildingFileElementRequest()
        {
            Id = "ifcelement1",
            Name = "Narrow wall",
            MaterialName = "Concrete",
            ExternalDatabaseId = "923e1c71-f172-4902-a5f5-c4a1e8a773bc", //https://oekobaudat.de/OEKOBAU.DAT/datasetdetail/process.xhtml?uuid=923e1c71-f172-4902-a5f5-c4a1e8a773bc&version=20.23.050
            PhaseLookup = "casco",
            Volume = 10,
            ClassificationLookup = "21-01 10 10 10",
            ElementClass = "ifcwall",
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

    private static async Task<Guid> GetPortfolioTypeId(HttpClient httpClientV3)
    {
        var accountClient = new AccountClient(httpClientV3);
        var folderTypes = await accountClient.GetFolderTypesAsync();

        return folderTypes.First(x => x.Name.En == "Portfolio").Id;
    }
}
