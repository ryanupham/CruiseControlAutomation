using Microsoft.Data.Sqlite;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PaymentAutomation.Utilities;

// https://stackoverflow.com/questions/68643057/decrypt-google-cookies-in-c-sharp-net-framework
static class ChromeCookieManager
{
    public static List<Cookie> GetCookies(string hostname)
    {
        var ChromeCookiePath = @$"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies";
        var data = new List<Cookie>();
        if (File.Exists(ChromeCookiePath))
        {
            try
            {
                SQLitePCL.Batteries.Init();

                using var conn = new SqliteConnection($"Data Source={ChromeCookiePath}");
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT name,encrypted_value,host_key FROM cookies WHERE host_key = '{hostname}'";
                var key = AesGcm256.GetKey();

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!data.Any(a => a.Name == reader.GetString(0)))
                        {
                            var encryptedData = GetBytes(reader, 1);
                            AesGcm256.Prepare(encryptedData, out var nonce, out var ciphertextTag);
                            var value = AesGcm256.Decrypt(ciphertextTag, key, nonce);

                            data.Add(new Cookie(reader.GetString(0), value));
                        }
                    }
                }
                conn.Close();
            }
            catch { }
        }
        return data;

    }

    private static byte[] GetBytes(SqliteDataReader reader, int columnIndex)
    {
        const int CHUNK_SIZE = 2 * 1024;
        var buffer = new byte[CHUNK_SIZE];
        long bytesRead;
        var fieldOffset = 0L;
        using MemoryStream stream = new();
        while ((bytesRead = reader.GetBytes(columnIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, (int)bytesRead);
            fieldOffset += bytesRead;
        }
        return stream.ToArray();
    }

    class AesGcm256
    {
        public static byte[] GetKey()
        {
            var path = @$"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data\Local State";

            var v = File.ReadAllText(path);

            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(v)!;
            string key = json.os_crypt.encrypted_key;

            var src = Convert.FromBase64String(key);
            var encryptedKey = src.Skip(5).ToArray();

            return ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser)!;
        }

        public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            var sR = string.Empty;
            try
            {
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

                cipher.Init(false, parameters);
                var plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                var retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
                cipher.DoFinal(plainBytes, retLen);

                sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return sR;
        }

        public static void Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
        {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

            Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }
    }
}
