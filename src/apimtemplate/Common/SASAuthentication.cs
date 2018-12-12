using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class SASAuthentication
    {
        // api-version query parameter - https://aka.ms/smapi#VersionQueryParameter
        static string apiVersion = "2014-02-14-preview";

        static string serviceName = "contoso5";
        static string baseUrl = string.Format("https://{0}.management.azure-api.net", serviceName);

        #region [TBI] SAS from Portal 
        // You can get an access token from the API Management portal or you can programmatically generate it. For
        // more instructions on both approaches, see http://aka.ms/smapi#Authentication
        // One common cause of 401 Unauthorized response codes is when the Expiry date of the token that was
        // generated in the publisher portal has passed. If that happens, regenerate the token using the directions 
        // in the link above. If you programmatically generate the token this typically does not happen.
        // To use a token generated in the publisher portal, follow the "To manually create an access token" instructions
        // at http://aka.ms/smapi#Authentication and paste in the token using the following format.
        static string sharedAccessSignature = "uid=...&ex=...";

        // To programmatically generate the token, call the CreateSharedAccessToken method below.
        #endregion

        public void GetSAStoken(string id, string key)
        {
            DateTime expiry = DateTime.UtcNow.AddDays(1);

            sharedAccessSignature = CreateSharedAccessToken(id, key, expiry);

        }
        static private string CreateSharedAccessToken(string id, string key, DateTime expiry)
        {
            using (var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                string dataToSign = id + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                string x = string.Format("{0}\n{1}", id, expiry.ToString("O", CultureInfo.InvariantCulture));
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var signature = Convert.ToBase64String(hash);
                string encodedToken = string.Format("uid={0}&ex={1:o}&sn={2}", id, expiry, signature);
                return encodedToken;
            }
        }
    }
}
