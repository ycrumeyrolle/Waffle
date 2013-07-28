// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Waffle.Dispatcher.DefaultHandlerTypeResolver.#GetHandlerTypes(Waffle.IAssembliesResolver)", Justification = "Any exception is volontary ignored.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Waffle.Filters.ExceptionFilterAttribute.#Waffle.Filters.IExceptionFilter.ExecuteExceptionFilterAsync(Waffle.Filters.HandlerExecutedContext,System.Threading.CancellationToken)", Justification = "Interface hidding is wanted.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Waffle.Filters.HandlerFilterAttribute.#Waffle.Filters.IHandlerFilter.ExecuteHandlerFilterAsync`1(Waffle.Filters.CommandHandlerContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<!!0>>)", Justification = "Interface hidding is wanted.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Services", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Validation.Validators", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Caching", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Eventing", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Validation.Providers", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Waffle.Dependencies", Justification = "Namespace follows folder structure")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Scope = "namespace", Justification = "Reviewed.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Scope = "member", Target = "Waffle.Events.IEventHandler`1.#Handle(!0,Waffle.Events.EventHandlerContext)", Justification = "Reviewed.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Scope = "member", Target = "Waffle.Events.IEventHandler.#Handle(Waffle.Events.IEvent,Waffle.Events.EventHandlerContext)", Justification = "Reviewed.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Scope = "member", Target = "Waffle.Events.IEventStore.#StoreAsync(Waffle.Events.IEvent,System.String,System.Threading.CancellationToken)", Justification = "Reviewed.")]
