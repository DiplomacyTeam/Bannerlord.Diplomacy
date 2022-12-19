using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Diplomacy.PatchTools
{
    /// <summary>
    /// Non-generic abstract root class of the PatchClass hierarchy. Do not inherit directly from this class.
    /// </summary>
    internal abstract class PatchClass
    {
        public Patch[] Patches => _patches ??= Prepare().ToArray();

        private Patch[]? _patches;

        /// <summary>
        /// Invoked only once for a patch class in order to define the active patches in it before applying them.
        /// </summary>
        /// <returns>A sequence of patch definitions that are ready to be applied.</returns>
        protected abstract IEnumerable<Patch> Prepare();

        public abstract class Patch
        {
            private readonly HarmonyPatchType _type;
            private readonly Reflect.Method _patchMethod;
            private readonly MethodBase _targetMethod;
            private readonly int _priority;

            protected Patch(HarmonyPatchType type,
                Reflect.Method patchMethod,
                Reflect.Method targetMethod,
                int priority) : this(type, patchMethod, targetMethod.MethodInfo, priority)
            {
            }

            protected Patch(
                HarmonyPatchType type,
                Reflect.Method patchMethod,
                MethodBase targetMethod,
                int priority)
            {
                _type = type;
                _patchMethod = patchMethod;
                _targetMethod = targetMethod;
                _priority = priority;
            }

            public void Apply(Harmony harmony)
            {
                var harmonyMethod = new HarmonyMethod(_patchMethod.MethodInfo, _priority);

                var newMethod = harmony.Patch(_targetMethod,
                                              _type == HarmonyPatchType.Prefix ? harmonyMethod : null,
                                              _type == HarmonyPatchType.Postfix ? harmonyMethod : null,
                                              _type == HarmonyPatchType.Transpiler ? harmonyMethod : null,
                                              _type == HarmonyPatchType.Finalizer ? harmonyMethod : null);

                if (newMethod is null)
                    throw new PatchCreationException(this);
            }

            public void Remove(Harmony harmony) => harmony.Unpatch(_targetMethod, _patchMethod.MethodInfo);

            public override string ToString() => $"{Enum.GetName(typeof(HarmonyPatchType), _type)} patch of "
                                               + $"{_targetMethod.Name} in type {_targetMethod.DeclaringType.Name} ({_targetMethod.DeclaringType.FullName})";

            private sealed class PatchCreationException : Exception
            {
                public PatchCreationException(Patch patch) : base($"Harmony: Failed to create patch: {patch}") { }
            }
        }
    }

    /// <summary>
    /// Superclass of a Harmony patch class <typeparamref name="TPatch"/>.
    /// </summary>
    /// <remarks>
    /// Instead of inheriting from this, inherit your patch class from <see cref="PatchClass{TPatch, TTarget}"/>
    /// when the patched target type is known, isn't <see langword="static""/>, and the patches in
    /// <typeparamref name="TPatch"/> all target that same type. The patch class must have a default constructor.
    /// </remarks>
    /// <typeparam name="TPatch">Patch class with patch method(s), which inherits from this.</typeparam>
    internal abstract class PatchClass<TPatch> : PatchClass where TPatch : PatchClass<TPatch>, new()
    {
        protected sealed class Prefix : Patch
        {
            public Prefix(string patchMethodName, Reflect.Method targetMethod, int priority = -1)
                : base(HarmonyPatchType.Prefix, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Prefix(string patchMethodName, Type targetType, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Prefix, new(typeof(TPatch), patchMethodName), new Reflect.Method(targetType, targetMethodName), priority) { }
        }

        protected sealed class Postfix : Patch
        {
            public Postfix(string patchMethodName, Reflect.Method targetMethod, int priority = -1)
                : base(HarmonyPatchType.Postfix, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Postfix(string patchMethodName, Type targetType, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Postfix, new(typeof(TPatch), patchMethodName), new Reflect.Method(targetType, targetMethodName), priority) { }
        }

        protected sealed class Transpiler : Patch
        {
            public Transpiler(string patchMethodName, Reflect.Method targetMethod, int priority = -1)
                : base(HarmonyPatchType.Transpiler, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Transpiler(string patchMethodName, Type targetType, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Transpiler, new(typeof(TPatch), patchMethodName), new Reflect.Method(targetType, targetMethodName), priority) { }
        }

        protected sealed class Finalizer : Patch
        {
            public Finalizer(string patchMethodName, Reflect.Method targetMethod, int priority = -1)
                : base(HarmonyPatchType.Finalizer, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Finalizer(string patchMethodName, Type targetType, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Finalizer, new(typeof(TPatch), patchMethodName), new Reflect.Method(targetType, targetMethodName), priority) { }
        }
    }

    /// <summary>
    /// Superclass of a Harmony patch class <typeparamref name="TPatch"/> that's targeting methods
    /// in the type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <remarks>
    /// For cases where <typeparamref name="TTarget"/> is not known at compile-time, is <see langword="static""/>,
    /// or <typeparamref name="TPatch"/>'s patch methods target different types, then instead inherit your patch class
    /// from this class's superclass, <see cref="PatchClass{TPatch}"/>. The patch class must have a default constructor.
    /// </remarks>
    /// <typeparam name="TPatch">Patch class with patch method(s), which inherits from this.</typeparam>
    /// <typeparam name="TTarget">Type with the method(s) targeted for patching.</typeparam>
    internal abstract class PatchClass<TPatch, TTarget> : PatchClass<TPatch> where TPatch : PatchClass<TPatch, TTarget>, new()
    {
        protected new sealed class Prefix : Patch
        {
            public Prefix(string patchMethodName, Reflect.Method<TTarget> targetMethod, int priority = -1)
                : base(HarmonyPatchType.Prefix, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Prefix(string patchMethodName, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Prefix, new(typeof(TPatch), patchMethodName), new Reflect.Method(typeof(TTarget), targetMethodName), priority) { }
        }

        protected new sealed class Postfix : Patch
        {
            public Postfix(string patchMethodName, Reflect.Method<TTarget> targetMethod, int priority = -1)
                : base(HarmonyPatchType.Postfix, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Postfix(string patchMethodName, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Postfix, new(typeof(TPatch), patchMethodName), new Reflect.Method(typeof(TTarget), targetMethodName), priority) { }

            public Postfix(string patchMethodName, MethodBase targetMethod, int priority = -1)
                : base(HarmonyPatchType.Postfix, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }
        }

        protected new sealed class Transpiler : Patch
        {
            public Transpiler(string patchMethodName, Reflect.Method<TTarget> targetMethod, int priority = -1)
                : base(HarmonyPatchType.Transpiler, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Transpiler(string patchMethodName, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Transpiler, new(typeof(TPatch), patchMethodName), new Reflect.Method(typeof(TTarget), targetMethodName), priority) { }
        }

        protected new sealed class Finalizer : Patch
        {
            public Finalizer(string patchMethodName, Reflect.Method<TTarget> targetMethod, int priority = -1)
                : base(HarmonyPatchType.Finalizer, new(typeof(TPatch), patchMethodName), targetMethod, priority) { }

            public Finalizer(string patchMethodName, string targetMethodName, int priority = -1)
                : base(HarmonyPatchType.Finalizer, new(typeof(TPatch), patchMethodName), new Reflect.Method(typeof(TTarget), targetMethodName), priority) { }
        }
    }
}