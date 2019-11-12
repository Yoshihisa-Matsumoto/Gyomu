using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public class AESEncryption
    {
        private static PaddedBufferedBlockCipher _cipher = new PaddedBufferedBlockCipher(new AesEngine(), new Pkcs7Padding());
        private static string getUsingKey(string key)
        {
            if (key.Length < 16)
            {
                return key.PadRight(16, ' ');
            }
            else if (key.Length == 16)
                return key;
            else if (key.Length < 32)
            {
                return key.PadRight(32, ' ');
            }
            else if (key.Length == 32)
                return key;
            else
                throw new InvalidOperationException("Key length must be less than 32");
        }
        public static string AESEncrypt(string plain, string key, Encoding enc = null)
        {

            lock (_cipher)
            {
                if (enc == null)
                    enc = Encoding.UTF8;

                byte[] result = bouncyCastleCrypto(true, enc.GetBytes(plain), key, enc);
                return Convert.ToBase64String(result);
            }
        }
        public static string AESDecrypt(string cipher, string key, Encoding enc = null)
        {
            lock (_cipher)
            {
                if (enc == null)
                    enc = Encoding.UTF8;

                byte[] result = bouncyCastleCrypto(false, Convert.FromBase64String(cipher), key, enc);
                return enc.GetString(result);
            }
        }
        private static byte[] bouncyCastleCrypto(bool forEncrypt, byte[] input, string key, Encoding enc)
        {
            string key2 = getUsingKey(key);
            try
            {
                byte[] keyByte = enc.GetBytes(key2);
                _cipher.Init(forEncrypt, new KeyParameter(keyByte));
                return _cipher.DoFinal(input);
            }
            catch (Org.BouncyCastle.Crypto.CryptoException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
