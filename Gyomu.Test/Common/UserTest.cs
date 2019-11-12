using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class UserTest
    {
        [Fact]
        public void CurrentUserTest()
        {
            Gyomu.Common.User user = Gyomu.Common.User.CurrentUser;
            Assert.Equal(System.Environment.GetEnvironmentVariable("USERNAME").ToUpper(), user.UserID.ToUpper());
        }
        [Fact]
        public void InvalidUserTest()
        {
            Gyomu.Common.User user = Gyomu.Common.User.GetUser("hogehoge");
            Assert.False(user.IsValid);
        }
        [Fact]
        public void MockUserTest()
        {

        }
    }
}
