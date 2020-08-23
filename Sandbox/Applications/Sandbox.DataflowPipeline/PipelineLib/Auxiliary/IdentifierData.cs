using System;

namespace DataflowPipeline
{
    public sealed class IdentifierData
    {
        public Guid Id { get; }

        public string Name { get; }


        private IdentifierData(Guid id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static IdentifierData CreateCommonData(string name)
        {
            return new IdentifierData(Guid.NewGuid(), name);
        }
    }
}
