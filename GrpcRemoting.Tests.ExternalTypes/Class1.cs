using System;
using System.Runtime.Serialization;

namespace GrpcRemoting.Tests.ExternalTypes
{
    [DataContract]
    [Serializable]
    public class DataClass
    {
        [DataMember]
        public int Value { get; set; }
    }
}