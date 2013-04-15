namespace CommandProcessing
{
    using System.Security.Principal;
    using System.Threading;

    public class DefaultPrincipalProvider : IPrincipalProvider
    {
        public IPrincipal Principal
        {
            get
            {
                return Thread.CurrentPrincipal;
            }

            set
            {
                Thread.CurrentPrincipal = value;
            }
        }
    }
}
