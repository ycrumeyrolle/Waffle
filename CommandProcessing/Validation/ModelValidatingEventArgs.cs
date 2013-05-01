//namespace CommandProcessing.Validation
//{
//    using System.ComponentModel;
//    using System.Web.Http.Validation;
//    using CommandProcessing.Filters;
//    using CommandProcessing.Internal;

//    public sealed class ModelValidatingEventArgs : CancelEventArgs
//    {
//        public ModelValidatingEventArgs(HandlerContext actionContext, ModelValidationNode parentNode)
//        {
//            if (actionContext == null)
//            {
//                throw Error.ArgumentNull("actionContext");
//            }

//            this.ActionContext = actionContext;
//            this.ParentNode = parentNode;
//        }

//        public HandlerContext ActionContext { get; private set; }

//        public ModelValidationNode ParentNode { get; private set; }
//    }
//}
