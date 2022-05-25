using System.Security.Cryptography;
using System.Text;

namespace pnp_memmon_hub
{
    internal class DpsKey
    {
        internal static string ComputeDeviceKey(string masterKey, string deviceId)
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(masterKey));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
        }
    }
}
