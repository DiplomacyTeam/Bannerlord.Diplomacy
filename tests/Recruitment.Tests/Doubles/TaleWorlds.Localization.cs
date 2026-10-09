namespace TaleWorlds.Localization
{
    public sealed class TextObject
    {
        private string _text;
        public TextObject(string text) => _text = text;
        public static TextObject GetEmpty() => new("");
        public TextObject SetTextVariable(string name, object value)
        {
            _text = _text.Replace("{" + name + "}", value.ToString());
            return this;
        }
        public bool IsEmpty() => _text.Length == 0;
        public override string ToString() => _text;
    }
}
