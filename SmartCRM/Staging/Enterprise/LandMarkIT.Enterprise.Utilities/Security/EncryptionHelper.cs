using System;
using System.Security.Cryptography;
using System.Text;

namespace LandmarkIT.Enterprise.Utilities.Security
{
    public static class EncryptionHelper
    {
        #region static variables
        private static string _passPhrase = default(string);
        private static string _saltValue = default(string);
        private static string _initVector = default(string);
        private static int _keySize = default(int);

        private static Rijndael _rijndael = default(Rijndael);
        #endregion

        #region default values
        static EncryptionHelper()
        {
            _passPhrase = "lm!t@secur!ty"; // can be any string
            _saltValue = "s@11salt"; // can be any string
            _initVector = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
            _keySize = 128; // can be 192 or 128 

            _rijndael = new Rijndael();
        }
        #endregion

        #region Encryption & Decryption methods
        /// <summary>
        /// Sets algorithm values
        /// </summary>
        /// <param name="passPhrase">can be any string</param>
        /// <param name="saltValue">can be any string</param>
        /// <param name="initVector">must be 16 bytes</param>
        /// <param name="keySize">192 or 128</param>
        public static void SetValues(string passPhrase, string saltValue, string initVector, int keySize)
        {
            _passPhrase = passPhrase;
            _saltValue = saltValue;
            _initVector = initVector;
            _keySize = keySize;
        }
        public static string Encrypt(string plainText)
        {
            return _rijndael.Encrypt(plainText, _passPhrase, _saltValue, _initVector, _keySize);
        }
        public static string Decrypt(string cipherText)
        {
            return _rijndael.Decrypt(cipherText, _passPhrase, _saltValue, _initVector, _keySize);
        }
        public static string ComputeHash(string plainText, AuthenticationType authenticationType)
        {
            var hashedValue = new StringBuilder();

            var hashedData = authenticationType == AuthenticationType.SHA
                ? SHA1CryptoServiceProvider.Create().ComputeHash(Encoding.Unicode.GetBytes(plainText))
                : MD5CryptoServiceProvider.Create().ComputeHash(Encoding.Unicode.GetBytes(plainText));

            foreach (var item in hashedData)
            {
                hashedValue.Append(String.Format("{0,2:X2}", item));
            }
            return hashedValue.ToString();
        }
        #endregion
    }
}
