using System;
using System.Json;
using System.Runtime.Serialization;

namespace Synchroniser.Models
{
    [DataContract]
    public class Contact
    {
        public readonly static string URI = "contacts";

        [DataMember(Name = "emailaddress1")]
        public string Email { get; set; }

        [DataMember(Name = "contactid")]
        public string Id { get; set; }

        [DataMember(Name = "jobtitle")]
        public string Title { get; set; }

        [DataMember(Name = "new_username")]
        public string Username { get; set; }

        [DataMember(Name = "new_registrationdetails")]
        public string Description { get; set; }

        [DataMember(Name = "department")]
        public string Department { get; set; }

        [DataMember(Name = "telephone1")]
        public string Phone { get; set; }

        [DataMember(Name = "firstname")]
        public string Firstname { get; set; }

        [DataMember(Name = "lastname")]
        public string Lastname { get; set; }

        [DataMember(Name = "createdon")]
        private string CreatedOnString { get; set; }

        [DataMember(Name = "statuscode")]
        public int Status { get; private set; }

        [IgnoreDataMember]
        public DateTime CreatedOn
        {
            get
            {
                if (string.IsNullOrEmpty(CreatedOnString))
                {
                    return DateTime.Now;
                }
                return DateTime.ParseExact(CreatedOnString, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Default construct for property binding
        /// </summary>
        public Contact() { }

        /// <summary>
        /// Temporary: This is just for testing purpose 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="department"></param>
        public Contact(string id, string department)
        {
            Id = id;
            Department = department;
        }

        /// <summary>
        /// To do a quick check by email in CRM to see if a Contact exists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string GetCheckQuery(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("email for checking a Contact has not been provided");
            return string.Format("contacts(emailaddress1='{0}')?$select=contactid", email);
        }

        /// <summary>
        /// Get a Contact by its contactid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Get(Guid id)
        {
            return $"contacts({id})";
        }
    }

    /// <summary>
    /// A simple version of Contact for creation
    /// </summary>
    public static class SimpleContact
    {
        public static string Create(string fullName, string email)
        {
            return new JsonObject {
                ["fullname"] = fullName,
                ["emailaddress1"] = email
            }.ToString();
        }
    }
}
