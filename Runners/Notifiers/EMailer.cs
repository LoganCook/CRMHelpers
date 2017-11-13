using System;
using System.Net.Mail;
using System.Collections.Generic;

namespace Runners.Notifiers
{
    public interface INotifier
    {
        /// <summary>
        /// Send a content to a list of targets which can be email addresses or urls
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        void Send(List<string> targets, string subject, string content);
    }

    public struct Sender
    {
        public string Name, Address, Password;

        public Sender(string displayName, string emailAddress, string password)
        {
            Name = displayName;
            Address = emailAddress;
            Password = password;
        }
    }

    class EMailer : INotifier
    {
        SmtpClient client;
        MailAddress from;

        /// <summary>
        /// Send by email. May allow more than one receiver to receive emails.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="receiver"></param>
        public EMailer(string host, int port, Sender sender)
        {
            client = new SmtpClient(host)
            {
                EnableSsl = true,
                Port = port,
                Credentials = new System.Net.NetworkCredential(sender.Address, sender.Password)
            };

            from = new MailAddress(sender.Address, sender.Name);
        }

        /// <summary>
        /// Send content to a list of receivers
        /// </summary>
        /// <param name="receivers"></param>
        /// <param name="content"></param>
        public void Send(List<string> receivers, string subject, string content)
        {
            MailAddress to = new MailAddress(receivers[0]);
            MailMessage message = new MailMessage(from, to)
            {
                Body = content,
                BodyEncoding = System.Text.Encoding.UTF8,
                Subject = subject
            };

            if (receivers.Count > 1)
            {
                for (int i = 1; i < receivers.Count; i++)
                {
                    message.CC.Add(new MailAddress(receivers[i]));
                }
            }

            client.Send(message);
            message.Dispose();
        }
    }
}
