namespace ProjectTools.Editor.AssetTools
{
    public sealed class AssetEntry
    {
        public string Path { get; }
        public long Bytes { get; }
        public string ImportSettings { get; }

        public AssetEntry(string path, long bytes, string importSettings)
        {
            Path = path;
            Bytes = bytes;
            ImportSettings = importSettings;
        }
    }
}
