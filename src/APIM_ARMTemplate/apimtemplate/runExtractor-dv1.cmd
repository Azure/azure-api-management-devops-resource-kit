@rem extract ARM templates from dv1. Example of apiName=api-security-v1, outputFolder=C:\Dev\EPA\api-security-v1\apis\arm-files
@ECHO on
set /p apiName=Enter API Name : 
set /p outputFolder=Enter Output folder: 
dotnet run extract --sourceApimName it-shared-apim-epa-vic-dv1 --destinationApimName it-shared-apim-epa-vic-dev --resourceGroup it-shared-integ-rg-epa-vic-dev --fileFolder "%outputFolder%" --apiName "%apiName%" --policyXMLBaseUrl https://deploystintegvicdev.blob.core.windows.net/deploysecurityv1apidef/_api-security-v1-features/drop/policies --linkedTemplatesBaseUrl https://deploystintegvicdev.blob.core.windows.net/deploysecurityv1apidef/_api-security-v1-features/drop/arm-files
pause