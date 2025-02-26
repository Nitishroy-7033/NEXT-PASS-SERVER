using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NextPassAPI.Identity.Utils
{
    public class EncryptionHelper
    {
        private readonly byte[] encryptionKey;

        public EncryptionHelper(string base64Key)
        {
            encryptionKey = Convert.FromBase64String(base64Key);

            if (encryptionKey.Length != 32) // Ensure it's 256-bit (AES-256)
                throw new ArgumentException("Invalid key length. Expected 32 bytes.");
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // Generates IV for this encryption session

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Store IV at start

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    return Convert.ToBase64String(ms.ToArray()); // Return IV + Cipher
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    byte[] iv = new byte[16];
                    ms.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string GenerateSecureKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] keyBytes = new byte[32]; // 256-bit key for AES-256
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes); // Store as Base64
            }
        }
    }
}
