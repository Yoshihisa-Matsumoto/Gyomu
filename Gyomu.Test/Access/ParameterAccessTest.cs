using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class ParameterAccessTest
    {
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void StringValuePropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, stringSetParameter);
        }

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void BoolValuePropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,boolSetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void DecimalValuePropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,decimalSetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void StringListValuePropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,stringListSetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void StringBase64PropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,stringBase64SetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void StringAESPropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,stringAESSetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void UserStringEncryptTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,userEncryptSetParameter);
        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void AdminAESPropertyTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db,stringAdminAESSetParameter);
        }
        private void stringSetParameter()
        {

            string key = "TEST_STRING_TEST";
            string itemValue = "Value@!@#$%";
            Gyomu.Access.ParameterAccess.SetStringValue(key, itemValue);

            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetStringValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));

        }
        private void intSetParameter()
        {
            string key = "TEST_INT_TEST";
            int itemValue = 12345678;
            Gyomu.Access.ParameterAccess.SetIntValue(key, itemValue);
            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetIntValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));
        }

        private void decimalSetParameter()
        {
            string key = "TEST_DECIMAL_TEST";
            decimal itemValue = (decimal)12345678.9012;
            Gyomu.Access.ParameterAccess.SetDecimalValue(key, itemValue);
            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetDecimalValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));
        }
        private void boolSetParameter()
        {
            string key = "TEST_BOOL_TEST";
            bool itemValue = true;
            Gyomu.Access.ParameterAccess.SetBoolValue(key, itemValue);
            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetBoolValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));
        }
        private void stringListSetParameter()
        {
            string key = "TEST_STRINGLIST_TEST";
            List<string> itemValue = new List<string>() { "Value@!@#$%", "ABC#DI$FG" };
            Gyomu.Access.ParameterAccess.SetStringListValue(key, itemValue);
            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetStringListValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));
        }
        private void stringBase64SetParameter()
        {
            string key = "TEST_BASE64_TEST";
            string itemValue = "Value@!@#$%";
            Gyomu.Access.ParameterAccess.SetStringValueWithBase64Encode(key, itemValue);

            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetBase64EncodedStringValue(key));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));

        }

        private void stringAESSetParameter()
        {
            string aes_original_key = Gyomu.Access.ParameterAccess.GetBase64EncodedStringValue("gyomu_aes_key");

            if (string.IsNullOrEmpty(aes_original_key))
                Gyomu.Access.ParameterAccess.SetStringValueWithBase64Encode("gyomu_aes_key","AES_KEY_TEST");

            string key = "TEST_AES_TEST";
            string itemValue = "Value@!@#$%";
            Gyomu.Access.ParameterAccess.SetStringValueWithAESEncryption(key, itemValue);

            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetAESEncryptStringValue(key));

            if (string.IsNullOrEmpty(aes_original_key))
                Gyomu.Access.ParameterAccess.SetStringValue("gyomu_aes_key", null);

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));

            

        }
        private void userEncryptSetParameter()
        {
            Gyomu.Common.User currentUser = Gyomu.Common.User.CurrentUser;
            string root_key = Gyomu.Access.ParameterAccess.GetKey(Gyomu.Access.ParameterAccess.USER_ROOTKEY, currentUser);
            bool isRootKeyExist = Gyomu.Access.ParameterAccess.KeyExist(root_key, new List<Models.ParameterMaster>());
            if (isRootKeyExist == false)
            {
                Gyomu.Access.ParameterAccess.SetRootKeyWithUserEncryption(Gyomu.Access.ParameterAccess.USER_ROOTKEY, "TestData", currentUser);
            }

            string key = "TEST_USERENC_TEST";
            string itemValue = "Value@!@#$%";
            Gyomu.Access.ParameterAccess.SetStringValueWithUserEncryption(key, itemValue, currentUser);

            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetUserEncryptedStringValue(key,currentUser));

            Gyomu.Access.ParameterAccess.SetStringValue(key, null,currentUser);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key,currentUser));

            if (isRootKeyExist == false)
                Gyomu.Access.ParameterAccess.SetStringValue(root_key, null);
        }
        private void stringAdminAESSetParameter()
        {
            Gyomu.Common.User currentUser = Gyomu.Common.User.CurrentUser;

            string root_key = Gyomu.Access.ParameterAccess.GetKey(Gyomu.Access.ParameterAccess.USER_ROOTKEY, currentUser);
            bool isRootKeyExist = Gyomu.Access.ParameterAccess.KeyExist(root_key, new List<Models.ParameterMaster>());
            if (isRootKeyExist == false)
            {
                Gyomu.Access.ParameterAccess.SetRootKeyWithUserEncryption(Gyomu.Access.ParameterAccess.USER_ROOTKEY, "TestData", currentUser);
            }

            string adminaes_original_key = Gyomu.Access.ParameterAccess.GetUserEncryptedStringValue("gyomu_admin_aes_key", currentUser);

            if (string.IsNullOrEmpty(adminaes_original_key))
                Gyomu.Access.ParameterAccess.SetStringValueWithUserEncryption("gyomu_admin_aes_key", "AES_KEY_TEST", currentUser);

            string key = "TEST_AES_TEST";
            string itemValue = "Value@!@#$%";
            Gyomu.Access.ParameterAccess.SetStringValueWithAdminAESEncryption(key,itemValue,currentUser );

            Assert.Equal(itemValue, Gyomu.Access.ParameterAccess.GetAdminAESEncryptStringValue(key,currentUser));

            if (string.IsNullOrEmpty(adminaes_original_key))
                Gyomu.Access.ParameterAccess.SetStringValue("gyomu_admin_aes_key", null,currentUser);

            Gyomu.Access.ParameterAccess.SetStringValue(key, null);
            Assert.Null(Gyomu.Access.ParameterAccess.GetStringValue(key));

            if (isRootKeyExist == false)
                Gyomu.Access.ParameterAccess.SetStringValue(root_key, null);

        }
    }
}
