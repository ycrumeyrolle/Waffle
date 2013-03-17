namespace CommandProcessing.Interception
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Default <see cref="IProxyBuilder"/> instance for proxying an service.
    /// Only the virtual methods and propoerties can be intercepted and the claas must havez a paramterless constructor.
    /// A different implementation can be registered via the <see cref="ProcessorConfiguration.Services"/>.
    /// </summary>
    public class DefaultProxyBuilder : IProxyBuilder
    {
        private readonly Lazy<AssemblyBuilder> assemblyBuilder;
        
        private readonly Lazy<ModuleBuilder> moduleBuilder;

        private readonly ConcurrentDictionary<string, Type> typeCache = new ConcurrentDictionary<string, Type>();
        
        private readonly MethodInfo executingMethod;

        private readonly MethodInfo executedMethod;

        private readonly MethodInfo exceptionMethod;

        private FieldBuilder interceptorFieldBuilder;

        public DefaultProxyBuilder()
        {
            this.assemblyBuilder = new Lazy<AssemblyBuilder>(this.InitializeAssemblyBuilder);
            this.moduleBuilder = new Lazy<ModuleBuilder>(this.InitializeModuleBuilder);
            var interceptorType = typeof(IInterceptionProvider);
            this.executingMethod = interceptorType.GetMethod("OnExecuting", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            this.executedMethod = interceptorType.GetMethod("OnExecuted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            this.exceptionMethod = interceptorType.GetMethod("OnException", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Exception) }, null);
        }

        public T Build<T>(T source, IInterceptionProvider interceptorProvider) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Type type = typeof(T);

            string typeName = string.Format(CultureInfo.InvariantCulture, "{0}_proxy", type.Name);
            Type result;
            if (this.typeCache.Keys.Contains(typeName))
            {
                result = this.typeCache[typeName];
            }
            else
            {
                TypeBuilder typeBuilder = this.moduleBuilder.Value.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, source.GetType(), null);
                var innerFieldBuilder = this.EmitDefaultCtor(source.GetType(), typeBuilder);

                foreach (MethodInfo method in type.GetMethods().Where(m => m.IsVirtual))
                {
                    this.EmitMethod(innerFieldBuilder, method, typeBuilder);
                }

                result = typeBuilder.CreateType();
                this.typeCache.TryAdd(typeName, result);
            }

            object o = Activator.CreateInstance(result, new object[] { source, interceptorProvider });
            return (T)o;
        }

        private void EmitMethod(FieldBuilder innerFieldBuilder, MethodInfo method, TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, method.ReturnType, method.GetParameters().Select(pi => pi.ParameterType).ToArray());

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder returnResult = null;
            if (method.ReturnType != typeof(void))
            {
                returnResult = il.DeclareLocal(method.ReturnType);
            }

            LocalBuilder exception = il.DeclareLocal(typeof(Exception));
            Label endOfMethod = il.DefineLabel();

            // try block
            il.BeginExceptionBlock();

            // Call OnExecuting method
            this.EmitOnExecutingMethodCall(il);

            // Call method
            EmitInnerMethodCall(method, innerFieldBuilder, il);

            // Call OnExecuted method
            this.EmitOnExecutedMethodCall(il);
            il.Emit(OpCodes.Leave_S, endOfMethod);

            il.BeginCatchBlock(typeof(Exception));

            // Call OnException method
            il.Emit(OpCodes.Stloc_S, exception);
            this.EmitOnExceptionMethodCall(exception, il);

            // re-throw;
            il.Emit(OpCodes.Rethrow);

            il.EndExceptionBlock();

            il.MarkLabel(endOfMethod);
            if (returnResult != null)
            {
                // return returnValue;
                il.Emit(OpCodes.Ldloc_S, returnResult);
            }

            // done
            il.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

        private FieldBuilder EmitDefaultCtor(Type type, TypeBuilder typeBuilder)
        {
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Unable to create a proxy of the type {0}. It has no parameterless constructor.", type.Name));
            }

            FieldBuilder innerFieldBuilder = typeBuilder.DefineField("inner", type, FieldAttributes.Private);
            this.interceptorFieldBuilder = typeBuilder.DefineField("interceptor", typeof(IInterceptionProvider), FieldAttributes.Private);
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { type, typeof(IInterceptionProvider) });
            ILGenerator cIl = constructorBuilder.GetILGenerator();
            cIl.Emit(OpCodes.Ldarg_0); // Load this to stack   
            cIl.Emit(OpCodes.Call, ctor); // Call base (object) constructor      
            cIl.Emit(OpCodes.Ldarg_0); // Load this to stack            
            cIl.Emit(OpCodes.Ldarg_1); // Load the inner object to stack  
            cIl.Emit(OpCodes.Stfld, innerFieldBuilder); // Set proxy to the actual proxy  
            cIl.Emit(OpCodes.Ldarg_0); // Load this to stack            
            cIl.Emit(OpCodes.Ldarg_2); // Load the inner object to stack  
            cIl.Emit(OpCodes.Stfld, this.interceptorFieldBuilder); // Set interceptor to the actual interceptor   

            cIl.Emit(OpCodes.Ret);
            return innerFieldBuilder;
        }

        private void EmitOnExceptionMethodCall(LocalBuilder exception, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, this.interceptorFieldBuilder);
            il.Emit(OpCodes.Ldloc_S, exception);
            il.Emit(OpCodes.Callvirt, this.exceptionMethod);
        }

        private void EmitOnExecutedMethodCall(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, this.interceptorFieldBuilder);
            il.Emit(OpCodes.Callvirt, this.executedMethod);
        }

        private void EmitOnExecutingMethodCall(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, this.interceptorFieldBuilder);
            il.Emit(OpCodes.Callvirt, this.executingMethod);
        }

        private AssemblyBuilder InitializeAssemblyBuilder()
        {
            var currentAssemblyName = this.GetType().Assembly.GetName();
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(currentAssemblyName.Name + "_proxy"), AssemblyBuilderAccess.Run);
        }

        private ModuleBuilder InitializeModuleBuilder()
        {
            string assemblyName = this.assemblyBuilder.Value.GetName().Name;
            return this.assemblyBuilder.Value.DefineDynamicModule(assemblyName);
        }

        private static void EmitInnerMethodCall(MethodInfo method, FieldBuilder innerFieldBuilder, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, innerFieldBuilder);

            // Load method parameters
            int parametersCount = method.GetParameters().Length;
            for (int i = 0; i < parametersCount; i++)
            {
                il.Emit(OpCodes.Ldarg, i + 1);
            }

            il.Emit(OpCodes.Callvirt, method);
            if (method.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc_0);
            }
        }
    }
}