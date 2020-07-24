namespace UAssetGUI
{
    public static class Utils
    {
        public static T TryGetElement<T>(this T[] array, int index)
        {
            if (array != null && index < array.Length)
            {
                return array[index];
            }
            return default(T);
        }
    }
}
