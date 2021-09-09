# Madaster API example - C#
In this folder you will find an example application for the Madaster API written in C#. The example uses an generated API client using the [build-in OpenAPI tooling](https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi)

## The upload example
This example executes the following steps:
- Create a new file in the building
- Uploads an ifc-file from disk


## Prerequisites
- Existing building in the platform, copy the buildingId from the url

## Running the example
1. Create a API token in the platform (needs write permissions to create a folder/building)
2. Download this example repo
4. Update the API token and buildingId in ```Program.cs```
5. Run the example