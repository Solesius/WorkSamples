using System;
namespace microhttp
{
    class ADUser 
    {
        public string First {get;set;}
        public string Last {get;set;}
        public string UserName {get;set;}
        public string Description {get;set;}
        public string Mail {get;set;}
        public string Office {get;set;}
        public string Phone {get;set;}
        public Boolean Enabled {get;set;}
        public Boolean Locked {get;set;}
        public Boolean Expired {get;set;}
        public string Created {get;set;}
        public string PwdLastSet {get;set;}
        public string LastLogon {get;set;}
        public string PwdExpiration {get;set;}

        
        public override string ToString() 
        {
            return "UserName: " + UserName + "\n" +
                "DisplayName: " + First +" "+ Last + "\n" +
                "Description: " + Description + "\n" +
                "Mail: " + Mail + "\n" +
                "Office: " + Office + "\n" +
                "Phone: " + Phone + "\n" +
                "Enabled: " + Enabled.ToString() + "\n" +
                "Locked: " + Locked.ToString() + "\n" +
                "Expired: " + Expired + "\n" +
                "Created: " + Created + "\n" +
                "PasswordLastSet: " + PwdLastSet + "\n" +
                "LastLogon: " + LastLogon + "\n" +
                "PasswordExpiresOn: " + PwdExpiration;
        }
    }
}