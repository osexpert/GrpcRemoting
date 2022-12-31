using System.Runtime.Serialization;

namespace GrpcRemoting.Tests.ExternalTypes
{
    [DataContract]
    public class DataClass
    {
        [DataMember]
        public int Value { get; set; }
    }
}