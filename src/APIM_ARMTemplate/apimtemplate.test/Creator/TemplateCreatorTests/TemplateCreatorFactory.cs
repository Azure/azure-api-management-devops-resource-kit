using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class TemplateCreatorFactory
    {
        public static APIVersionSetTemplateCreator GenerateAPIVersionSetTemplateCreator()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
            return apiVersionSetTemplateCreator;
        }

        public static APITemplateCreator GenerateAPITemplateCreator()
        {
            FileReader fileReader = new FileReader();
            TemplateCreator templateCreator = new TemplateCreator();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, templateCreator, policyTemplateCreator, productAPITemplateCreator, diagnosticTemplateCreator);
            return apiTemplateCreator;
        }

        public static DiagnosticTemplateCreator GenerateDiagnosticTemplateCreator()
        {
            return new DiagnosticTemplateCreator();
        }

        public static MasterTemplateCreator GenerateMasterTemplateCreator()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);
            return masterTemplateCreator;
        }

        public static PolicyTemplateCreator GeneratePolicyTemplateCreator()
        {
            FileReader fileReader = new FileReader();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
            return policyTemplateCreator;
        }

        public static ProductAPITemplateCreator GenerateProductAPITemplateCreator()
        {
            return new ProductAPITemplateCreator();
        }
    }
}
