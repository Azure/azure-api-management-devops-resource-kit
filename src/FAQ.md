# Azure API Management DevOps FAQ

### Is everything available through ARM templates now?
The only thing that's not available is developer portal content. Everything else is available through ARM.  

### Is there a size limit of ARM templates?
Yes, each template is limited to 1MB. Our proposed approach uses linked templates, which helps to keep each template relatively small. 

### Your example is hosted in a public repo on Github, what if I want to use a private repo? 
Check out this [article](https://blog.eldert.net/api-management-ci-cd-using-arm-templates-linked-template/), if you use a private repo, you can add a step in your pipeline to copy the linked templates to a blob container and access the files with a SAS token. 

### How do I deploy all configurations versus just deploy an API?
In our example, each template can be deployed individually. You can also use the master template, which ties everything together, to deploy all templates. 
