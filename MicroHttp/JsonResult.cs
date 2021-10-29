namespace microhttp
{
    class JsonResult
    {
        private string _key;
        private object _value;

        public JsonResult () {}

        public object Value () => this._value;
        JsonResult (string key, object value) => (_key,_value) = (key,value);

        public override string ToString() => "{" + "\"" + _key + "\"" + ":" + "\"" + _value + "\"" + "}";
    }
}