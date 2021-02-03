﻿using System;
using System.Reflection;
using Xunit;

namespace Namotion.Reflection.Tests
{
#nullable enable
    public class GenericsTests
    {
        public class UnconstrainedGenericClass<T>
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }
        public class NotNullGenericClass<T> where T : notnull
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }
        public class NullableGenericClass<T> where T : class?
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }

        public class StructGenericClass<T> where T : struct
        {
            public T Prop { get; set; }

            public T Field = default!;

            public T DoStuff(T original) => original;
        }

        [Fact]
        public void OpenGenericsType()
        {
            void DoTest(Type t, Nullability expectedNullability)
            {
                Assert.Equal(expectedNullability, t.GetTypeInfo().GenericTypeParameters[0].ToContextualType().Nullability);
                Assert.Equal(expectedNullability, t.GetProperty("Prop")!.ToContextualProperty().Nullability);
                
                var method = t.GetMethod("DoStuff")!;
                Assert.Equal(expectedNullability, method.ReturnParameter.ToContextualParameter().Nullability);
                Assert.Equal(expectedNullability, method.GetParameters()[0].ToContextualParameter().Nullability);
            }

            DoTest(typeof(UnconstrainedGenericClass<>), Nullability.Nullable);
            DoTest(typeof(NotNullGenericClass<>), Nullability.NotNullable);
            DoTest(typeof(NullableGenericClass<>), Nullability.Nullable);
            DoTest(typeof(StructGenericClass<>), Nullability.NotNullable);
        }

        public class ClosedGenericsClass
        {
            public UnconstrainedGenericClass<string> NotNull1 { get; set; }
            public NotNullGenericClass<string> NotNull2 { get; set; }
            public NullableGenericClass<string> NotNull3 { get; set; }

            public StructGenericClass<int> Struct { get; set; }


            public UnconstrainedGenericClass<string?> Nullable1 { get; set; }
            public NullableGenericClass<string?> Nullable2 { get; set; }
        }

        [Fact]
        public void ClosedGenericsType()
        {
            void DoTest(string propertyName, Nullability expectedNullability)
            {
                var property = typeof(ClosedGenericsClass).GetProperty(propertyName)!.ToContextualProperty();

                Assert.Equal(expectedNullability, property.PropertyType.GenericArguments[0].Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetProperty("Prop")!.Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetField("Field")!.Nullability);
            }

            DoTest(nameof(ClosedGenericsClass.NotNull1), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull2), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull3), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.Struct), Nullability.NotNullable);

            DoTest(nameof(ClosedGenericsClass.Nullable1), Nullability.Nullable);
            DoTest(nameof(ClosedGenericsClass.Nullable2), Nullability.Nullable);
        }

        public class GenericClass<T>
        {
            public T GetT()
            {
                return default!;
            }

            public void SetT(T value)
            {
            }

            public T Prop { get; set; } = default!;

            public T Field = default!;
        }

        public class GenericClassContainer
        {
            public GenericClass<string?> Nullable = new GenericClass<string?>();
            public GenericClass<string> NonNullable = new GenericClass<string>();
        }

        [Fact]
        public void GenericClassResolutionTest()
        {
            var nullableField = typeof(GenericClassContainer).GetField(nameof(GenericClassContainer.Nullable))!.ToContextualField();
            var nonNullableField = typeof(GenericClassContainer).GetField(nameof(GenericClassContainer.NonNullable))!.ToContextualField();

            Assert.Equal(Nullability.NotNullable, nonNullableField.FieldType.GetProperty("Prop")!.Nullability);
            Assert.Equal(Nullability.Nullable, nullableField.FieldType.GetProperty("Prop")!.Nullability);

            Assert.Equal(Nullability.NotNullable, nonNullableField.FieldType.GetField("Field")!.Nullability);
            Assert.Equal(Nullability.Nullable, nullableField.FieldType.GetField("Field")!.Nullability);
        }
    }
}