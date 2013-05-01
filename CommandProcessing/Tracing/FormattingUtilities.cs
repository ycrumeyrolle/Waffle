namespace CommandProcessing.Tracing
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using CommandProcessing.Filters;

    /// <summary>
    /// General purpose utilities to format strings used in tracing.
    /// </summary>
    internal static class FormattingUtilities
    {
        public static readonly string NullMessage = "null";
        
        public static string ActionDescriptorToString(HandlerDescriptor descriptor)
        {
            Contract.Assert(descriptor != null);

            return descriptor.Name;
        }

        public static string ActionInvokeToString(HandlerContext handlerContext)
        {
            Contract.Assert(handlerContext != null);
            return handlerContext.Descriptor.Name;
        }
        
        //public static string ModelStateToString(ModelStateDictionary modelState)
        //{
        //    Contract.Assert(modelState != null);

        //    if (modelState.IsValid)
        //    {
        //        return string.Empty;
        //    }

        //    StringBuilder modelStateBuilder = new StringBuilder();
        //    foreach (string key in modelState.Keys)
        //    {
        //        ModelState state = modelState[key];
        //        if (state.Errors.Count > 0)
        //        {
        //            foreach (ModelError error in state.Errors)
        //            {
        //                string errorString = Error.Format(Resources.TraceModelStateErrorMessage,
        //                    key,
        //                    error.ErrorMessage);
        //                if (modelStateBuilder.Length > 0)
        //                {
        //                    modelStateBuilder.Append(',');
        //                }

        //                modelStateBuilder.Append(errorString);
        //            }
        //        }
        //    }

        //    return modelStateBuilder.ToString();
        //}

        public static string ValueToString(object value, CultureInfo cultureInfo)
        {
            Contract.Assert(cultureInfo != null);

            if (value == null)
            {
                return NullMessage;
            }

            return Convert.ToString(value, cultureInfo);
        }
    }
}