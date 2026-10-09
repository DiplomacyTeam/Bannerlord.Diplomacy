namespace TaleWorlds.Library
{
    public static class MBMath
    {
        public static float ClampFloat(float value, float min, float max) => Math.Clamp(value, min, max);
    }

    public static class InformationManager
    {
        public static List<InformationMessage> Messages { get; } = new();
        public static void DisplayMessage(InformationMessage message) => Messages.Add(message);
    }

    public sealed class InformationMessage
    {
        public InformationMessage(string information) => Information = information;
        public string Information { get; }
    }
}
