using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class AESEncryptTest
    {
        [Fact]
        public void AESEncryptionTest()
        {
            string plain = "Hello$Test";
            string key = "abc";
            string encData = Gyomu.Common.AESEncryption.AESEncrypt(plain, key);
            Assert.Equal(plain, Gyomu.Common.AESEncryption.AESDecrypt(encData, key));
        }
        [Fact]
        public void AESInvalidKeyExceptionTest()
        {
            string plain = "Hello$Test";
            string key = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTU";
            Assert.Throws<InvalidOperationException>(() =>
           {
               Gyomu.Common.AESEncryption.AESEncrypt(plain, key);
           });
        }
        [Fact]
        public void AESInvalidDecryptExceptionTest()
        {
            string plain = "Hello$Test";
            string key = "abc";
            string key2 = "def";
            string encData = Gyomu.Common.AESEncryption.AESEncrypt(plain, key);
            Assert.Throws<InvalidOperationException>(() =>
            {
                Gyomu.Common.AESEncryption.AESDecrypt(encData, key2);
            });
        }
    }
}
