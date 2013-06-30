// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "CommandProcessing.Dispatcher.DefaultHandlerTypeResolver.#GetHandlerTypes(CommandProcessing.IAssembliesResolver)", Justification = "Any exception is volontary ignored.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "CommandProcessing.Filters.ExceptionFilterAttribute.#CommandProcessing.Filters.IExceptionFilter.ExecuteExceptionFilterAsync(CommandProcessing.Filters.HandlerExecutedContext,System.Threading.CancellationToken)", Justification = "Interface hidding is wanted.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "CommandProcessing.Filters.HandlerFilterAttribute.#CommandProcessing.Filters.IHandlerFilter.ExecuteHandlerFilterAsync`1(CommandProcessing.Filters.HandlerContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<!!0>>)", Justification = "Interface hidding is wanted.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Services", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Validation.Validators", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Caching", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Eventing", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Validation.Providers", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "CommandProcessing.Dependencies", Justification = "Namespace follows folder structure")]
