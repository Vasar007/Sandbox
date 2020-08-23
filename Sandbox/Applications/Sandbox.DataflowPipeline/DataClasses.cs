using System;

namespace Sandbox.DataflowPipeline
{
    public interface IData
    {
    }

    public sealed class Data1 : IData
    {
        public int Value { get; }


        public Data1(int value)
        {
            Value = value;
        }
    }

    public sealed class Data2 : IData
    {
        public string Value { get; }


        public Data2(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public sealed class Data3 : IData
    {
        public double Value { get; }


        public Data3(double value)
        {
            Value = value;
        }
    }
}
