using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class UserEncryptionTest
    {
        [Fact]
        public void UserEncryptTest()
        {
            string value = "HELLO$1234G.xn";
            string encData = Gyomu.Common.UserEncryption.Encrypt(value);
            Assert.Equal(value, Gyomu.Common.UserEncryption.Decrypt(encData));
        }
        [Fact]
        public void UserInvalidDataDecryptTest()
        {
            string value = "HELLO$1234G.xn";
            Assert.Throws<System.FormatException>(() => {
                Gyomu.Common.UserEncryption.Decrypt(value);
            });
        }
    }
}
