////namespace Waffle.Tracing
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Diagnostics.CodeAnalysis;
////    using System.Diagnostics.Contracts;
////    using System.Linq;
////    using System.Text;
////    using System.Threading.Tasks;
////    using Waffle.Internal;
////    using Waffle.Tracing;
////    using Waffle.Validation;

////    /// <summary>
////    /// Extension methods for <see cref="TraceRecord"/>.
////    /// </summary>
////    internal static class TraceWriterExceptionMapper
////    {
////        private const string HttpErrorExceptionMessageFormat = "ExceptionMessage{0}='{1}'";
////        private const string HttpErrorExceptionTypeFormat = "ExceptionType{0}='{1}'";
////        private const string HttpErrorMessageDetailFormat = "MessageDetail='{0}'";
////        private const string HttpErrorModelStateErrorFormat = "ModelStateError=[{0}]";
////        private const string HttpErrorModelStatePairFormat = "{0}=[{1}]";
////        private const string HttpErrorStackTraceFormat = "StackTrace{0}={1}";
////        private const string HttpErrorUserMessageFormat = "UserMessage='{0}'";

////        /// <summary>
////        /// Examines the given <see cref="TraceRecord"/> to determine whether it
////        /// contains an <see cref="HttpResponseException"/> and if so, modifies
////        /// the <see cref="TraceRecord"/> to capture more detailed information.
////        /// </summary>
////        /// <param name="traceRecord">The <see cref="TraceRecord"/> to examine and modify.</param>
////        public static void TranslateHttpResponseException(TraceRecord traceRecord)
////        {
////            Contract.Assert(traceRecord != null);

////            HandlerResponseException httpResponseException = ExtractHttpResponseException(traceRecord.Exception);
////            if (httpResponseException == null)
////            {
////                return;
////            }

////            HandlerResponse response = httpResponseException.Response;
////            Contract.Assert(response != null);

////            traceRecord.Level = GetMappedTraceLevel(httpResponseException) ?? traceRecord.Level;

////            if (response.Value == null)
////            {
////                return;
////            }

////            object messageObject = null;
////            object messageDetailsObject = null;

////            List<string> messages = new List<string>();

////            // Extract the exception from this HttpError and then incrementally
////            // walk down all inner exceptions.
////            AddExceptions(httpError, messages);

////            // ModelState errors are handled with a nested HttpError
////            object modelStateErrorObject = null;
////            if (httpError.TryGetValue(HttpErrorKeys.ModelStateKey, out modelStateErrorObject))
////            {
////                HttpError modelStateError = modelStateErrorObject as HttpError;
////                if (modelStateError != null)
////                {
////                    messages.Add(FormatModelStateErrors(modelStateError));
////                }
////            }

////            traceRecord.Message = String.Join(", ", messages);
////        }

////        /// <summary>
////        /// Gets the <see cref="TraceLevel"/> per the <see cref="HttpStatusCode"/> if the given exception is
////        /// a <see cref="HttpResponseException"/>; otherwise <see langword="null"/>.
////        /// </summary>
////        /// <param name="exception">The exception.</param>
////        /// <returns>The corresponding trace level if the exception represents an <see cref="HttpResponseException"/>.</returns>
////        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "shared source. called from a different assembly")]
////        public static TraceLevel? GetMappedTraceLevel(Exception exception)
////        {
////            HandlerResponseException httpResponseException = ExtractHttpResponseException(exception);
////            if (httpResponseException == null)
////            {
////                return null;
////            }

////            return GetMappedTraceLevel(httpResponseException);
////        }

////        /// <summary>
////        /// Gets the <see cref="TraceLevel"/> per the <see cref="HttpStatusCode"/>.
////        /// </summary>
////        /// <param name="httpResponseException">The exception for which the trace level has to be found.</param>
////        /// <returns>The mapped trace level.</returns>
////        public static TraceLevel? GetMappedTraceLevel(HandlerResponseException httpResponseException)
////        {
////            Contract.Assert(httpResponseException != null);

////            HttpResponseMessage response = httpResponseException.Response;
////            Contract.Assert(response != null);

////            TraceLevel? level = null;

////            // Client level errors are downgraded to TraceLevel.Warn
////            if ((int)response.StatusCode < (int)HttpStatusCode.InternalServerError)
////            {
////                level = TraceLevel.Warn;
////            }

////            // Non errors are downgraded to TraceLevel.Info
////            if ((int)response.StatusCode < (int)HttpStatusCode.BadRequest)
////            {
////                level = TraceLevel.Info;
////            }

////            return level;
////        }

////        private static HandlerResponseException ExtractHandlerResponseException(Exception exception)
////        {
////            if (exception == null)
////            {
////                return null;
////            }

////            var handlerResponseException = exception as HandlerResponseException;
////            if (handlerResponseException != null)
////            {
////                return handlerResponseException;
////            }

////            var aggregateException = exception as AggregateException;
////            if (aggregateException != null)
////            {
////                handlerResponseException = aggregateException
////                    .Flatten()
////                    .InnerExceptions
////                    .Select(ExtractHandlerResponseException)
////                    .Where(ex => ex != null && ex.Response != null)
////                    .FirstOrDefault();
////                return handlerResponseException;
////            }

////            return ExtractHandlerResponseException(exception.InnerException);
////        }

////        /// <summary>
////        /// Unpacks any exceptions in the given <see cref="HttpError"/> and adds
////        /// them into a collection of name-value pairs that can be composed into a single string.
////        /// </summary>
////        /// <remarks>
////        /// This helper also iterates over all inner exceptions and unpacks them too.
////        /// </remarks>
////        /// <param name="httpError">The <see cref="HttpError"/> to unpack.</param>
////        /// <param name="messages">A collection of messages to which the new information should be added.</param>
////        private static void AddExceptions(HttpError httpError, List<string> messages)
////        {
////            Contract.Assert(httpError != null);
////            Contract.Assert(messages != null);

////            object exceptionMessageObject = null;
////            object exceptionTypeObject = null;
////            object stackTraceObject = null;
////            object innerExceptionObject = null;

////            for (int i = 0; httpError != null; i++)
////            {
////                // For uniqueness, key names append the depth of inner exception
////                string indexText = i == 0 ? String.Empty : Error.Format("[{0}]", i);

////                if (httpError.TryGetValue(HttpErrorKeys.ExceptionTypeKey, out exceptionTypeObject))
////                {
////                    messages.Add(Error.Format(HttpErrorExceptionTypeFormat, indexText, exceptionTypeObject));
////                }

////                if (httpError.TryGetValue(HttpErrorKeys.ExceptionMessageKey, out exceptionMessageObject))
////                {
////                    messages.Add(Error.Format(HttpErrorExceptionMessageFormat, indexText, exceptionMessageObject));
////                }

////                if (httpError.TryGetValue(HttpErrorKeys.StackTraceKey, out stackTraceObject))
////                {
////                    messages.Add(Error.Format(HttpErrorStackTraceFormat, indexText, stackTraceObject));
////                }

////                if (!httpError.TryGetValue(HttpErrorKeys.InnerExceptionKey, out innerExceptionObject))
////                {
////                    break;
////                }

////                Contract.Assert(!Object.ReferenceEquals(httpError, innerExceptionObject));

////                httpError = innerExceptionObject as HttpError;
////            }
////        }
        
////        private static string FormatModelStateErrors(ModelStateDictionary modelState)
////        {
////            Contract.Assert(modelState != null);

////            List<string> messages = new List<string>();
////            foreach (var pair in modelState)
////            {
////                IEnumerable<string> errorList = pair.Value as IEnumerable<string>;
////                if (errorList != null)
////                {
////                    messages.Add(Error.Format(HttpErrorModelStatePairFormat, pair.Key, String.Join(", ", errorList)));
////                }
////            }

////            return Error.Format(HttpErrorModelStateErrorFormat, String.Join(", ", messages));
////        }
////    }
////}
