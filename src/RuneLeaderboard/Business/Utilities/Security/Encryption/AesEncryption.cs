using System.Security.Cryptography;
using System.Text;


namespace Api.Business.Utilities.Security.Encryption;

public static class AesEncryption
{
    public static string Decrypt(string cipherText, string key, string iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.ASCII.GetBytes(key);
            aes.IV = Convert.FromBase64String(iv);

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }


    public static string Encrypt(string plainText, string key, string iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.ASCII.GetBytes(key);
            aes.IV = Convert.FromBase64String(iv);

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
