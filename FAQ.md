# Azure API Management DevOps FAQ

### Can I deploy everything through ARM templates now?
The only thing that's not available is developer portal content. Everything else is available through ARM.  

### Your example is hosted in a public repo on Github, what if I want to use a private repo? 
Check out this [article](https://blog.eldert.net/api-management-ci-cd-using-arm-templates-linked-template/), if you use a private repo, you can add a step in your pipeline to copy the linked templates to a blob container and access the files with a SAS token. 

### Why did I run into `PessimisticConcurrencyConflict` error when trying to deploy the templates? 
If you are trying to make multiple changes to the same API (e.g., import an Open API definition, add to a product), then you should daisy chain the operations using the DependsOn element. 
