using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class ProposalInformation
    {
        public List<Common.WindowsUser> DestinationPersons { get; private set; }

        public bool ProposalRequired { get; private set; } 

        public ProposalInformation(bool isNeeded)
        {
            ProposalRequired = isNeeded;
            DestinationPersons = new List<Common.WindowsUser>();
        }
    }
}
