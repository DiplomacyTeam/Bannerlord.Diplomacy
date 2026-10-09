namespace TaleWorlds.Localization
{
    public sealed class TextObject
    {
        private string _text;
        // Like the game's TextObject, the "{=id}" localization prefix isn't part of the displayed text.
        public TextObject(string text) => _text = text.StartsWith("{=") && text.IndexOf('}') is var end && end > 0 ? text[(end + 1)..] : text;
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
