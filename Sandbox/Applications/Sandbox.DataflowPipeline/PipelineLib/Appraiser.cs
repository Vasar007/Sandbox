using System;
using System.Collections.Generic;

namespace DataflowPipeline
{
    public sealed class Appraiser
    {
        public Func<IReadOnlyList<IData>, IReadOnlyList<string>> Func { get; }

        public Type DataType { get; }


        public Appraiser(Func<IReadOnlyList<IData>, IReadOnlyList<string>> func, Type dataType)
        {
            Func = func ?? throw new ArgumentNullException(nameof(func));
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        }
    }
}
