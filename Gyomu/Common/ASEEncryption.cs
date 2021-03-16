using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public class AESEncryption
    {
        private static GcmBlockCipher _cipher = new GcmBlockCipher(new AesEngine());
        private const int NONCE_BIT_SIZE = 128;
        private const int MAC_BIT_SIZE = 128;
        //private static PaddedBufferedBlockCipher _cipher = new PaddedBufferedBlockCipher(new AesEngine(), new Pkcs7Padding());
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
                byte[] nonce = new byte[NONCE_BIT_SIZE/8];
                if (forEncrypt)
                {
                    SecureRandom random = new SecureRandom();
                    random.NextBytes(nonce, 0, nonce.Length);
                    AeadParameters parameters = new AeadParameters(new KeyParameter(keyByte), MAC_BIT_SIZE, nonce);
                    _cipher.Init(forEncrypt, parameters);
                    byte[] output_bytes = new byte[_cipher.GetOutputSize(input.Length)];
                    int len = _cipher.ProcessBytes(input, 0, input.Length, output_bytes, 0);
                    _cipher.DoFinal(output_bytes, len);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        using (System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(ms))
                        {
                            binaryWriter.Write(nonce);
                            binaryWriter.Write(output_bytes);
                        }
                        return ms.ToArray();
                    }
                }
                else
                {
                    using(System.IO.MemoryStream ms = new System.IO.MemoryStream(input))
                    {
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                        {
                            nonce = reader.ReadBytes(NONCE_BIT_SIZE / 8);
                            AeadParameters parameters = new AeadParameters(new KeyParameter(keyByte), MAC_BIT_SIZE, nonce);
                            _cipher.Init(forEncrypt, parameters);
                            byte[] cipherBytes = reader.ReadBytes(input.Length - nonce.Length);
                            byte[] plain_bytes = new byte[_cipher.GetOutputSize(cipherBytes.Length)];
                            int len = _cipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plain_bytes, 0);
                            _cipher.DoFinal(plain_bytes, len);
                            return plain_bytes;
                        }
                    }
                }
                
                //_cipher.Init(forEncrypt, new KeyParameter(keyByte));
                //return _cipher.DoFinal(input);
            }
            catch (Org.BouncyCastle.Crypto.CryptoException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
