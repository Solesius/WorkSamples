using System.Text.Json;

namespace microhttp
{
    class QueryInput
    {
        public string Name { get; set; }
        public string ObjectClass { get; set; }

        public QueryInput(
            string Name,
            string ObjectClass
        ) => (this.Name, this.ObjectClass) = (Name, ObjectClass) ;

        public override string ToString()
        {
            return JsonSerializer.Serialize<QueryInput>(this);
        }
    }
}