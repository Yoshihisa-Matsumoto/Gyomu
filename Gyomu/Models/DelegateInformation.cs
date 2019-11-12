using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class DelegateInformation
    {
        public List<Common.WindowsUser> DelegateUserList { get; private set; }
        public bool DelegationRequired { get; private set; }
        private DelegateInformation(bool delegationRequired, List<Common.WindowsUser> delegateUsers)
        {

            DelegateUserList = delegateUsers;
            DelegationRequired = delegationRequired;
        }

        internal static DelegateInformation CreateNoDelegate()
        {
            return new DelegateInformation(false, null);
        }
        internal static DelegateInformation CreateDelegate(List<Common.WindowsUser> delegate_to)
        {
            return new DelegateInformation(true,  delegate_to);
        }
    }
}
