using System;
using System.Collections.Generic;
using System.Text;
using Gyomu.Models;

namespace Gyomu.Common.Task
{
    public abstract class AbstractSimpleTask:AbstractBaseTask
    {
        private protected override DelegateInformation DelegateInformation
        {
            get
            {
                return DelegateInformation.CreateNoDelegate();
            }
        }
        private protected override ProposalInformation GetProposalInformation
        {
            get
            {
                return new ProposalInformation(false);
            }
        }
    }
}
