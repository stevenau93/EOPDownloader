using System;
using System.Collections.Generic;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.MIME;
using Limilabs.Mail.Headers;
using System.Threading;
using System.IO;

namespace EPODownloader
{
    class Program
    {
        static string Username= "stevenau1993";
        static string Password= "Passnhutren93";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Program is running.");
                Console.WriteLine("===================");
                Process();
            }
            catch
            {
                while (true) {
                    using (Imap imap = new Imap())
                    {
                        try
                        {
                            imap.Connect("imap.gmail.com", 993, true);
                            imap.UseBestLogin(Username, Password);
                            Console.WriteLine("Reconnected.");
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Internet problem occured.Please wait 10s to reconnected");
                            Thread.Sleep(5000);
                        }
                    }
                }
                Process();
            }

        }

        public static void GetDataFromMail()
        {
            string dataDir = GetDataDir_Emails();    
            string dataDirAtt = GetDataDir_Attachments();

            using (Imap imap = new Imap())
            {
                EmailItem item = new EmailItem();

                imap.Connect("imap.gmail.com", 993, true);
                imap.UseBestLogin(Username, Password);
                imap.SelectInbox();
                List<long> uids =  imap.Search(Flag.Unseen);
                int count = 0;
                if (uids.Count > 0)
                {
                    imap.MarkMessageSeenByUID(uids);                   
                    foreach (long uid in uids)
                    {
                            IMail email = new MailBuilder().CreateFromEml(imap.GetMessageByUID(uid));              
                            var eml = imap.GetMessageByUID(uid);
                            item.Subject = email.Subject;
                            item.MessageID = email.MessageID;
                            item.Date = Convert.ToDateTime(email.Date);
                            item.Content = email.GetBodyAsHtml();
                            item.Path = dataDir + email.MessageID + ".eml";

                            string filename = string.Format(dataDir + item.MessageID + ".eml", uid);
                            File.WriteAllBytes(filename, eml);
                            foreach (MailAddress cc in email.Cc)
                            {
                                foreach (MailBox mailbox in cc.GetMailboxes())
                                {
                                    item.Cc.Add(mailbox.Address);
                                }
                            }

                            int emailID = Email.InsertEmail(item);

                            if (emailID > 0)
                            {
                                foreach (MimeData SaveAttachment in email.Attachments)
                                {
                                    AttachmentItem AttachmentItem = new AttachmentItem();

                                    Guid g = Guid.NewGuid();
                                    while (Email.GUIDIsExist(g.ToString()))
                                    {
                                        g = Guid.NewGuid();
                                    }
                                    AttachmentItem.GUID = Convert.ToString(g);
                                    AttachmentItem.EmailID = emailID;
                                    AttachmentItem.AttachmentName = SaveAttachment.FileName;
                                    AttachmentItem.ContentType = Convert.ToString(SaveAttachment.ContentType).ToLower();
                                    AttachmentItem.Size = Convert.ToInt64(SaveAttachment.Size);
                                    AttachmentItem.Path = dataDirAtt + AttachmentItem.GUID + "-" + SaveAttachment.SafeFileName;

                                    
                                    if (Email.InsertAttachment(AttachmentItem))
                                    {

                                        SaveAttachment.Save(dataDirAtt + AttachmentItem.GUID + "-" + SaveAttachment.SafeFileName);
                                       
                                    }
                                    else
                                    {
                                        Console.WriteLine("Fail to download attachment.");
                                    }
                                }
                                count++;
                                Console.WriteLine(count + " - Inserted email " + item.Subject + " - " + item.Date);
                            }
                        }
                    
                    Console.WriteLine("--Download " + count + " emails completed--");
                    imap.Close();
                }
            }
        }

        public static void Process()
        {        
            while (true)
            {
                GetDataFromMail();
                Thread.Sleep(5000);
            }
        }



        internal static string GetDataDir_Emails()
        {
            return Path.GetFullPath("../../Emails/");
        }
        internal static string GetDataDir_Attachments()
        {
            return Path.GetFullPath("../../Emails/Attachments/");
        }
    }
}

