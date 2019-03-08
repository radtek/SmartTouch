using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LandmarkIT.Enterprise.Utilities.Security
{
    internal class Rijndael
    {
        public string Encrypt(string plainText, string passPhrase, string saltValue, string initVector, int keySize)
        {
            var cipherText = default(string);

            var initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(passPhrase, saltValueBytes);
            var keyBytes = password.GetBytes(keySize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();

                cipherText = Convert.ToBase64String(memoryStream.ToArray());

                cryptoStream.Close();
                memoryStream.Close();
            }

            return cipherText;
        }

        public string Decrypt(string cipherText, string passPhrase, string saltValue, string initVector, int keySize)
        {
            var plainText = default(string);
            var initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(passPhrase, saltValueBytes);
            var keyBytes = password.GetBytes(keySize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

                cryptoStream.Close();
                memoryStream.Close();
            }

            return plainText;
        }
    }
}
