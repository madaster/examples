# Madaster API example - C#
In this folder you will find an example application for the Madaster API written in C#. The example uses an generated API client using the [build-in OpenAPI tooling](https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi)

## The example
This example executes the following steps:
- Retrieve the Madaster account
- Retrieves the folder types for the account, searches for a folder type with the name 'portfolio'
- Retrieves the folders for the account, searched for a folder with the name 'API portfolio'
  - Creates a new folder if the folder is not found
- Create a new building
- Create a new file
  - Sets the file to status importing (needed to create ifc elements)
- Create a new ifc element
- Starts file processing
- Activates the building file
- Creates a materialpassport
- Retrieves the download url

## Running the example
1. Create a API token in the platform (needs write permissions to create a folder/building)
2. Download this example repo
4. Update the API token in ```Program.cs```
5. Run the example

## Updates:
- dec 2022: updated example, uses API v4 to create database and product.
