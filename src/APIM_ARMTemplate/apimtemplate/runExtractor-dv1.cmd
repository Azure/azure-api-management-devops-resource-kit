@rem extract ARM templates from dv1
@ECHO on
dotnet run extract --sourceApimName it-shared-apim-epa-vic-dv1 --destinationApimName it-shared-apim-epa-vic-dev --resourceGroup it-shared-integ-rg-epa-vic-dev --fileFolder C:\Dev\EPA\api-security-v1\apis\arm-files --apiName api-security-v1 --policyXMLBaseUrl https://deploystintegvicdev.blob.core.windows.net/deploysecurityv1apidef/_api-security-v1-features/drop/policies --linkedTemplatesBaseUrl https://deploystintegvicdev.blob.core.windows.net/deploysecurityv1apidef/_api-security-v1-features/drop/arm-files
pause