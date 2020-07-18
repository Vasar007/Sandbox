using System;

namespace DataflowPipeline
{
    public sealed class IdentifierData<TKey>
    {
        public Guid Id { get; }

        public string Name { get; }

        public TKey Key { get; }


        private IdentifierData(Guid id, string name, TKey key)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Key = key;
        }

        public static IdentifierData<TKey> CreateKeyData(string name, TKey key)
        {
            return new IdentifierData<TKey>(Guid.NewGuid(), name, key);
        }
    }
}
