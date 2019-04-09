# ARM Template samples showing how to Create an API Version and Revision

The example shows how to create `Versions` and `Revisions` of a sample `HttpBin` API in ApiManagement service. 

## Version Set
- api-httpbin.version-set.template.json

Execute this template to create a new Version set `versionset-httpbin-api` in API Management service.

## v1
- api-httpbin.v1.template.json

Execute this template to create a new `Http Bin` Api having a `GET` and `POST` Operation and associated to the `Started` Product.

## v2
- api-httpbin.v2.template.json

Execute this template to create a new Version `v2` of the `Http Bin` API.

## v2-rev2
- api-httpbin.v2-rev2.template

Execute this template to create a new revision of the `v2` `Http Bin` Api having which adds a `DELETE` Operation to the API.

## v2-switch-rev
- api-httpbin.v2.switch.template.json

Execute this template to create a `beta` release and switch the `HttpBinAPI-v2;rev2` to be the current Api Revision of the `v2` `HttpBin` API.