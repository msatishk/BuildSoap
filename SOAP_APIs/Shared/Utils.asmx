using System;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Shared.Utils
{
    public class Utils
    {
        public static string VerifyAppSettingString(string SettingName)
        {
            string Setting = ConfigurationManager.AppSettings[SettingName];
            if (string.IsNullOrEmpty(Setting))
            {
                throw new Exception($"Please set Environment variable {SettingName}.");
            }
            return Setting;
        }

        public static string GetSecret(string secretName)
        {
            string res = null;
            try
            {
                string keyVaultUri = VerifyAppSettingString("KeyVaultURI");
                SecretClient client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                Azure.Response<KeyVaultSecret> secret =  client.GetSecret(secretName);
                res = secret?.Value?.Value;
                if (string.IsNullOrEmpty(res))
                {
                    throw new Exception($"Please set KeyVault secret for variable {secretName}.");
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return res;
        }
    }
}
