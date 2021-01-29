using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.ComponentModel;
namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public byte[] EncryptedBytes = new byte[32];
        public byte[] EncryptKey = new byte[32];
        public byte[] IV = new byte[32];
        public byte[] EncryptKey2 = new byte[32];
        public byte[] IV2 = new byte[32];
        public byte[] EncryptKey3 = new byte[32];
        public byte[] IV3 = new byte[32];

        public static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public Rijndael AES = Rijndael.Create();

        public ICryptoTransform encryptor;
        public ICryptoTransform decryptor;
     
        private void EncryptIdxListCode()
        {            
            int second, temp;
            for (int i = 0; i < ImageIdxList.Length; i++)
            {
                EncryptRNG();
                second = GetRNGIndex();
                second = ((second % ImageIdxList.Length) + ImageIdxList.Length) % ImageIdxList.Length;                
                temp = ImageIdxList[i];
                ImageIdxList[i] = ImageIdxList[second];
                ImageIdxList[second] = temp;
            }
        }        
        
        private void DecryptIdxListCode()
        {            
            int second, temp;            
            for (int i = ImageIdxList.Length - 1; i >= 0; i--)
            {
                second = GetRNGIndex();
                DecryptRNG();
                second = ((second % ImageIdxList.Length) + ImageIdxList.Length) % ImageIdxList.Length;                
                temp = ImageIdxList[i];
                ImageIdxList[i] = ImageIdxList[second];
                ImageIdxList[second] = temp;
            }            
        }
        
        public int GetRNGIndex()
        {
            int IFI = BitConverter.ToInt32(EncryptedBytes, 0);
            IFI ^= BitConverter.ToInt32(EncryptedBytes, 4);
            IFI += BitConverter.ToInt32(EncryptedBytes, 8);
            IFI ^= BitConverter.ToInt32(EncryptedBytes, 12);
            IFI += BitConverter.ToInt32(EncryptedBytes, 16);
            IFI ^= BitConverter.ToInt32(EncryptedBytes, 20);
            IFI += BitConverter.ToInt32(EncryptedBytes, 24);
            IFI ^= BitConverter.ToInt32(EncryptedBytes, 28);
            return IFI;
        }
        public void EncryptRNG()
        {
            RunEncryptor(EncryptKey2, IV2, EncryptKey);
            RunEncryptor(IV2, EncryptKey2, IV);
            RunEncryptor(EncryptKey3, IV3, EncryptKey2);
            RunEncryptor(IV3, EncryptKey3, IV2);
            RunEncryptor(EncryptKey, IV, EncryptKey3);
            RunEncryptor(IV, EncryptKey, IV3);
            RunEncryptor(EncryptKey, IV, EncryptedBytes);
        }

        public void DecryptRNG()
        {
            RunDecryptor(EncryptKey, IV, EncryptedBytes);
            RunDecryptor(IV, EncryptKey, IV3);
            RunDecryptor(EncryptKey, IV, EncryptKey3);
            RunDecryptor(IV3, EncryptKey3, IV2);
            RunDecryptor(EncryptKey3, IV3, EncryptKey2);
            RunDecryptor(IV2, EncryptKey2, IV);
            RunDecryptor(EncryptKey2, IV2, EncryptKey);
        }

        public void InitRNGKeys()
        {
            rng.GetBytes(EncryptedBytes);
            rng.GetBytes(EncryptKey);
            rng.GetBytes(IV);
            rng.GetBytes(EncryptKey2);
            rng.GetBytes(IV2);
            rng.GetBytes(EncryptKey3);
            rng.GetBytes(IV3);

            AES.KeySize = 256;
            AES.BlockSize = 256;
            AES.Mode = CipherMode.CBC;
            AES.Padding = PaddingMode.None;
        }
        public void RunEncryptor(byte[] Key, byte[] IV, byte[] EncryptThis)
        {
            AES.Key = Key;
            AES.IV = IV;
            encryptor = AES.CreateEncryptor(AES.Key, AES.IV);
            encryptor.TransformBlock(EncryptThis, 0, EncryptThis.Length, EncryptThis, 0);
        }
        public void RunDecryptor(byte[] Key, byte[] IV, byte[] EncryptThis)
        {
            AES.Key = Key;
            AES.IV = IV;
            encryptor = AES.CreateDecryptor(AES.Key, AES.IV);
            encryptor.TransformBlock(EncryptThis, 0, EncryptThis.Length, EncryptThis, 0);
        }    
    }
}