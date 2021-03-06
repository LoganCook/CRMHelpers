﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    /// <summary>
    /// To unwrap a dynamics search return which value key can have 0 to many objects:
    /// {
    ///   "@odata.context":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/$metadata#salesorders",
    ///    "value":[ ... ]
    ///  }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class OData<T>
    {
        [DataMember(Name = "value")]
        public List<T> Value { get; set; }
    }
}
