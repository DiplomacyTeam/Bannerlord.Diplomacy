namespace TaleWorlds.Library
{
    public static class MBMath
    {
        public static float ClampFloat(float value, float min, float max) => Math.Clamp(value, min, max);
    }
}
