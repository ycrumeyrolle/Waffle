namespace Waffle
{
    using System;
    using Waffle.Internal;

    public class HandlerResponse<T>
    {
        private readonly HandlerResponse response;

        internal static readonly HandlerResponse<T> Empty = new HandlerResponse<T>(HandlerResponse.Empty);

        public HandlerResponse(HandlerResponse response)
        {
            if (response == null)
            {
                Error.ArgumentNull("response");
            }

            this.response = response;
        }

        public HandlerRequest Request
        {
            get
            {
                return this.response.Request;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.response.Exception;
            }
        }

        public T Value
        {
            get
            {
                return (T)this.response.Value;
            }
        }
    }
}
