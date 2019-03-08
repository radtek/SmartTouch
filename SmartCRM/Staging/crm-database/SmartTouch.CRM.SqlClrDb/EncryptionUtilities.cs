using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;


[CompilerGenerated]
public static class EncryptionUtilities
{
    /*
    * Same methods are available in Enterprise-Utitilies project. If any change to these methods. Please change there also.
    * */
    private static string _keyValue;

    private static string KeyValue
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_keyValue))
            {
                _keyValue = File.ReadAllText(@"C:\Smarttouch\EncryptionKey.txt");
                //using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\SmartTouch"))
                //{
                //    _keyValue = key.GetValue("EKey") as string;
                //}
            }
            
            return _keyValue;
        }
    }

    private const int keysize = 256;
    private const string initVector = "smartindia7hcuot";

    //Encrypt
    [Microsoft.SqlServer.Server.SqlFunction]
    public static string EncryptString(string plainText)
    {
        byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        PasswordDeriveBytes password = new PasswordDeriveBytes(KeyValue, null);
        byte[] keyBytes = password.GetBytes(keysize / 8);
        RijndaelManaged symmetricKey = new RijndaelManaged();
        symmetricKey.Mode = CipherMode.CBC;
        ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        cryptoStream.FlushFinalBlock();
        byte[] Encrypted = memoryStream.ToArray();
        memoryStream.Close();
        cryptoStream.Close();
        return Convert.ToBase64String(Encrypted);
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static string DecryptString(string cipherText)
    {
        byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
        byte[] DeEncryptedText = Convert.FromBase64String(cipherText);
        PasswordDeriveBytes password = new PasswordDeriveBytes(KeyValue, null);
        byte[] keyBytes = password.GetBytes(keysize / 8);
        RijndaelManaged symmetricKey = new RijndaelManaged();
        symmetricKey.Mode = CipherMode.CBC;
        ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
        MemoryStream memoryStream = new MemoryStream(DeEncryptedText);
        CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        byte[] plainTextBytes = new byte[DeEncryptedText.Length];
        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        memoryStream.Close();
        cryptoStream.Close();
        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
    }

}

