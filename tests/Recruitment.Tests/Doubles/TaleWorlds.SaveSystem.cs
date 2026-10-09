namespace TaleWorlds.SaveSystem
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SaveableFieldAttribute : Attribute
    {
        public SaveableFieldAttribute(int id) { }
    }
}
