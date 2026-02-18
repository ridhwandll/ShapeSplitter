using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class SaveCrypto
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("A8F3C1E9D2B7K4M9"); // 16 bytes
    private static readonly byte[] IV =  Encoding.UTF8.GetBytes("1H7F2D9K3M5P8Q4Z"); // 16 bytes

    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs =
            new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sw = new StreamWriter(cs);

        sw.Write(plainText);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string encryptedText)
    {
        byte[] buffer = Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream ms = new MemoryStream(buffer);
        using CryptoStream cs =
            new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}