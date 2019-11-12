using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class Base64EncodingTest
    {
        [Fact]
        public void Base64EncodeDecodeTest()
        {
            string plain = "Hello$Test";
            string enc = Gyomu.Common.Base64Encode.Encode(plain);
            Assert.Equal(plain, Gyomu.Common.Base64Encode.Decode(enc));
        }
        [Fact]
        public void Base64InvalidDecodeTest()
        {
            string plain = "Hello$Test";
            Assert.Throws<FormatException>(() =>
            {
                Gyomu.Common.Base64Encode.Decode(plain);
            });
        }
    }
}
