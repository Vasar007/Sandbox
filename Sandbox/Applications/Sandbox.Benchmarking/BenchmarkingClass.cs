using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarking
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class BenchmarkingClass
    {
        public sealed class TestClass
        {
            public string MyProperty { get; } = "Default Value";

            public TestClass()
            {
            }
        }

        private TestClass _instance;

        private Action<TestClass, string>? _backingFieldReflectionSetter;

        private Action<TestClass, string>? _backingFieldILSetter;

        public BenchmarkingClass()
        {
            _instance = new TestClass();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _instance = new TestClass();
            
            _backingFieldReflectionSetter = CreateReadOnlyPropertySetterFullyReflection();
            _backingFieldILSetter = CreateReadOnlyPropertySetternWithIL();
        }

        [Benchmark]
        public Action<TestClass, string> CreateReadOnlyPropertySetterFullyReflection()
        {
            (FieldInfo backingField, _) = GetReflectionInfo();

            return (instance, value) => backingField.SetValue(instance, value);
        }

        [Benchmark]
        public void SetReadOnlyPropertyFullyReflection()
        {
            _backingFieldReflectionSetter!(_instance, "New value 1");
        }

        [Benchmark]
        public Action<TestClass, string> CreateReadOnlyPropertySetternWithIL()
        {
            (FieldInfo backingField, PropertyInfo? propertyInfo) = GetReflectionInfo();

            var method = new DynamicMethod(
              name: $"Set_{backingField.Name}",
              returnType: null,
              parameterTypes: new[] { propertyInfo?.DeclaringType!, propertyInfo?.PropertyType! },
              restrictedSkipVisibility: true
            );

            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, backingField);
            gen.Emit(OpCodes.Ret);

            return (Action<TestClass, string>) method.CreateDelegate(typeof(Action<TestClass, string>));
        }

        [Benchmark]
        public void SetReadOnlyPropertyReflectionWithIL()
        {
            _backingFieldILSetter!(_instance, "New value 2");
        }

        private static (FieldInfo backingField, PropertyInfo? propertyInfo) GetReflectionInfo()
        {
            var type = typeof(TestClass);
            var propertyInfo = type.GetProperty(nameof(TestClass.MyProperty));

            var backingField = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(field =>
                    field.Attributes.HasFlag(FieldAttributes.Private) &&
                    field.Attributes.HasFlag(FieldAttributes.InitOnly) &&
                    field.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute)) &&
                    (field.DeclaringType == propertyInfo?.DeclaringType) &&
                    field.FieldType.IsAssignableFrom(propertyInfo?.PropertyType) &&
                    field.Name.StartsWith("<" + propertyInfo.Name + ">", StringComparison.Ordinal) // Dangerous code. Name of backing field is internal detail of .NET.
                );

            if (backingField is null)
            {
                throw new InvalidOperationException(
                    $"Failed to find backing field for property '{propertyInfo?.Name}'."
                );
            }

            return (backingField, propertyInfo);
        }
    }
}
