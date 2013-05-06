﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CommandProcessing {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CommandProcessing.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value must be greater than or equal to {0}..
        /// </summary>
        internal static string ArgumentMustBeGreaterThanOrEqualTo {
            get {
                return ResourceManager.GetString("ArgumentMustBeGreaterThanOrEqualTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value must be less than or equal to {0}..
        /// </summary>
        internal static string ArgumentMustBeLessThanOrEqualTo {
            get {
                return ResourceManager.GetString("ArgumentMustBeLessThanOrEqualTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument &apos;{0}&apos; is null or empty..
        /// </summary>
        internal static string ArgumentNullOrEmpty {
            get {
                return ResourceManager.GetString("ArgumentNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Category=&apos;{0}&apos;.
        /// </summary>
        internal static string CategoryFormat {
            get {
                return ResourceManager.GetString("CategoryFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property {0}.{1} could not be found..
        /// </summary>
        internal static string Common_PropertyNotFound {
            get {
                return ResourceManager.GetString("Common_PropertyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  The type {0} must derive from {1}.
        /// </summary>
        internal static string Common_TypeMustDeriveFromType {
            get {
                return ResourceManager.GetString("Common_TypeMustDeriveFromType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} must have a public constructor which accepts three parameters of types {1}, {2}, and {3}..
        /// </summary>
        internal static string DataAnnotationsModelValidatorProvider_ConstructorRequirements {
            get {
                return ResourceManager.GetString("DataAnnotationsModelValidatorProvider_ConstructorRequirements", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type {0} must have a public constructor which accepts two parameters of types {1} and {2}..
        /// </summary>
        internal static string DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements {
            get {
                return ResourceManager.GetString("DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred when trying to create a handler of type &apos;{0}&apos;. Make sure that the handler has a parameterless public constructor..
        /// </summary>
        internal static string DefaultHandlerActivator_ErrorCreatingHandler {
            get {
                return ResourceManager.GetString("DefaultHandlerActivator_ErrorCreatingHandler", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple types were found that match the command of type &apos;{0}&apos;. This can happen if the processor that services this request found multiple handlers defined with the same command, which is not supported.{2}{2}
        ///The request for &apos;{0}&apos; has found the following matching handlers : {1}.
        /// </summary>
        internal static string DefaultHandlerSelector_CommandTypeAmbiguous {
            get {
                return ResourceManager.GetString("DefaultHandlerSelector_CommandTypeAmbiguous", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No handler was found that matches the command of type &apos;{0}&apos;..
        /// </summary>
        internal static string DefaultHandlerSelector_HandlerNotFound {
            get {
                return ResourceManager.GetString("DefaultHandlerSelector_HandlerNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service type {0} is not supported..
        /// </summary>
        internal static string DefaultServices_InvalidServiceType {
            get {
                return ResourceManager.GetString("DefaultServices_InvalidServiceType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No service registered for type &apos;{0}&apos;..
        /// </summary>
        internal static string DependencyResolverNoService {
            get {
                return ResourceManager.GetString("DependencyResolverNoService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Elapsed=&apos;{0} ms&apos;.
        /// </summary>
        internal static string ElapsedFormat {
            get {
                return ResourceManager.GetString("ElapsedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exception={0}.
        /// </summary>
        internal static string ExceptionFormat {
            get {
                return ResourceManager.GetString("ExceptionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to After calling {0}.OnHandlerExecuted, the HandlerExecutedContext properties Result and Exception were both null. At least one of these values must be non-null. To provide a new response, please set the Result object; to indicate an error, please throw an exception..
        /// </summary>
        internal static string HandlerFilterAttribute_MustSupplyResponseOrException {
            get {
                return ResourceManager.GetString("HandlerFilterAttribute_MustSupplyResponseOrException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Id={0}.
        /// </summary>
        internal static string IdFormat {
            get {
                return ResourceManager.GetString("IdFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value of argument &apos;{0}&apos; ({1}) is invalid for Enum type &apos;{2}&apos;..
        /// </summary>
        internal static string InvalidEnumArgument {
            get {
                return ResourceManager.GetString("InvalidEnumArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message=&apos;{0}&apos;.
        /// </summary>
        internal static string MessageFormat {
            get {
                return ResourceManager.GetString("MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &apos;{0}&apos; on type &apos;{1}&apos; is invalid. Value-typed properties marked as [Required] must also be marked with [DataMember(IsRequired=true)] to be recognized as required. Consider attributing the declaring type with [DataContract] and the property with [DataMember(IsRequired=true)]..
        /// </summary>
        internal static string MissingDataMemberIsRequired {
            get {
                return ResourceManager.GetString("MissingDataMemberIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} property is required..
        /// </summary>
        internal static string MissingRequiredMember {
            get {
                return ResourceManager.GetString("MissingRequiredMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation={0}.{1}.
        /// </summary>
        internal static string OperationFormat {
            get {
                return ResourceManager.GetString("OperationFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request.
        /// </summary>
        internal static string ShortRequestFormat {
            get {
                return ResourceManager.GetString("ShortRequestFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Response.
        /// </summary>
        internal static string ShortResponseFormat {
            get {
                return ResourceManager.GetString("ShortResponseFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{0}] Level={1}, Kind={2}.
        /// </summary>
        internal static string TimeLevelKindFormat {
            get {
                return ResourceManager.GetString("TimeLevelKindFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{0}] Request received.
        /// </summary>
        internal static string TimeRequestFormat {
            get {
                return ResourceManager.GetString("TimeRequestFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{0}] Sending response.
        /// </summary>
        internal static string TimeResponseFormat {
            get {
                return ResourceManager.GetString("TimeResponseFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handler filter for &apos;{0}&apos;.
        /// </summary>
        internal static string TraceActionFilterMessage {
            get {
                return ResourceManager.GetString("TraceActionFilterMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cancelled.
        /// </summary>
        internal static string TraceCancelledMessage {
            get {
                return ResourceManager.GetString("TraceCancelledMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Executed handler for command &apos;{0}&apos;.
        /// </summary>
        internal static string TraceHandlerExecutedMessage {
            get {
                return ResourceManager.GetString("TraceHandlerExecutedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Selected handler &apos;{0}&apos;.
        /// </summary>
        internal static string TraceHandlerSelectedMessage {
            get {
                return ResourceManager.GetString("TraceHandlerSelectedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The TraceLevel property must be a value between TraceLevel.Off and TraceLevel.Fatal, inclusive..
        /// </summary>
        internal static string TraceLevelOutOfRange {
            get {
                return ResourceManager.GetString("TraceLevelOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to none.
        /// </summary>
        internal static string TraceNoneObjectMessage {
            get {
                return ResourceManager.GetString("TraceNoneObjectMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validated request for command &apos;{0}&apos;.
        /// </summary>
        internal static string TraceRequestValidatedMessage {
            get {
                return ResourceManager.GetString("TraceRequestValidatedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The model object inside the metadata claimed to be compatible with {0}, but was actually {1}..
        /// </summary>
        internal static string ValidatableObjectAdapter_IncompatibleType {
            get {
                return ResourceManager.GetString("ValidatableObjectAdapter_IncompatibleType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A value is required but was not present in the request..
        /// </summary>
        internal static string Validation_ValueNotFound {
            get {
                return ResourceManager.GetString("Validation_ValueNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Field &apos;{0}&apos; on type &apos;{1}&apos; is attributed with one or more validation attributes. Validation attributes on fields are not supported. Consider using a public property for validation instead..
        /// </summary>
        internal static string ValidationAttributeOnField {
            get {
                return ResourceManager.GetString("ValidationAttributeOnField", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-public property &apos;{0}&apos; on type &apos;{1}&apos; is attributed with one or more validation attributes. Validation attributes on non-public properties are not supported. Consider using a public property for validation instead..
        /// </summary>
        internal static string ValidationAttributeOnNonPublicProperty {
            get {
                return ResourceManager.GetString("ValidationAttributeOnNonPublicProperty", resourceCulture);
            }
        }
    }
}
