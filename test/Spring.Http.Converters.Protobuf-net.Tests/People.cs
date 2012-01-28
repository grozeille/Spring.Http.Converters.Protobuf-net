using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Spring.Http.Converters.ProtobufNet
{
    [ProtoContract]
    public class People
    {
        [ProtoMember(1, IsRequired = true)]
        public int Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string Firstname { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string Lastname { get; set; }

        public override bool Equals(object obj)
        {
            People other = obj as People;

            return other != null 
                && Object.Equals(other.Id, this.Id) 
                && Object.Equals(other.Firstname, this.Firstname) 
                && Object.Equals(other.Lastname, this.Lastname);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Id.GetHashCode();
            hash = hash * 23 + (Firstname == null ? 0 : Firstname.GetHashCode());
            hash = hash * 23 + (Lastname == null ? 0 : Lastname.GetHashCode());
            return hash;
        }
    }
}
