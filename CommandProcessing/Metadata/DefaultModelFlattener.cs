namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using CommandProcessing.Internal;

    /// <summary>
    /// Recursively flatten an object. 
    /// </summary>
    public class DefaultModelFlattener : IModelFlattener
    {
        private interface IKeyBuilder
        {
            string AppendTo(string prefix);
        }

        /// <summary>
        /// Flatten the <paramref name="model"/>.
        /// Properties are stored into a dictionary.
        /// Property name is used as key. Properties in a submodel are separated with a period. 
        /// <see cref="Enumerable"/> properties are keyed with square brackets.
        /// </summary>
        /// <example>
        /// <list type="bullet">
        /// <item>Property1</item>
        /// <item>Property2</item>
        /// <item>Property3.SubProperty1</item>
        /// <item>Property3.SubProperty2</item>
        /// <item>Property4[0].Item</item>
        /// <item>Property4[1].Item</item>
        /// <item>...</item>
        /// </list>
        /// </example>
        /// <param name="model">The model to be flattened.</param>
        /// <param name="type">The <see cref="Type"/> to use for flattening.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns>The <see cref="ModelDictionary"/>.</returns>
        public ModelDictionary Flatten(object model, Type type, ModelMetadataProvider metadataProvider, string keyPrefix)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (metadataProvider == null)
            {
                throw Error.ArgumentNull("metadataProvider");
            }

            ModelMetadata metadata = metadataProvider.GetMetadataForType(() => model, type);
            VisitContext visitContext = new VisitContext
                {
                    MetadataProvider = metadataProvider,
                    RootPrefix = keyPrefix
                };
            this.VisitNodeAndChildren(metadata, visitContext);

            return visitContext.FlatCommand;
        }

        private void VisitNodeAndChildren(ModelMetadata metadata, VisitContext visitContext)
        {
            object model = metadata.Model;

            // Optimization: we don't need to recursively traverse the graph for null and primitive types
            if (model == null || TypeHelper.IsSimpleType(model.GetType()))
            {
                ShallowVisit(metadata, visitContext);
                return;
            }

            // Check to avoid infinite recursion. This can happen with cycles in an object graph.
            if (visitContext.Visited.Contains(model))
            {
                return;
            }

            visitContext.Visited.Add(model);

            // Visit the children first - depth-first traversal
            IEnumerable enumerableModel = model as IEnumerable;
            if (enumerableModel == null)
            {
                this.VisitProperties(metadata, visitContext);
            }
            else
            {
                this.VisitElements(enumerableModel, visitContext);
            }

            ShallowVisit(metadata, visitContext);

            // Pop the object so that it can be visited again in a different path
            visitContext.Visited.Remove(model);
        }

        private void VisitProperties(ModelMetadata metadata, VisitContext visitContext)
        {
            PropertyScope propertyScope = new PropertyScope();
            visitContext.KeyBuilders.Push(propertyScope);
            foreach (ModelMetadata childMetadata in metadata.Properties)
            {
                propertyScope.PropertyName = childMetadata.PropertyName;
                this.VisitNodeAndChildren(childMetadata, visitContext);
            }

            visitContext.KeyBuilders.Pop();
        }

        private void VisitElements(IEnumerable model, VisitContext visitContext)
        {
            Type elementType = GetElementType(model.GetType());
            ModelMetadata elementMetadata = visitContext.MetadataProvider.GetMetadataForType(null, elementType);

            ElementScope elementScope = new ElementScope { Index = 0 };
            visitContext.KeyBuilders.Push(elementScope);
            foreach (object element in model)
            {
                elementMetadata.Model = element;
                this.VisitNodeAndChildren(elementMetadata, visitContext);

                elementScope.Index++;
            }

            visitContext.KeyBuilders.Pop();
        }

        // Visits a single node (not including children)
        private static void ShallowVisit(ModelMetadata metadata, VisitContext visitContext)
        {
            Contract.Requires(visitContext.KeyBuilders != null);
            string key = visitContext.RootPrefix;
            foreach (IKeyBuilder keyBuilder in visitContext.KeyBuilders.Reverse())
            {
                key = keyBuilder.AppendTo(key);
            }

            if (!metadata.IsComplexType)
            {
                visitContext.FlatCommand.Add(key, metadata.Model);
            }
            else
            {
                Console.WriteLine(metadata.Model);
            }
        }

        private static Type GetElementType(Type type)
        {
            Contract.Assert(typeof(IEnumerable).IsAssignableFrom(type));

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            foreach (Type implementedInterface in type.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return implementedInterface.GetGenericArguments()[0];
                }
            }

            return typeof(object);
        }
        
        private class PropertyScope : IKeyBuilder
        {
            public string PropertyName { get; set; }

            public string AppendTo(string prefix)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    return this.PropertyName ?? string.Empty;
                }

                if (string.IsNullOrEmpty(this.PropertyName))
                {
                    return prefix;
                }

                return prefix + "." + this.PropertyName;
            }
        }

        private class ElementScope : IKeyBuilder
        {
            public int Index { get; set; }

            public string AppendTo(string prefix)
            {
                string index = this.Index.ToString(CultureInfo.InvariantCulture);
                return (prefix.Length == 0) ? "[" + index + "]" : prefix + "[" + index + "]";
            }
        }

        private class VisitContext
        {
            public VisitContext()
            {
                this.Visited = new HashSet<object>();
                this.KeyBuilders = new Stack<IKeyBuilder>();
                this.FlatCommand = new ModelDictionary();
            }

            public ModelMetadataProvider MetadataProvider { get; set; }

            public ModelDictionary FlatCommand { get; private set; }

            public HashSet<object> Visited { get; private set; }

            public Stack<IKeyBuilder> KeyBuilders { get; private set; }

            public string RootPrefix { get; set; }
        }
    }
}