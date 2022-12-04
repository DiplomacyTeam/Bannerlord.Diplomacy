using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;
using System.Reflection;

namespace Diplomacy
{
    internal static class Reflect
    {
        public class Method
        {
            public Type RequestedType { get; init; }

            public string Name { get; init; }

            public Type[]? Parameters { get; init; }

            public Type[]? Generics { get; init; }

            public MethodInfo MethodInfo { get; init; }

            public Type Type => MethodInfo is { DeclaringType: { } dt } ? dt : RequestedType;

            protected virtual string MethodType => "method";

            public virtual string PrettyName => $"{MethodType} {Name}{GenericsString}{ParametersString}";

            protected virtual MethodInfo? ResolveMethodInfo() => AccessTools.Method(RequestedType, Name, Parameters, Generics);

            public Method(Type type, string name, Type[]? parameters = null, Type[]? generics = null)
            {
                Name = name;
                Parameters = parameters;
                Generics = generics;
                RequestedType = type ?? throw new ArgumentNullException($"Null type given when reflecting {PrettyName}!", nameof(type));
                MethodInfo = ResolveMethodInfo() ?? throw new MissingMethodException($"Failed to find {PrettyName} in type {Type.FullName}!");
            }

            public TDelegate GetDelegate<TDelegate>(object instance) where TDelegate : Delegate
            {
                return instance is null
                    ? throw new ArgumentNullException(nameof(instance), $"{Type.Name} instance cannot be null when binding closed instance delegate to {PrettyName}!")
                    : AccessTools2.GetDelegate<TDelegate>(instance, MethodInfo) ?? throw new InvalidOperationException($"Failed to bind closed delegate to instance {PrettyName} of type {instance.GetType().FullName}!");
            }

            public TDelegate GetOpenDelegate<TDelegate>() where TDelegate : Delegate
            {
                return AccessTools2.GetDelegate<TDelegate>(MethodInfo) ?? throw new InvalidOperationException($"Failed to bind open delegate to {PrettyName} of type {Type.FullName}!");
            }

            public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
            {
                return AccessTools2.GetDelegate<TDelegate>(MethodInfo) ?? throw new InvalidOperationException($"Failed to bind closed delegate to {PrettyName} of type {Type.FullName}!");
            }

            protected string ParametersString => Parameters is null || Parameters.Length == 0
                ? string.Empty
                : $"({string.Join(", ", Parameters.Select(t => t.Name))})";

            protected string GenericsString => Generics is null || Generics.Length == 0
                ? string.Empty
                : $"<{string.Join(",", Generics.Select(t => t.Name))}>";
        }

        public class DeclaredMethod : Method
        {
            public DeclaredMethod(Type type, string name, Type[]? parameters = null, Type[]? generics = null)
                : base(type, name, parameters, generics) { }

            protected override MethodInfo? ResolveMethodInfo() => AccessTools.DeclaredMethod(Type, Name, Parameters, Generics);
        }

        public class Getter : Method
        {
            public Getter(Type type, string name) : base(type, name) { }
            protected override MethodInfo? ResolveMethodInfo() => AccessTools.PropertyGetter(Type, Name);
            protected override string MethodType => "property getter";
        }

        public class DeclaredGetter : Getter
        {
            public DeclaredGetter(Type type, string name) : base(type, name) { }
            protected override MethodInfo? ResolveMethodInfo() => AccessTools.DeclaredPropertyGetter(Type, Name);
        }

        public class Setter : Method
        {
            public Setter(Type type, string name) : base(type, name) { }
            protected override MethodInfo? ResolveMethodInfo() => AccessTools.PropertySetter(Type, Name);
            protected override string MethodType => "property setter";
        }

        public class DeclaredSetter : Setter
        {
            public DeclaredSetter(Type type, string name) : base(type, name) { }
            protected override MethodInfo? ResolveMethodInfo() => AccessTools.DeclaredPropertySetter(Type, Name);
        }

        public sealed class Method<T> : Method
        {
            public Method(string name, Type[]? parameters = null, Type[]? generics = null)
                : base(typeof(T), name, parameters, generics) { }
        }

        public sealed class DeclaredMethod<T> : DeclaredMethod
        {
            public DeclaredMethod(string name, Type[]? parameters = null, Type[]? generics = null)
                : base(typeof(T), name, parameters, generics) { }
        }

        public sealed class Getter<T> : Getter
        {
            public Getter(string name) : base(typeof(T), name) { }
        }

        public sealed class DeclaredGetter<T> : DeclaredGetter
        {
            public DeclaredGetter(string name) : base(typeof(T), name) { }
        }

        public sealed class Setter<T> : Setter
        {
            public Setter(string name) : base(typeof(T), name) { }
        }

        public sealed class DeclaredSetter<T> : DeclaredSetter
        {
            public DeclaredSetter(string name) : base(typeof(T), name) { }
        }
    }
}