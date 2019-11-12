using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Gyomu.Common.Net
{
    public class Email
    {
        private readonly short _app_id;
        private Common.Configurator Config { get; set; }
        public Email(short app_id, Common.Configurator config)
        {
            _app_id = app_id;
            Config = config;
        }

        internal static string SmtpServerPortSplitWithColon = Access.ParameterAccess.GetStringValue(Common.SettingItem.SMTPSERVER_PORT);
        internal static string SmtpExternalServerPortSplitWithColon = Access.ParameterAccess.GetStringValue(Common.SettingItem.SMTPEXTERNALSERVER_PORT);
        private static List<string> SmtpTargetInternalDomainList = Access.ParameterAccess.GetStringListValue(Common.SettingItem.SMTP_TARGETINTERNALDOMAINS);
        public StatusCode Send(string fromAddress, string fromDescription, string[] to_address, string[] cc_address, string subject, string body, string[] attachment)
        {
            try
            {
                if (string.IsNullOrEmpty(SmtpServerPortSplitWithColon))
                {
                    return new StatusCode(Common.CommonStatusCode.EMAIL_INVALID_SETTING, Config);
                }
                bool isExternal = false;
                foreach (string add in to_address)
                {
                    string mailDomain = add.Substring(add.IndexOf('@')+1);
                    if(SmtpTargetInternalDomainList!=null && SmtpTargetInternalDomainList.Count>0 
                        && SmtpTargetInternalDomainList.Contains(mailDomain)==false)
                    {
                        isExternal = true;
                        break;
                    }
                }
                if (cc_address != null)
                {
                    foreach (string add in cc_address)
                    {
                        string mailDomain = add.Substring(add.IndexOf('@')+1);
                        if (SmtpTargetInternalDomainList != null && SmtpTargetInternalDomainList.Count > 0
                        && SmtpTargetInternalDomainList.Contains(mailDomain) == false)
                        {
                            isExternal = true;
                            break;
                        }
                    }
                }
                string MailServerSetting = SmtpServerPortSplitWithColon;
                if (isExternal)
                {
                    if (String.IsNullOrEmpty(SmtpExternalServerPortSplitWithColon) == false)
                        MailServerSetting = SmtpExternalServerPortSplitWithColon;
                }
                string strServer = MailServerSetting.Split(':')[0];
                int port = Int32.Parse(MailServerSetting.Split(':')[1]);
                System.Net.Mail.SmtpClient mail = new System.Net.Mail.SmtpClient(strServer)
                {
                    Port = port,
                    Credentials = System.Net.CredentialCache.DefaultNetworkCredentials
                };

                using (MailMessage msg = new MailMessage())
                {
                    if (to_address != null && to_address.Length > 0)
                    {
                        foreach (string to in to_address)
                            msg.To.Add(to);
                    }
                    if (cc_address != null && cc_address.Length > 0)
                    {
                        foreach (string cc in cc_address)
                            msg.CC.Add(cc);
                    }
                    if (string.IsNullOrEmpty(fromDescription))
                        msg.From = new MailAddress(fromAddress);
                    else
                        msg.From = new MailAddress(fromAddress, fromDescription);

                    msg.Subject = subject;

                    msg.IsBodyHtml = true;
                    msg.Body = body;
                    if (attachment != null && attachment.Length > 0)
                    {
                        foreach (string filename in attachment)
                        {
                            if (string.IsNullOrEmpty(filename) == false)
                                msg.Attachments.Add(new Attachment(filename));
                        }
                    }
                    mail.Send(msg);
                }
                return StatusCode.SUCCEED_STATUS;
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.EMAIL_SEND_ERROR, ex, Config, _app_id);
            }
        }

    }
}
