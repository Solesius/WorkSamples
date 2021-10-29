using System;

namespace microhttp {
    class ADGroupMember 
    {
        public string DisplayName {get; set;}
        public string UserName {get;set;}
        public string Location {get; set;}
        public string OrganizationalUnit {get; set;}

        public override bool Equals(object that)
        {
           return this.UserName.Equals( ((ADGroupMember)that).UserName );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayName, UserName, Location, OrganizationalUnit);
        }
    }
}