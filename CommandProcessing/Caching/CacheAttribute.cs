////namespace CommandProcessing.Caching
////{
////    using System;
////    using System.Collections.Concurrent;
////    using System.Collections.Generic;
////    using System.Runtime.Caching;
////    using CommandProcessing.Filters;
////    using CommandProcessing.Services;

////    public class CacheAttribute : FilterAttribute, IHandlerFilter
////    {
////        private readonly ConcurrentDictionary<Type, object> cache = new ConcurrentDictionary<Type, object>();

////        private string varyByParams = "*";

////        private string[] vary

////        public string VaryByParams
////        {
////            get
////            {
////                return this.varyByParams;
////            }

////            set
////            {
////                if (string.IsNullOrEmpty(value))
////                {
////                    throw new ArgumentException("value");
////                }

////                this.varyByParams = value;
////            }
////        }

////        public void OnCommandExecuting(HandlerExecutingContext context)
////        {

////        }

////        public void OnCommandExecuted(HandlerExecutedContext context)
////        {
////            throw new NotImplementedException();
////        }

////        private static string GetOrCreateKey(Command command)
////        {
////            IKeyedCommand keyedCommand = command as IKeyedCommand;
////            if (keyedCommand != null)
////            {
////                return keyedCommand.Key;
////            }

////            string key = null;
////            return key;

////        }

////    }

////    public interface IKeyedCommand
////    {
////        string Key { get; }
////    }
////}
