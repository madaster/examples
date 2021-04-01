import madasterapi
import time

from datetime import date, datetime
from pprint import pprint
from madasterapi.api.account_api import AccountApi
from madasterapi.api.folder_api import FolderApi
from madasterapi.api.building_api import BuildingApi
from madasterapi.api.building_file_api import BuildingFileApi
from madasterapi.api.building_file_element_api import BuildingFileElementApi
from madasterapi.api.system_settings_api import SystemSettingsApi

from madasterapi.model.folder_request import FolderRequest
from madasterapi.model.accept_language import AcceptLanguage
from madasterapi.model.building_request import BuildingRequest
from madasterapi.model.building_phase import BuildingPhase
from madasterapi.model.building_file_request import BuildingFileRequest
from madasterapi.model.building_request_file_type import BuildingRequestFileType
from madasterapi.model.building_file_element_request import BuildingFileElementRequest
from madasterapi.model.building_file_statuses import BuildingFileStatuses

configuration = madasterapi.Configuration(
    host = "https://api.madaster.com",
    api_key={'ApiKeyAuth': '-- replace me --'}
)

with madasterapi.ApiClient(configuration) as api_client:
    accountClient = AccountApi(api_client)
    folderClient = FolderApi(api_client)
    buildingClient = BuildingApi(api_client)
    settingsClient = SystemSettingsApi(api_client)
    fileClient = BuildingFileApi(api_client)
    elementClient = BuildingFileElementApi(api_client)
    
    # Print the account we are running as
    try:
        account = accountClient.account_get_account()
        print(f"Current account: {account.name}")
    except madasterapi.ApiException as e:
        print("Exception when calling account_api->account_get_account: %s\n" % e)

    # Find folderType with the dutch name 'Portfolio'
    try:
        folderTypes = accountClient.account_get_folder_types()
        fldr = next(f for f in folderTypes if f.name.nl == "Portfolio")
    except madasterapi.ApiException as e:
        print("Exception when calling account_api->account_get_folder_types: %s\n" % e)
    
    # Get all folders, try to find the folder with name 'Python portfolio'. Create folder if not found
    try:
        folders = folderClient.folder_get_folders()
        folder = next(f for f in folders if f.name == "Python portfolio")
        req = FolderRequest(
            name="Python portfolio",
            foldertype_id=fldr.id,
            parent_id=folder.parent_id,
            description=f"API portfolio: {datetime.now()}",
            owner="Folder owner"
        )
        folder = folderClient.folder_update_folder(folder.id, folder_request=req)
        print("- Updated portfolio")

    except madasterapi.ApiException as e:
        print("Exception when calling account_api->account_get_folder_types: %s\n" % e)
    except StopIteration as i:
        req = FolderRequest(
            name="Python portfolio",
            foldertype_id=fldr.id,
            parent_id=account.id,
            description="",
            owner="Folder owner"
        )
        folder = folderClient.folder_add_folder(folder_request=req)
        print("- Created portfolio")

    # Create building (with Madaster material classification)
    usages = settingsClient.system_settings_get_building_usages(accept_language=AcceptLanguage("nl"))
    
    materialClassifications = settingsClient.system_settings_get_material_classifications(accept_language=AcceptLanguage("nl"))
    request = BuildingRequest(
      folder_id=folder.id,
      name=f"API building: {datetime.now()}",
      phase=BuildingPhase("New"),
      gross_surface_area=float(120),
      building_usage=list(usages.keys())[0],
      material_classification_type_id=next(f for f in materialClassifications if f.name == "Madaster").id,
      completion_date=datetime.now()
    )
    building = buildingClient.building_add_building(building_request=request)
    print("- Created building")

    # Create building file (with nl-sfb classification)
    classifications = settingsClient.system_settings_get_classification_methods()
    fileRequest = BuildingFileRequest(
        name="API ifc file",
        preferred_database_ids=["00000000-0000-0000-0000-000000000000"],
        type=BuildingRequestFileType("Source"),
        classification_type_id=next(f for f in classifications if f.name.nl == "NL-SfB").id,
    )
    file = fileClient.building_file_add_file(building.id, building_file_request=fileRequest)
    print("  - Created file")

    # Set file in status importing
    fileClient.building_file_set_importing(building.id, file.id)
    print("  => Enable import")

    # Add ifc element in the file
    element = BuildingFileElementRequest(
        id = "ifcelement1",
        name = "My steel wall",
        material_name = "staal",
        phase_lookup = "casco",
        volume = float(10),
        classification_lookup = "21.22",
        element_class = "ifcwall",
        type_name = "walltype 123"
    )
    response = elementClient.building_file_element_add_element(building.id, file.id, building_file_element_request=element)
    print("   - Created ifc element")

    # Start the refinement
    fileClient.building_file_start_refinement(building.id, file.id)
    print("  => Started refinement", end="", flush=True)

    # Wait while the file is processed
    fileStatus = fileClient.building_file_get_status_by_id(building.id, file.id)
    while fileStatus.status.value != "Mapped":
      print(".", end="", flush=True)
      time.sleep(5)
      fileStatus = fileClient.building_file_get_status_by_id(building.id, file.id)
    else:
      print("")

    # Activate the file, building will be recalculated
    fileClient.building_file_set_active(building.id, file.id, body=True)
    print("  => File activated, building processing started", end="", flush=True)
    building = buildingClient.building_get_building_by_id(building.id)
    while building.is_dirty == True:
      print(".", end="", flush=True)
      time.sleep(5)
      building = buildingClient.building_get_building_by_id(building.id)
    else:
      print("")

    # Generate a materialpassport
    pdf = buildingClient.building_create_pdf_passport(building.id, accept_language=AcceptLanguage("nl"))
    print("  => Passport created, processing started", end="", flush=True)
    while pdf.status.value != "Uploaded":
      print(".", end="", flush=True)
      time.sleep(5)
      pdf = fileClient.building_file_get_file_by_id(building.id, pdf.id)
    else:
      print("")

    # Retrieve the download url
    dl = fileClient.building_file_download(building.id, pdf.id)
    print(f"  => Passport download: {dl.url}")
