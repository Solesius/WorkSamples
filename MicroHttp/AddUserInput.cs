using System.Text.Json;

namespace microhttp
{
    class AddUserInput
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }

        public AddUserInput(
            string Name,
            string GroupName
        ) => (this.UserName, this.GroupName) = (Name, GroupName) ;

        public override string ToString()
        {
            return JsonSerializer.Serialize<AddUserInput>(this);
        }
    }
}