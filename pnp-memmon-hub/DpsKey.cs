namespace pnp_memmon_hub
{
    internal class DpsKey
    {
        internal static string ComputeDeviceKey(string masterKey, string deviceId) => 
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));
    }
}
