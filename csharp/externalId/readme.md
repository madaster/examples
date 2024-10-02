# Madaster API example - C#
In this folder you will find an example application for the Madaster API written in C#. The example uses an generated API client using the [build-in OpenAPI tooling](https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi)

This example makes use of new fields:
- externalId on folder & building
- externalDatabaseId on buildingfilelement


## The example
This example executes the following steps:
- Retrieves the a folder by ExternalId
  - Creates a new folder if the folder is not found
- Retrieves the a building by ExternalId
  -   Creates a new building if the building is not found
- Create a new file
  - Sets the file to status importing (needed to create ifc elements)
- Create a new ifc element, uses the externalDatabaseId to link to a product from the Okobaudat database
- Starts file processing

## Running the example
1. Create a API token in the platform (needs write permissions to create a folder/building)
2. Download this example repo
4. Update the API token in ```Program.cs```
5. Run the example
