using System;
using System.Json;
using System.Runtime.Serialization;

namespace Client.Types
{
    /// <summary>
    /// Contact with basic fileds for creation
    /// </summary>
    [DataContract]
    public class ContactBase
    {
        public readonly static string URI = "contacts";

        [DataMember(Name = "firstname", EmitDefaultValue = false)]
        public string Firstname { get; set; }

        [DataMember(Name = "lastname", EmitDefaultValue = false)]
        public string Lastname { get; set; }

        [DataMember(Name = "emailaddress1", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "new_username", EmitDefaultValue = false)]
        public string Username { get; set; }

        [DataMember(Name = "department", EmitDefaultValue = false)]
        public string Department { get; set; }

        /// <summary>
        /// Create a Contact with basic information of current instance
        /// </summary>
        /// <param name="accountID">ID represents Account the new Contact associate to, optional</param>
        /// <returns>JsonObject</returns>
        public JsonObject Create(string accountID=null)
        {
            JsonObject contactObj = new JsonObject
            {
                ["firstname"] = Firstname,
                ["lastname"] = Lastname,
                ["emailaddress1"] = Email,
                ["new_username"] = Username,
                ["department"] = Department
            };
            if (!string.IsNullOrEmpty(accountID))
            {
                contactObj["parentcustomerid_account@odata.bind"] = string.Format("/accounts({0})", accountID);
            }
            return contactObj;
        }

        /// <summary>
        /// Create a Contact and attach it to an Account with information given from caller
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="department"></param>
        /// <param name="accountID"></param>
        /// <returns></returns>
        public static JsonObject Create(string firstName, string lastName, string email, string username, string department, string accountID)
        {
            return new JsonObject
            {
                ["firstname"] = firstName,
                ["lastname"] = lastName,
                ["emailaddress1"] = email,
                ["new_username"] = username,
                ["department"] = department,
                ["parentcustomerid_account@odata.bind"] = string.Format("/accounts({0})", accountID)
            };
        }
    }

    [DataContract]
    public class Contact: ContactBase
    {
        [DataMember(Name = "contactid")]
        public string ID { get; set; }

        [DataMember(Name = "jobtitle", EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "new_registrationdetails", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "telephone1", EmitDefaultValue = false)]
        public string Phone { get; set; }


        [DataMember(Name = "createdon", EmitDefaultValue = false)]
        private string CreatedOnString { get; set; }

        [DataMember(Name = "statuscode", EmitDefaultValue = false)]
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
    }
}
