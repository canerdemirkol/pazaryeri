using System.Security.Cryptography;
using System.Text;

namespace OBase.Pazaryeri.Core.Utility
{
    public static class Cipher
    {
        //private static readonly string saltValue = "p&@3r^wQ";
        //private static readonly string passPhrase = "@8uRda^4pqc'q@";
        //private static readonly string hashAlgorithm = "SHA256";
        //private static readonly string initVector = "@1B2c3D4e5F6g7H8";
        //private static readonly int keySize = 256;
        //private static readonly int passwordIterations = 2;
        //private static bool IsUnlocked = true;
        private static readonly string key = "b14ca5898a4e4142aace2ea2143a2410";
        public static string EncryptString(string text2Crypt)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(text2Crypt);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string crypt2Text)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(crypt2Text);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);//I have already defined "Key" in the above EncryptString function
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}