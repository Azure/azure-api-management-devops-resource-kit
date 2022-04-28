# Supported Resources by Resource Kit

## What is the resource
---
Azure-ApiManagement-Devops-Resources-Kit heavily uses Azure API Management public API and relies on it to get all the information about running services.

Azure API Management team develops services and products continuously, and public API happens to change from time to time. You can always view the **Current-GA**, **current-Preview** and **Previous-GA** public API references on [Azure REST API page](https://docs.microsoft.com/en-us/rest/api/apimanagement/)

This **Resource-Kit** works with `Resource` entities. Let's define, what `Resource` in this case means:  
`Resource` - entity, that is exposed by ApiManagement, representing some information regarding specific ApiManagement instance. If one contains some set of `resources` describing ApiManagement instance, he\she can recreate another instance one by one (with absolutely the same resources)

## Supported Resources 
Please, refer to this document to understand what resources are supported in each of the Api-Versions published by the ApiManagement team:

- [api-version = 2021-08-01](./2021-08-01.md)