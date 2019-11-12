using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Transactions;
using System.Linq;
using System.Threading;

namespace Gyomu.Common.Task
{
    public abstract class AbstractBaseTask : INotifyPropertyChanged
    {
        #region Field


        protected string TaskDataParameter { get; private set; }
        private protected Models.TaskData CurrentTask { get; private set; }
        private protected Models.TaskInfo CurrentTaskInfo { get; set; }
        public long TaskDataID { get; internal set; }
        public long LatestInstanceID { get; private set; }
        public Configurator Config { get; internal set; }
        protected User CurrentUser { get { return Config.User; } }

        internal const string STATUS_INIT = "INIT";
        internal const string STATUS_EXECUTE = "EXEC";
        internal const string STATUS_COMPLETE = "COMPLETE";
        internal const string STATUS_FAIL = "FAIL";
        internal const string STATUS_REQUEST = "REQUEST";
        internal const string STATUS_APPROVAL = "APPROVE";
        internal const string STATUS_REJECT = "REJECT";
        internal const string STATUS_NOTEXEC = "NOTEXEC";
        internal const string STATUS_CANCEL = "CANCEL";
        internal const string STATUS_DELEGATE = "DELEGATE";
        /// <summary>
        /// Use it when you delegate task to the initiator of the task
        /// </summary>
        public const string DelegateBackToOwner = "@@OWNER@@";
        /// <summary>
        /// Use it when you delegate task back to previous person
        /// </summary>
        public const string DelegateBackToPreviousPerson = "@@PREVIOUS@@";
        /// <summary>
        /// Use it when you delegate task the person who requested approval
        /// </summary>
        public const string ProposeToOwner = "@@PROPOSITION_OWNER@@";
        /// <summary>
        /// Use it when you delegate task the persons to whom requester requested.
        /// </summary>
        public const string ProposeToDestination = "@@PROPOSITION_DESTINATION@@";
        
        protected bool CanRequest { get { return NextActionEnable(STATUS_REQUEST); } }
        protected bool CanApprove { get { return NextActionEnable(STATUS_APPROVAL); } }
        protected bool CanReject { get { return NextActionEnable(STATUS_REJECT); } }
        protected bool CanCancel { get { return NextActionEnable(STATUS_CANCEL); } }
        protected bool CanExecute { get { return NextActionEnable(STATUS_EXECUTE); } }
        
        public event EventHandler TaskFinished;
        public event PropertyChangedEventHandler PropertyChanged;
        protected internal Thread AsyncThread { get; set; }
        private static readonly TimeSpan _lock_timespan = new TimeSpan(0);
        private bool taskDataLoaded = false;
        private protected Models.TaskInstance LatestInstance { get; private set; }
        private List<Models.TaskInstance> Instances { get; set; }
        private List<Models.TaskAccessList> AccessList { get; set; }
        private object LockObject = new object();
        #endregion

        #region Constructor

        internal AbstractBaseTask()
        {
            PropertyChanged += AbstractBaseTask_PropertyChanged;
            OnInit();
        }
        private void AbstractBaseTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
                return;
            if (string.IsNullOrEmpty(e.PropertyName.Trim()))
                return;
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    Models.TaskDataLog taskLog = new Models.TaskDataLog() { task_data_id = TaskDataID, log = e.PropertyName ?? "" };
                    GyomuDataAccess.InsertTaskDataLog(ref taskLog);
                }
            }
            catch (Exception) { }
        }

        #endregion
        #region Public Method
        public StatusCode Start(string parameter, string comment)
        {
            StatusCode retVal = Init(parameter, comment);
            if (!retVal.IsSucceeded)
                return retVal;
            return Run(parameter, comment);
        }

        public StatusCode Restart()
        {
            lock (LockObject)
            {
                StatusCode retVal;
                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_EXECUTE);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        retVal = executionDecision(TaskDataParameter, null);
                    }
                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                return retVal;
            }
        }
        public StatusCode ExcecuteDelegateTask(string parameter, string comment)
        {
            lock (LockObject)
            {
                StatusCode retVal;
                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_EXECUTE);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        retVal = executionDecision(parameter, comment);
                    }
                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                return retVal;
            }
        }

        public StatusCode Approve(string parameter, string comment, long latest_instance_id)
        {
            lock (LockObject)
            {
                StatusCode retVal;
                LatestInstanceID = latest_instance_id;
                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_APPROVAL);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        //Create New Instance
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            retVal =
                                CreateNewInstance(parameter, STATUS_APPROVAL, null,
                                                  null,
                                                  comment);
                            scope.Complete();
                        }
                        if (!retVal.IsSucceeded)
                            return retVal;
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            retVal = OnApprove(parameter, comment);
                            if (!retVal.IsSucceeded)
                                return retVal;
                            if (IsEmailNecessary(STATUS_APPROVAL))
                            {
                                retVal = SendProposalMailFromRequestedStatus(ProposalRecipients, comment, STATUS_APPROVAL);
                                if (!retVal.IsSucceeded)
                                    return retVal;
                            }
                        }
                        retVal = executionDecision(parameter, comment);
                    }
                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                return retVal;
            }
        }
        public StatusCode Reject(string parameter, string comment, long latest_instance_id)
        {
            lock (LockObject)
            {
                StatusCode retVal;
                LatestInstanceID = latest_instance_id;
                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_REJECT);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        //Create New Instance
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            retVal =
                                CreateNewInstance(parameter, STATUS_REJECT, null, 
                                                  null,
                                                  comment);
                            scope.Complete();
                        }
                    }
                    if (!retVal.IsSucceeded)
                        return retVal;


                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        retVal = OnReject(parameter, comment);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        if (IsEmailNecessary(STATUS_REJECT))
                        {
                            retVal = SendProposalMailFromRequestedStatus(ProposalRecipients, comment, STATUS_REJECT);
                            if (!retVal.IsSucceeded)
                                return retVal;
                        }
                    }

                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                return retVal;
            }
        }
        public StatusCode Abort(string parameter, string comment)
        {
            StatusCode retVal =
                                CreateNewInstance(parameter, STATUS_CANCEL, null,  null, comment);
            if (retVal.IsSucceeded == false)
                return retVal;
            return OnCancel(parameter, comment);
        }
        public StatusCode Cancel(string parameter, string comment, long latest_instance_id)
        {
            lock (LockObject)
            {
                StatusCode retVal;
                LatestInstanceID = latest_instance_id;
                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_CANCEL);
                        if (!retVal.IsSucceeded)
                            return retVal;
                    }
                    if (!retVal.IsSucceeded)
                        return retVal;

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        retVal =
                            CreateNewInstance(parameter, STATUS_CANCEL, null,  null, comment);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        retVal = OnCancel(parameter, comment);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        retVal = SendProposalMailFromRequestedStatus(ProposalRecipients, comment, STATUS_CANCEL);
                        if (!retVal.IsSucceeded)
                            return retVal;

                    }

                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                return retVal;
            }
        }

        public StatusCode Request(string parameter, string comment, long latest_instance_id)
        {
            lock (LockObject)
            {
                StatusCode retVal;
                LatestInstanceID = latest_instance_id;

                try
                {
                    //Just Lock the existing Instance * No Commit!!
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        retVal = CheckStatusAndOwner(STATUS_REQUEST);
                        if (!retVal.IsSucceeded)
                            return retVal;
                        Models.ProposalInformation apply_info  = GetProposalInformation;
                        if (apply_info.ProposalRequired)
                        {
                            return SendApply(apply_info, parameter, comment);
                        }
                        return retVal;
                    }
                }
                catch (Exception ex)
                {
                    return
                        new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
            }
        }

        public StatusCode GetProposalMessageForGUI(out string strMessage)
        {
            strMessage = null;
            bool isProposeItemFound = false;
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            if (CanApprove)
            {
                foreach (Models.TaskInstance instance in Instances)
                {
                    if (isProposeItemFound == false && instance.task_status.Equals(STATUS_REQUEST))
                    {
                        isProposeItemFound = true;
                        strMessage = instance.comment;
                        if (string.IsNullOrEmpty(strMessage) == false)
                            break;
                    }
                    if (isProposeItemFound && instance.task_status.Equals(STATUS_INIT))
                    {
                        strMessage = instance.comment;
                    }
                }
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    retVal = OnCustomProposalMessage(ref strMessage);
                }
            }
            return retVal;
        }
        public void Notify(string msg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(msg));
        }
        public void Notify(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        public bool WaitForCompletion(int timeout_sec, int every_sec)
        {
            bool result = false;
            DateTime targetTime = DateTime.UtcNow;
            //TimeSpan span_target = new TimeSpan(0, 0, timeout_sec);
            int monitor_time = every_sec * 1000;
            while (DateTime.UtcNow.Subtract(targetTime).TotalSeconds < timeout_sec && result == false)
            {
                result = IsFinished;
                if (result == false)
                {
                    System.Threading.Thread.Sleep(monitor_time);
                }
            }
            result = IsFinished;
            return result;
        }
        public bool FinishStatus(out StatusCode retVal)
        {
            retVal = StatusCode.SUCCEED_STATUS;

            try
            {
                loadTaskData();
                if (IsFinished == false)
                    return false;

                long status_id = 0;
                if (LatestInstance != null)
                {
                    if (LatestInstance.status_info_id.HasValue == false)
                    {
                        retVal = StatusCode.SUCCEED_STATUS;
                        return true;
                    }
                    status_id = LatestInstance.status_info_id.Value;
                }
                retVal = StatusCode.GetStatusCode(status_id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
        #region Internal Method
        internal StatusCode Run(string parameter, string comment)
        {
            try
            {
                lock (LockObject)
                {
                    using (new TransactionScope(TransactionScopeOption.RequiresNew, _lock_timespan))
                    {
                        LockInstanceAndRefreshCurrentTaskData();
                        return executionDecision(null, comment);
                    }
                }
            }
            catch (Exception ex)
            {
                return
                    new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
            }
        }

        internal StatusCode Init(string parameter, string comment)
        {
            StatusCode retVal;
            try
            {
                //User Check
                retVal = CheckStatusAndOwner(STATUS_INIT);
                if (!retVal.IsSucceeded)
                    return retVal;
                TaskDataParameter = parameter;
                if (TaskDataID != -1)
                {
                    return
                        new CommonStatusCode(CommonStatusCode.TASK_ALREADY_GENERATED, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, Config, ApplicationID);
                }
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {

                    retVal = CreateNewTask(comment, null);
                    if (!retVal.IsSucceeded)
                        return retVal;
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                retVal =
                    new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
            }
            return retVal;
        }
        #endregion
        #region Private Method


        private StatusCode executionDecision(string parameter, string comment)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            Models.ProposalInformation apply_info = GetProposalInformation;
            if (apply_info.ProposalRequired)
            {
                retVal = SendApply(apply_info, parameter, comment);
            }
            else
            {
                Models.DelegateInformation delegate_info = DelegateInformation;
                if (!retVal.IsSucceeded)
                    return retVal;
                if (IsAsynchrnous && delegate_info.DelegateUserList.Contains(CurrentUser) == false)
                {
                    //Create New Instance Delegate
                    if (!retVal.IsSucceeded)
                        return retVal;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        retVal = OnDelegate(parameter, comment);
                    }
                    if (!retVal.IsSucceeded)
                        return retVal;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        retVal =
                            CreateNewInstance(parameter, STATUS_DELEGATE,
                                              delegate_info.DelegateUserList,  null, comment);
                        scope.Complete();
                    }
                }
                else
                {
                    if (IsAsynchrnous)
                    {
                        //Create New Instance Executing
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            retVal =
                                CreateNewInstance(parameter, STATUS_EXECUTE, null,  null, null);
                            scope.Complete();
                        }
                        if (!retVal.IsSucceeded)
                            return retVal;
                        //Create Thread and call OnExec
                        Thread th = new Thread(new ParameterizedThreadStart(this.DoAsyncExec));
                        th.Start(parameter);
                        AsyncThread = th;
                    }
                    else
                    {
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            retVal =
                                CreateNewInstance(parameter, STATUS_EXECUTE, null,  null, null);

                            scope.Complete();
                        }
                        if (!retVal.IsSucceeded)
                            return retVal;
                        //Call OnExec;
                        retVal = DoExec(parameter);
                    }
                }
            }
            return retVal;
        }

        private void DoAsyncExec(object objParm)
        {
            string parameter = null;
            if (objParm != null)
                parameter = objParm.ToString();
            DoExec(parameter);
        }
        private StatusCode DoExec(string parameter)
        {
            StatusCode retVal;
            try
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        retVal = OnExec(parameter ?? TaskDataParameter);
                    }
                    if (retVal.IsSucceeded)
                        Notify("[Task] " + this.CurrentTaskInfo.description + " Done");
                }
                catch (Exception ex)
                {
                    retVal =
                        new CommonStatusCode(CommonStatusCode.TASK_EXECUTION_FAILED, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex, Config, ApplicationID);
                }
                //Create new Instance 
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    if (!retVal.IsSucceeded)
                    {
                        StatusCode retVal2 = CreateNewInstance(parameter, STATUS_FAIL, null,  retVal, null);
                        if (retVal2.IsSucceeded)
                            retVal = retVal2;
                    }
                    else
                    {
                        retVal =
                            CreateNewInstance(parameter, STATUS_COMPLETE,
                                               null,  null, null);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex2)
            {
                retVal =
                    new CommonStatusCode(CommonStatusCode.TASK_LIBRARY_INTERNAL_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter, TaskDataID }, ex2, Config, ApplicationID);
            }
            TaskFinished?.Invoke(this, null);
            return retVal;
        }
        private StatusCode SendApply(Models.ProposalInformation proposalInformation, string parameter, string comment)
        {
            StatusCode retVal;
            retVal = CheckStatusAndOwner(STATUS_REQUEST);
            if(!retVal.IsSucceeded)
                return retVal;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                retVal =
                    CreateNewInstance(parameter, STATUS_REQUEST, proposalInformation.DestinationPersons,
                                       null, comment);
                scope.Complete();
            }
            if (!retVal.IsSucceeded)
                return retVal;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                retVal = OnRequest(parameter, comment);
                if (!retVal.IsSucceeded)
                    return retVal;
                if (IsEmailNecessary(STATUS_REQUEST))
                {
                    return SendProposalMailFromRequestedStatus(proposalInformation.DestinationPersons, comment, STATUS_REQUEST);
                }
            }
            return retVal;
        }

        private StatusCode SendProposalMailFromRequestedStatus(List<WindowsUser> recipients, string comment, string requestStatus)
        {
            StringBuilder strMailTitle = new StringBuilder();
            StringBuilder strMailBody = new StringBuilder();
            string requestAction = "";
            switch (requestStatus)
            {
                case STATUS_REQUEST:
                    requestAction = "submitted";
                    break;
                case STATUS_APPROVAL:
                    requestAction = "approved";
                    break;
                case STATUS_REJECT:
                    requestAction = "rejected";
                    break;
                case STATUS_CANCEL:
                    requestAction = "cancelled";
                    break;
            }

            if (CurrentTaskInfo != null)
            {
                strMailTitle.Append(CurrentTaskInfo.description + " approval request " + requestAction + " by " + CurrentUser.UserID);
            }
            if (string.IsNullOrEmpty(comment) == false)
                strMailBody.Append(comment);
            else
                comment = "";

            string mail_title = strMailTitle.ToString();
            string mail_body = strMailBody.ToString();
            string custom_title = mail_title;
            string custom_body = mail_body;
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    OnCustomMailInformation(requestStatus, recipients, comment, ref custom_title, ref custom_body);

                }
                mail_title = custom_title;
                mail_body = custom_body;
            }
            catch (Exception ex)
            {
                return new CommonStatusCode(CommonStatusCode.OVERRIDE_METHOD_OTHER_ERROR, ex, Config, ApplicationID);
            }

            Net.Email email = new Net.Email(ApplicationID, Config);
            Common.WindowsUser winUser = Config.User as WindowsUser;
            List<string> recipientAddressList = new List<string>();
            foreach (WindowsUser recipient in recipients)
            {
                string mailAddress = recipient.MailAddress;
                if (string.IsNullOrEmpty(mailAddress) == false && recipientAddressList.Contains(mailAddress) == false)
                    recipientAddressList.Add(mailAddress);
            }
            return email.Send(winUser?.MailAddress, Config.Username, recipientAddressList.ToArray(), null, mail_title, mail_body, null);

        }


        private bool internalValidStatus(string target_status_mnemonic)
        {
            if (LatestInstance == null)
                return true;
            string current_status_mnemonic = LatestInstance.task_status;
            switch (target_status_mnemonic)
            {
                case STATUS_APPROVAL:
                    if (current_status_mnemonic.Equals(STATUS_REQUEST))
                        return true;
                    break;
                case STATUS_REJECT:
                    if (current_status_mnemonic.Equals(STATUS_REQUEST))
                        return true;
                    break;
                case STATUS_EXECUTE:  //Sould be here only when it's delegated execution call or when user choose to restart
                    switch (current_status_mnemonic)
                    {
                        case STATUS_DELEGATE:
                        case STATUS_INIT:
                        case STATUS_APPROVAL:
                            return true;
                    }
                    if (current_status_mnemonic.Equals(STATUS_COMPLETE) || current_status_mnemonic.Equals(STATUS_FAIL))
                    {
                        if (CurrentTaskInfo.restartable)
                            return true;
                    }
                    break;
                case STATUS_REQUEST:
                    if (current_status_mnemonic.Equals(STATUS_INIT)
                        || current_status_mnemonic.Equals(STATUS_APPROVAL)
                        || current_status_mnemonic.Equals(STATUS_CANCEL)
                        || current_status_mnemonic.Equals(STATUS_REJECT)
                        )
                        return true;
                    break;
                case STATUS_DELEGATE:
                    if (current_status_mnemonic.Equals(STATUS_INIT)
                        || current_status_mnemonic.Equals(STATUS_APPROVAL))
                        return true;
                    break;
                case STATUS_CANCEL:
                    if (current_status_mnemonic.Equals(STATUS_REQUEST))
                        return true;
                    break;
                default:
                    return true;

            }
            return false;
        }
        internal StatusCode CheckStatusAndOwner(string targetStatus)
        {
            if (internalValidStatus(targetStatus) == false)
            {
                return
                    new CommonStatusCode(CommonStatusCode.TASK_STATUS_INCONSISTENT,
                    new object[] { ApplicationID, TaskInfoID, TaskDataID, LatestInstanceID, LatestInstance.task_status, targetStatus }, Config, ApplicationID);
            }
            else
            {
                if (internalValidOwner(targetStatus) == false)
                {
                    return
                        new CommonStatusCode(CommonStatusCode.INVALID_USER_ACCESS,
                        new object[] { ApplicationID, TaskInfoID, TaskDataID, LatestInstanceID, targetStatus, CurrentUser.UserID }, Config, ApplicationID);
                }
                else
                    return StatusCode.SUCCEED_STATUS;
            }
        }
        private bool internalValidOwner(string target_status_mnemonic)
        {
            string current_status = null;
            if (LatestInstance == null)
                return true;
            if (target_status_mnemonic.Equals(STATUS_INIT) == false)
                current_status = LatestInstance.task_status;
            User requestor = null;
            switch (target_status_mnemonic)
            {
                case STATUS_INIT:
                    if (internalCheckTaskAccessibility())
                        return true;
                    break;
                case STATUS_APPROVAL:
                case STATUS_REJECT:
                    //Get Submit Persons
                    if (IsSelfApproveEnable == false)
                    {
                        requestor = User.GetUser(LatestInstance.entry_author);
                        if (requestor.IsValid && requestor.Equals(CurrentUser))
                            return false;
                    }
                    foreach (Models.TaskSubmitInformation taskSubmitInformation in GyomuDataAccess.SelectTaskSubmitInformation(LatestInstance))
                    {
                        User submit_user = User.GetUser(taskSubmitInformation.submit_to);
                        if (submit_user.IsValid)
                        {
                            if (submit_user.IsGroup == false && submit_user.Equals(CurrentUser))
                                return true;
                            if (submit_user.IsGroup)
                            {
                                if (CurrentUser.IsInGroup(submit_user))
                                    return true;
                            }
                        }
                    }
                    break;
                case STATUS_EXECUTE:
                    //If Current Status is not Delegate, it must be equal to Latest instance's user
                    if (STATUS_DELEGATE.Equals(current_status) == false)
                    {
                        Models.DelegateInformation info = DelegateInformation;
                        if (info.DelegationRequired)
                        {
                            if (Instances.Where(i => i.task_status.Equals(STATUS_DELEGATE) && i.entry_author.Equals(CurrentUser.UserID)).Count() > 0)
                                return true;
                            User previousUser = User.GetUser(LatestInstance.entry_author);
                            if (previousUser.IsValid && previousUser.Equals(CurrentUser))
                                return true;
                        }
                        else
                        {
                            User previousUser = User.GetUser(LatestInstance.entry_author);
                            if (previousUser.IsValid && previousUser.Equals(CurrentUser))
                                return true;
                        }
                    }
                    //Else it must be submit user in the latest instance
                    else
                    {
                        foreach (Models.TaskSubmitInformation taskSubmitInformation in GyomuDataAccess.SelectTaskSubmitInformation(LatestInstance))
                        {
                            User submit_user = User.GetUser(taskSubmitInformation.submit_to);
                            if (submit_user.IsValid)
                            {
                                if (submit_user.IsGroup == false && submit_user.Equals(CurrentUser))
                                    return true;
                                if (submit_user.IsGroup && CurrentUser.IsInGroup(submit_user))
                                    return true;
                            }
                        }
                    }
                    break;
                case STATUS_REQUEST:
                    //Approver or Starter
                    User starter = User.GetUser(CurrentTask.entry_author);
                    if (starter.IsValid && starter.Equals(CurrentUser))
                        return true;
                    if (LatestInstance.task_status.Equals(STATUS_APPROVAL))
                    {
                        User approver = User.GetUser(LatestInstance.entry_author);
                        if (approver.IsValid && approver.Equals(CurrentUser))
                            return true;
                    }
                    break;
                case STATUS_CANCEL:
                    //Requester
                    requestor = User.GetUser(LatestInstance.entry_author);
                    if (requestor.IsValid && requestor.Equals(CurrentUser))
                        return true;
                    break;
                case STATUS_COMPLETE:
                    //For Force Success
                    if (internalCheckTaskAccessibility() == false)
                        return false;
                    if (current_status.Equals(STATUS_FAIL))
                        return true;
                    break;
                default:
                    return true;
            }
            return false;
        }
        private bool internalCheckTaskAccessibility()
        {
            if (AccessList.Count == 0)
                return true;
            string currentUserName = Config.Username.ToUpper();
            bool canAccess = false;
            foreach (Models.TaskAccessList access in AccessList)
            {
                if (currentUserName.Equals(access.account_name.ToUpper()))
                {
                    if (access.forbidden)
                        return false;
                    if (access.can_access)
                        canAccess = true;
                }
                User targetUser = User.GetUser(access.account_name);
                if (targetUser.IsValid && targetUser.IsGroup)
                {
                    if (Config.User.IsInGroup(targetUser))
                    {
                        if (access.forbidden)
                            return false;
                        if (access.can_access)
                            canAccess = true;
                    }

                }
            }

            return canAccess;
        }



        private List<WindowsUser> ProposalRecipients
        {
            get
            {
                List<WindowsUser> recipients = new List<WindowsUser>();
                Models.TaskInstance requestRow = Instances.Where(i => i.task_status.Equals(STATUS_REQUEST)).OrderByDescending(i => i.entry_date).FirstOrDefault();
                if (requestRow != null)
                {
                    WindowsUser user = WindowsUser.GetWindowsUser(requestRow.entry_author);
                    if (user.IsValid)
                        recipients.Add(user);
                    foreach (Models.TaskSubmitInformation taskSubmitInformation in GyomuDataAccess.SelectTaskSubmitInformation(LatestInstance))
                    {
                        user = WindowsUser.GetWindowsUser(taskSubmitInformation.submit_to);
                        if (user.IsValid && recipients.Contains(user) == false)
                            recipients.Add(user);

                    }
                }
                return recipients;
            }
        }
        private bool NextActionEnable(string status)
        {
            lock (LockObject)
            {
                loadTaskData();
                bool eligiblity = true;
                if (LatestInstance == null)
                    return false;
                eligiblity = internalValidOwner(status) &&
                                  internalValidStatus(status);
                switch (status)
                {
                    case STATUS_REQUEST:

                        if (eligiblity)
                        {
                            Models.ProposalInformation proposalInformation = GetProposalInformation;
                            if (proposalInformation.ProposalRequired)
                                return true;
                        }
                        return false;
                }
                return eligiblity;
            }
        }
        #endregion
        #region Abstract Method&Field
        public abstract short ApplicationID { get; }
        public abstract short TaskInfoID { get; }
        private protected abstract Models.DelegateInformation DelegateInformation { get; }


        private protected abstract Models.ProposalInformation GetProposalInformation{get;}
        protected abstract StatusCode OnExec(string parameter);

        #endregion
        #region Virtual Method&Field
        protected virtual bool IsEmailNecessary(string requestStatus) { return true; }
        protected virtual bool IsSelfApproveEnable { get { return true; } }
        public virtual string CustomTaskParameterViewClass { get { return null; } }
        public virtual Type CustomTaskParameterClass { get { return null; } }
        public virtual string[] InputMilestones { get { return new string[] { }; } }
        public virtual Dictionary<string, Models.FilePath> InputFiles { get { return new Dictionary<string, Models.FilePath>(); } }
        public virtual Dictionary<string, Models.FilePath> ExportedFiles { get { return new Dictionary<string, Models.FilePath>(); } }
        public virtual string[] ExportedMilestones { get { return new string[] { }; } }
        protected virtual void OnInit() { }
        protected virtual StatusCode OnCustomProposalMessage(ref string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }

        protected virtual void OnCustomMailInformation(string taskStatus, List<WindowsUser> recipients, string comment, ref string mail_title, ref string mail_body)
        {
        }
        protected virtual StatusCode OnRequest(string parameter, string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }
        protected virtual StatusCode OnReject(string parameter, string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }
        protected virtual StatusCode OnApprove(string parameter, string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }
        protected virtual StatusCode OnCancel(string parameter, string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }
        protected virtual StatusCode OnDelegate(string parameter, string comment)
        {
            return StatusCode.SUCCEED_STATUS;
        }
        private protected virtual bool IsAsynchrnous { get { return false; } }
        #endregion
        #region Common Database Access
        private StatusCode CreateNewTask(string comment, Models.TaskData parentTask)
        {
            try
            {
                GyomuDataAccess.CreateNewTask(CurrentTaskInfo, Config, TaskDataParameter, comment, parentTask, out Models.TaskData taskData, out Models.TaskInstance taskInstance, out Models.TaskDataStatus taskStatus);
                CurrentTask = taskData;
                LatestInstance = taskInstance;
                Instances = new List<Models.TaskInstance>() { LatestInstance };
                LatestInstanceID = LatestInstance.id;
            }
            catch (Exception ex)
            {
                return
                    new CommonStatusCode(CommonStatusCode.TASK_GENERATE_ERROR, new object[] { ApplicationID, TaskInfoID, TaskDataParameter }, ex, Config,ApplicationID);
            }
            return StatusCode.SUCCEED_STATUS;
        }


        private StatusCode CreateNewInstance(string parameter, string mnemonic, List<WindowsUser> submit_to,StatusCode statusCode, string comment)
        {
            try
            {
                GyomuDataAccess.CreateNewTaskInstance(CurrentTask, Config, mnemonic, parameter, comment, statusCode,submit_to, out Models.TaskInstance taskInstance);
                LatestInstance = taskInstance;
                LatestInstanceID = LatestInstance.id;
                Instances.Add(LatestInstance);
                Notify(null);
            }
            catch (Exception ex)
            {
                return
                    new CommonStatusCode(CommonStatusCode.TASK_INSTANCE_GENERATE_ERROR, 
                    new object[] { ApplicationID,TaskInfoID,TaskDataID,parameter}, ex,
                                              Config,ApplicationID);
            }
            return StatusCode.SUCCEED_STATUS;
        }

        
        private void loadTaskData()
        {
            if (taskDataLoaded)
                return;
            if (CurrentTask == null)
                CurrentTask = GyomuDataAccess.SelectTaskData(TaskDataID);
            TaskDataParameter = CurrentTask.parameter;
            Instances = GyomuDataAccess.SelectTaskInstanceByTaskData(CurrentTask);
            LatestInstance = Instances.FirstOrDefault();
            LatestInstanceID = LatestInstance.id;
            AccessList = GyomuDataAccess.SelectTaskAccessLists(ApplicationID, TaskInfoID);
            taskDataLoaded = true;
        }

        

        

        private void LockInstanceAndRefreshCurrentTaskData()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;            
            CurrentTask = GyomuDataAccess.LockTaskData(TaskDataID);
            loadTaskData();
        }

        public bool IsFinished
        {
            get
            {
                Models.TaskDataStatus status = GyomuDataAccess.SelectTaskStatus(CurrentTask);
                switch (status.task_status)
                {
                    case STATUS_COMPLETE:
                    case STATUS_FAIL:
                        return true;
                }
                return false;
            }
        }
        protected Models.TaskInstance FindLatestInstance(string targetStatus)
        {            
            loadTaskData();
            return Instances.Where(i => i.task_status.Equals(targetStatus)).OrderByDescending(i => i.entry_date).FirstOrDefault();
        }
        protected List<Models.TaskSubmitInformation> LoadSubmitInformation(Models.TaskInstance taskInstance)
        {
            return GyomuDataAccess.SelectTaskSubmitInformation(taskInstance);
        }
        #endregion
    }
}
