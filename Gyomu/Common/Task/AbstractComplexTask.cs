using System;
using System.Collections.Generic;
using System.Text;
using Gyomu.Models;

namespace Gyomu.Common.Task
{
    public abstract class AbstractComplexTask : AbstractBaseTask
    {

        public abstract bool SupportDelegate { get; }

        protected abstract List<string> DelegatePersons
        {
            get;
        }

        protected abstract bool NeedMoreApproval { get; }

        protected abstract List<WindowsUser> ApprovalPersons { get; }



        private protected override DelegateInformation DelegateInformation
        {
            get
            {
                if (SupportDelegate && DelegatePersons != null && DelegatePersons.Count > 0)
                {
                    List<WindowsUser> delegate_users = GetUser(DelegatePersons);
                    return DelegateInformation.CreateDelegate(delegate_users);
                }
                else
                    return Models.DelegateInformation.CreateNoDelegate();
            }
        }
        protected List<WindowsUser> GetUser(List<string> lstUserName)
        {
            List<WindowsUser> lstUser = new List<WindowsUser>();
            WindowsUser target_user = null;
            Models.TaskInstance taskInstance = null;
            foreach (string user_name in lstUserName)
            {
                switch (user_name)
                {
                    case DelegateBackToOwner:
                        if (CurrentTask!=null)
                        {
                            target_user = WindowsUser.GetWindowsUser(CurrentTask.entry_author);
                            if (target_user.IsValid)
                                lstUser.Add(target_user);
                        }
                        break;
                    case DelegateBackToPreviousPerson:
                        taskInstance = FindLatestInstance(STATUS_REQUEST);
                        if (taskInstance == null && CurrentUser is WindowsUser)
                            lstUser.Add(CurrentUser as WindowsUser);
                        else
                        {
                            target_user = WindowsUser.GetWindowsUser(taskInstance.entry_author);
                            if (target_user.IsValid)
                                lstUser.Add(target_user);
                        }

                        break;
                    case ProposeToDestination:

                        taskInstance = FindLatestInstance(STATUS_REQUEST);
                        if (taskInstance != null)
                        {

                            List<Models.TaskSubmitInformation> taskSubmitList = LoadSubmitInformation(taskInstance);
                            foreach (
                                TaskSubmitInformation taskSubmit in
                                    taskSubmitList)
                            {
                                target_user = WindowsUser.GetWindowsUser(taskSubmit.submit_to);
                                if (target_user.IsValid && lstUser.Contains(target_user) == false)
                                    lstUser.Add(target_user);
                            }
                        }
                        break;
                    case ProposeToOwner:
                        taskInstance = FindLatestInstance(STATUS_REQUEST);
                        if (taskInstance != null)
                        {
                            target_user = WindowsUser.GetWindowsUser(taskInstance.entry_author);
                            if (target_user.IsValid && lstUser.Contains(target_user) == false)
                                lstUser.Add(target_user);
                        }
                        break;
                    default:
                        target_user = WindowsUser.GetWindowsUser(user_name);
                        if (target_user.IsValid && lstUser.Contains(target_user) == false)
                            lstUser.Add(target_user);
                        break;
                }
            }
            return lstUser;
        }
        private protected override ProposalInformation GetProposalInformation
        {
            get
            {
                ProposalInformation proposalInformation = null;
                if (NeedMoreApproval)
                {
                    proposalInformation = new ProposalInformation(true);
                    if (ApprovalPersons != null)
                    {
                        foreach (WindowsUser user in ApprovalPersons)
                        {
                            if (user.IsValid && proposalInformation.DestinationPersons.Contains(user) == false)
                                proposalInformation.DestinationPersons.Add(user);
                        }
                    }
                    return proposalInformation;
                }
                else
                {
                    return new ProposalInformation(false);
                }
            }
        }
        
    }
}
