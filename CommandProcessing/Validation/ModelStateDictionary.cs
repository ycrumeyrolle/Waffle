namespace CommandProcessing.Validation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using CommandProcessing.Internal;

    [Serializable]
    public class ModelStateDictionary : IDictionary<string, ModelState>
    {
        private readonly Dictionary<string, ModelState> innerDictionary = new Dictionary<string, ModelState>(StringComparer.OrdinalIgnoreCase);

        public ModelStateDictionary()
        {
        }

        public ModelStateDictionary(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw Error.ArgumentNull("dictionary");
            }

            foreach (var entry in dictionary)
            {
                this.innerDictionary.Add(entry.Key, entry.Value);
            }
        }

        public int Count
        {
            get { return this.innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<string, ModelState>)this.innerDictionary).IsReadOnly; }
        }

        public bool IsValid
        {
            get { return this.Values.All(modelState => modelState.Errors.Count == 0); }
        }

        public ICollection<string> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        public ICollection<ModelState> Values
        {
            get { return this.innerDictionary.Values; }
        }

        public ModelState this[string key]
        {
            get
            {
                ModelState value;
                this.innerDictionary.TryGetValue(key, out value);
                return value;
            }

            set
            {
                this.innerDictionary[key] = value;
            }
        }

        public void Add(KeyValuePair<string, ModelState> item)
        {
            ((IDictionary<string, ModelState>)this.innerDictionary).Add(item);
        }

        public void Add(string key, ModelState value)
        {
            this.innerDictionary.Add(key, value);
        }

        public void AddModelError(string key, Exception exception)
        {
            this.GetModelStateForKey(key).Errors.Add(exception);
        }

        public void AddModelError(string key, string errorMessage)
        {
            this.GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        public void Clear()
        {
            this.innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)this.innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this.innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex)
        {
            ((IDictionary<string, ModelState>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, ModelState>> GetEnumerator()
        {
            return this.innerDictionary.GetEnumerator();
        }

        private ModelState GetModelStateForKey(string key)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            ModelState modelState;
            if (!this.TryGetValue(key, out modelState))
            {
                modelState = new ModelState();
                this[key] = modelState;
            }

            return modelState;
        }

        public bool IsValidField(string key)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            // if the key is not found in the dictionary, we just say that it's valid (since there are no errors)
            foreach (KeyValuePair<string, ModelState> entry in this.FindKeysWithPrefix(key))
            {
                if (entry.Value.Errors.Count != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public void Merge(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var entry in dictionary)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public bool Remove(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)this.innerDictionary).Remove(item);
        }

        public bool Remove(string key)
        {
            return this.innerDictionary.Remove(key);
        }

        //public void SetModelValue(string key, ValueProviderResult value)
        //{
        //    this.GetModelStateForKey(key).Value = value;
        //}

        public bool TryGetValue(string key, out ModelState value)
        {
            return this.innerDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.innerDictionary).GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, ModelState>> FindKeysWithPrefix(string prefix)
        {
            Contract.Assert(prefix != null);

            ModelState exactMatchValue;
            if (this.TryGetValue(prefix, out exactMatchValue))
            {
                yield return new KeyValuePair<string, ModelState>(prefix, exactMatchValue);
            }

            foreach (var entry in this)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Everything is prefixed by the empty string
                if (prefix.Length == 0)
                {
                    yield return entry;
                }
                else
                {
                    char charAfterPrefix = key[prefix.Length];
                    switch (charAfterPrefix)
                    {
                        case '[':
                        case '.':
                            yield return entry;
                            break;
                    }
                }
            }
        }
    }
}
