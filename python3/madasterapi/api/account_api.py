"""
    Madaster Private API - Build: 8815

    Welcome to the **Madaster Private API** endpoint. This endpoint can be used to interact with the Madaster Platform and its resources. This API does not fully cover all functionality of the platform yet, please see below for the available functions and what they can be used for. For detailed information about the platform and this API, please refer to the [Madaster Documentation](https://docs.madaster.com) or the [Madaster API Documentation](https://docs.madaster.com/api).<br/><br/>To access these resources, you need an authorization token. If you do not have one yet, see the chapter about Authorization in the [API documentation](https://docs.madaster.com/api). This token should be sent as a header with the name 'X-API-Key', which will authenticate the request with the token. The documentation below specifies which requests are available and which responses they might produce.<br/><br/>This API can be reached at the endpoint: **[https://api.madaster.com/](https://api.madaster.com/)**  # noqa: E501

    The version of the OpenAPI document: v3.0
    Contact: service@madaster.com
    Generated by: https://openapi-generator.tech
"""


import re  # noqa: F401
import sys  # noqa: F401

from madasterapi.api_client import ApiClient, Endpoint as _Endpoint
from madasterapi.model_utils import (  # noqa: F401
    check_allowed_values,
    check_validations,
    date,
    datetime,
    file_type,
    none_type,
    validate_and_convert_types
)
from madasterapi.model.account_response import AccountResponse
from madasterapi.model.folder_type import FolderType


class AccountApi(object):
    """NOTE: This class is auto generated by OpenAPI Generator
    Ref: https://openapi-generator.tech

    Do not edit the class manually.
    """

    def __init__(self, api_client=None):
        if api_client is None:
            api_client = ApiClient()
        self.api_client = api_client

        def __account_get_account(
            self,
            **kwargs
        ):
            """Gets the current account for the token  # noqa: E501

            This method makes a synchronous HTTP request by default. To make an
            asynchronous HTTP request, please pass async_req=True

            >>> thread = api.account_get_account(async_req=True)
            >>> result = thread.get()


            Keyword Args:
                _return_http_data_only (bool): response data without head status
                    code and headers. Default is True.
                _preload_content (bool): if False, the urllib3.HTTPResponse object
                    will be returned without reading/decoding response data.
                    Default is True.
                _request_timeout (float/tuple): timeout setting for this request. If one
                    number provided, it will be total request timeout. It can also
                    be a pair (tuple) of (connection, read) timeouts.
                    Default is None.
                _check_input_type (bool): specifies if type checking
                    should be done one the data sent to the server.
                    Default is True.
                _check_return_type (bool): specifies if type checking
                    should be done one the data received from the server.
                    Default is True.
                _host_index (int/None): specifies the index of the server
                    that we want to use.
                    Default is read from the configuration.
                async_req (bool): execute request asynchronously

            Returns:
                AccountResponse
                    If the method is called asynchronously, returns the request
                    thread.
            """
            kwargs['async_req'] = kwargs.get(
                'async_req', False
            )
            kwargs['_return_http_data_only'] = kwargs.get(
                '_return_http_data_only', True
            )
            kwargs['_preload_content'] = kwargs.get(
                '_preload_content', True
            )
            kwargs['_request_timeout'] = kwargs.get(
                '_request_timeout', None
            )
            kwargs['_check_input_type'] = kwargs.get(
                '_check_input_type', True
            )
            kwargs['_check_return_type'] = kwargs.get(
                '_check_return_type', True
            )
            kwargs['_host_index'] = kwargs.get('_host_index')
            return self.call_with_http_info(**kwargs)

        self.account_get_account = _Endpoint(
            settings={
                'response_type': (AccountResponse,),
                'auth': [
                    'ApiKeyAuth'
                ],
                'endpoint_path': '/api/v3.0/account',
                'operation_id': 'account_get_account',
                'http_method': 'GET',
                'servers': None,
            },
            params_map={
                'all': [
                ],
                'required': [],
                'nullable': [
                ],
                'enum': [
                ],
                'validation': [
                ]
            },
            root_map={
                'validations': {
                },
                'allowed_values': {
                },
                'openapi_types': {
                },
                'attribute_map': {
                },
                'location_map': {
                },
                'collection_format_map': {
                }
            },
            headers_map={
                'accept': [
                    'application/json'
                ],
                'content_type': [],
            },
            api_client=api_client,
            callable=__account_get_account
        )

        def __account_get_folder_types(
            self,
            **kwargs
        ):
            """Gets the foldertypes for the account  # noqa: E501

            This method makes a synchronous HTTP request by default. To make an
            asynchronous HTTP request, please pass async_req=True

            >>> thread = api.account_get_folder_types(async_req=True)
            >>> result = thread.get()


            Keyword Args:
                _return_http_data_only (bool): response data without head status
                    code and headers. Default is True.
                _preload_content (bool): if False, the urllib3.HTTPResponse object
                    will be returned without reading/decoding response data.
                    Default is True.
                _request_timeout (float/tuple): timeout setting for this request. If one
                    number provided, it will be total request timeout. It can also
                    be a pair (tuple) of (connection, read) timeouts.
                    Default is None.
                _check_input_type (bool): specifies if type checking
                    should be done one the data sent to the server.
                    Default is True.
                _check_return_type (bool): specifies if type checking
                    should be done one the data received from the server.
                    Default is True.
                _host_index (int/None): specifies the index of the server
                    that we want to use.
                    Default is read from the configuration.
                async_req (bool): execute request asynchronously

            Returns:
                [FolderType]
                    If the method is called asynchronously, returns the request
                    thread.
            """
            kwargs['async_req'] = kwargs.get(
                'async_req', False
            )
            kwargs['_return_http_data_only'] = kwargs.get(
                '_return_http_data_only', True
            )
            kwargs['_preload_content'] = kwargs.get(
                '_preload_content', True
            )
            kwargs['_request_timeout'] = kwargs.get(
                '_request_timeout', None
            )
            kwargs['_check_input_type'] = kwargs.get(
                '_check_input_type', True
            )
            kwargs['_check_return_type'] = kwargs.get(
                '_check_return_type', True
            )
            kwargs['_host_index'] = kwargs.get('_host_index')
            return self.call_with_http_info(**kwargs)

        self.account_get_folder_types = _Endpoint(
            settings={
                'response_type': ([FolderType],),
                'auth': [
                    'ApiKeyAuth'
                ],
                'endpoint_path': '/api/v3.0/account/foldertypes',
                'operation_id': 'account_get_folder_types',
                'http_method': 'GET',
                'servers': None,
            },
            params_map={
                'all': [
                ],
                'required': [],
                'nullable': [
                ],
                'enum': [
                ],
                'validation': [
                ]
            },
            root_map={
                'validations': {
                },
                'allowed_values': {
                },
                'openapi_types': {
                },
                'attribute_map': {
                },
                'location_map': {
                },
                'collection_format_map': {
                }
            },
            headers_map={
                'accept': [
                    'application/json'
                ],
                'content_type': [],
            },
            api_client=api_client,
            callable=__account_get_folder_types
        )
