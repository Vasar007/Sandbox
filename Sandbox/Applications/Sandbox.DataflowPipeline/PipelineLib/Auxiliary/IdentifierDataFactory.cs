namespace DataflowPipeline
{
    public static class IdentifierDataFactory
    {
        public static IdentifierData CreateCommonData(string name)
        {
            return IdentifierData.CreateCommonData(name);
        }

        public static IdentifierData<TKey> CreateKeyData<TKey>(string name, TKey key)
        {
            return IdentifierData<TKey>.CreateKeyData(name, key);
        }
    }
}
