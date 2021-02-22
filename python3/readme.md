# Madaster API example - python
In this folder you will find an example application for the Madaster API writte in python. The example uses an generated API client from the openAPI specification. The client is generated using the following command:

```
docker run --rm -v ${PWD}:/local openapitools/openapi-generator-cli generate -i https://api.madaster.com/api/v3.0/swagger.json -g python -o /local/generated --package-name madasterapi
```

## The example
This example executes the following steps:
- Retrieve the Madaster account
- Retrieves the folder types for the account, searches for a folder type with the name portfolio
- Retrieves the folders for the account, searched for a folder with the name 'Python portfolio'
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
3. Run ```pip install -r requirements.txt```
4. Update the API token in ```example.py```
5. Run ```python example.py```