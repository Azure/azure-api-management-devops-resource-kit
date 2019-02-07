using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class IssueTemplateResource : APITemplateSubResource
    {
        public IssueTemplateProperties properties { get; set; }
        public IssueTemplateSubResource[] resources { get; set; }
    }

    public class IssueTemplateProperties
    {
        public string title { get; set; }
        public string description { get; set; }
        public string createdDate { get; set; }
        public string state { get; set; }
        public string userId { get; set; }
        public string apiId { get; set; }
    }

    public class IssueTemplateAttachment : IssueTemplateSubResource
    {
        public IssueTemplateAttachmentProperties properties { get; set; }
    }

    public class IssueTemplateAttachmentProperties
    {
        public string title { get; set; }
        public string contentFormat { get; set; }
        public string content { get; set; }
    }

    public class IssueTemplateComment : IssueTemplateSubResource
    {
        public IssueTemplateCommentProperties properties { get; set; }
    }

    public class IssueTemplateCommentProperties
    {
        public string text { get; set; }
        public string createdDate { get; set; }
        public string userId { get; set; }
    }

    public abstract class IssueTemplateSubResource { 
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }}
}
