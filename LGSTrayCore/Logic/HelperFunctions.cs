namespace LGSTrayCore.Logic
{
    internal static class HelperFunctions
    {
        public static byte[] LoadEmbeddedIcon(string iconName)
        {
            using (Stream? s = typeof(HelperFunctions).Assembly.GetManifestResourceStream($"{typeof(HelperFunctions).Assembly.GetName().Name}.Ressources.{iconName}"))
            {
                if (s == null)
                {
                    return [];
                }

                using (MemoryStream ms = new())
                {
                    s.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
