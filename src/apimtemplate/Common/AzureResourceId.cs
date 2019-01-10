using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIManagementTemplate
{
    public class AzureResourceId
    {

        public string ResourceGroupName
        {
            get
            {
                if (this.splittedId.Length > 4)
                {
                    return this.splittedId[4];
                }
                return "";
            }
            set
            {
                if (this.splittedId.Length > 4)
                {
                    this.splittedId[4] = value;
                }
            }
        }
        public string SubscriptionId
        {
            get
            {
                if (this.splittedId.Length > 2)
                {
                    return this.splittedId[2];
                }
                return "";
            }
            set
            {
                if (this.splittedId.Length > 2)
                {
                    this.splittedId[2] = value;
                }
            }
        }
        public string ResourceName
        {
            get
            {
                return this.splittedId.Last();
            }
            set
            {
                this.splittedId[this.splittedId.Length - 1] = value;
            }
        }

        public string ValueAfter(String type)
        {
            var rest = this.splittedId.SkipWhile(s => s != type).Skip(1);
            return rest.FirstOrDefault();
        }

        public void ReplaceValueAfter(String type, string value)
        {
            var position = this.splittedId.TakeWhile(s => s != type).Count() + 1;
            if (position < this.splittedId.Length)
                this.splittedId[position] = value;
        }

        private string[] splittedId;
        public AzureResourceId(string resourceid)
        {
            string replaced = "/" + resourceid.Substring(resourceid.IndexOf("subscriptions/"));
            this.splittedId = replaced.Split('/');
        }

        public override string ToString()
        {
            return splittedId.Aggregate((a, n) => { return a + '/' + n; }).ToString();
        }
    }
}
