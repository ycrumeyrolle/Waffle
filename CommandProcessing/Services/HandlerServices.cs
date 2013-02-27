namespace CommandProcessing.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a container for services that can be specific to a handler. 
    /// This shadows the services from its parent <see cref="ServicesContainer"/>. A handler can either set a service here, or fall through 
    /// to the more global set of services. 
    /// </summary>
    public class HandlerServices : ServicesContainer
    {
        private readonly ServicesContainer parent;

        // This lists specific services that have been over ridden for the handler.
        // Anything missing means just fall through and ask the parent. 
        // This dictionary is only written at initialization time, and then read-only during steady state.
        // So it can be safely read from multiple threads after initialization.
        private Dictionary<Type, object> overrideSingle;

        private Dictionary<Type, List<object>> overrideMulti;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerServices"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent container.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="parent"/> is null.
        /// </exception>
        public HandlerServices(ServicesContainer parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            this.parent = parent;
        }

        /// <summary>
        /// Determine whether the service type should be fetched with GetService or GetServices. 
        /// </summary>
        /// <param name="serviceType">
        /// Type of service to query.
        /// </param>
        /// <returns>
        /// True if the service is singular. 
        /// </returns>
        public override bool IsSingleService(Type serviceType)
        {
            return this.parent.IsSingleService(serviceType);
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The first instance of the service, or null if the service is not found.</returns>
        public override object GetService(Type serviceType)
        {
            if (this.overrideSingle != null)
            {
                object item;
                if (this.overrideSingle.TryGetValue(serviceType, out item))
                {
                    return item;
                }
            }

            return this.parent.GetService(serviceType);
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. Returns an empty enumeration if the
        /// service is not found. </returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            if (this.overrideMulti != null)
            {
                List<object> list;
                if (this.overrideMulti.TryGetValue(serviceType, out list))
                {
                    return list;
                }
            }

            return this.parent.GetServices(serviceType);
        }

        /// <inheritdoc/>
        protected override void ReplaceSingle(Type serviceType, object service)
        {
            if (this.overrideSingle == null)
            {
                this.overrideSingle = new Dictionary<Type, object>();
            }

            this.overrideSingle[serviceType] = service;
        }

        /// <inheritdoc/>
        protected override void ClearSingle(Type serviceType)
        {
            if (this.overrideSingle == null)
            {
                return;
            }

            this.overrideSingle.Remove(serviceType);
        }

        /// <summary>
        /// Returns the list of object for the given service type. Also validates <paramref name="serviceType"/> is in the known service type list.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The list of service instances of the given type. </returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "want a mutable list")]
        protected override List<object> GetServiceInstances(Type serviceType)
        {
            if (this.overrideMulti == null)
            {
                this.overrideMulti = new Dictionary<Type, List<object>>();
            }

            List<object> list;
            if (!this.overrideMulti.TryGetValue(serviceType, out list))
            {
                // Copy parents list. 
                list = new List<object>(this.parent.GetServices(serviceType));

                // Copy into per-handler. If they're asking for the list, the expectation is that it's going to get mutated.
                this.overrideMulti[serviceType] = list;
            }

            return list;
        }
    }
}