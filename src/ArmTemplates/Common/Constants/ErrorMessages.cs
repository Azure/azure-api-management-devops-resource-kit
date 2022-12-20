// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants
{
    public static class ErrorMessages
    {
        public const string DuplicateTagResourceNameErrorMessage = "Duplicate tag resource name found during sanitizing the display name. Please consider renaming tags: {0}, {1}. Both resulted resource name to be equal to: {2}";
        public const string EmptyResourceNameAfterSanitizingErrorMessage = "Sanitizing the display name '{0}' resulted empty string";
        public const string ApiVersionDoesNotExistErrorMessage = "API Version Set with this name doesn't exist";
    }
}
