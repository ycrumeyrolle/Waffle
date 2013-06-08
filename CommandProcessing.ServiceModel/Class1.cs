using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandProcessing.ServiceModel
{
    using System.Data.Services.Client;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using CommandProcessing.Queries;

    public class WcfQueryContext<TChannel> : IQueryContext where TChannel : IChannel
    {
        private readonly DataServiceContext context;

        private readonly TChannel channel;

        public WcfQueryContext()
        {
            this.channelFactory = new ChannelFactory<TChannel>("*");
            this.channel = this.channelFactory.CreateChannel();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query the context.
        /// </summary>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns>A <see cref="IQueryable{T}"/>.</returns>
        public IQueryable<T> Query<T>() where T : class
        {
            this.channel.Open();
            this.channel.
        }

        /// <summary>
        /// Find the object.
        /// </summary>
        /// <remarks>Returns <c>null</c> if nothing is found.</remarks>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <param name="keyValues">The values of the primary key for the object to be found.</param>
        /// <returns> The object found, or null.</returns>
        public T Find<T>(params object[] keyValues) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
