
# flake8: noqa

# Import all APIs into this package.
# If you have many APIs here with many many models used in each API this may
# raise a `RecursionError`.
# In order to avoid this, import only the API that you directly need like:
#
#   from .api.account_api import AccountApi
#
# or import this package, but before doing it, use:
#
#   import sys
#   sys.setrecursionlimit(n)

# Import APIs into API package:
from madasterapi.api.account_api import AccountApi
from madasterapi.api.building_api import BuildingApi
from madasterapi.api.building_file_api import BuildingFileApi
from madasterapi.api.building_file_element_api import BuildingFileElementApi
from madasterapi.api.database_api import DatabaseApi
from madasterapi.api.folder_api import FolderApi
from madasterapi.api.material_api import MaterialApi
from madasterapi.api.product_api import ProductApi
from madasterapi.api.system_settings_api import SystemSettingsApi
