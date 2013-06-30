namespace Waffle.Validation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Internal;

    /// <summary>
    /// Represents the state of validation for a command.
    /// </summary>
    [Serializable]
    public class ModelStateDictionary : IDictionary<string, ModelState>
    {
        private readonly Dictionary<string, ModelState> innerDictionary = new Dictionary<string, ModelState>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStateDictionary"/> class.
        /// </summary>
        public ModelStateDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStateDictionary"/> class by using values that are copied from the specified model-state dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
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

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <value>The number of key/value pairs in the collection.</value>
        public int Count
        {
            get { return this.innerDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, <c>false</c>.
        /// </returns>
        /// <value>A value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</value>
        public bool IsReadOnly
        {
            get { return ((IDictionary<string, ModelState>)this.innerDictionary).IsReadOnly; }
        }

        /// <summary>
        /// Gets a value that indicates whether this instance of the model-state dictionary is valid.
        /// </summary>
        /// <value>A value that indicates whether this instance of the model-state dictionary is valid.</value>
        public bool IsValid
        {
            get { return this.Values.All(modelState => modelState.Errors.Count == 0); }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </value>
        public ICollection<string> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </value>
        public ICollection<ModelState> Values
        {
            get { return this.innerDictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
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

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<string, ModelState> item)
        {
            ((IDictionary<string, ModelState>)this.innerDictionary).Add(item);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(string key, ModelState value)
        {
            this.innerDictionary.Add(key, value);
        }

        /// <summary>
        /// Adds the specified model error to the errors collection for the model-state dictionary that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="exception">The exception to add.</param>
        public void AddModelError(string key, Exception exception)
        {
            this.GetModelStateForKey(key).Errors.Add(exception);
        }

        /// <summary>
        /// Adds the specified error message to the errors collection for the model-state dictionary that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="errorMessage">The error message to add.</param>
        public void AddModelError(string key, string errorMessage)
        {
            this.GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            this.innerDictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <value>
        /// <c>true</c> if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, <c>false</c>.
        /// </value>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)this.innerDictionary).Contains(item);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, <c>false</c>.
        /// </value>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(string key)
        {
            return this.innerDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex)
        {
            ((IDictionary<string, ModelState>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
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

        /// <summary>
        /// Determines whether there are any <see cref="ModelError"/> objects that are associated with or prefixed with the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the model-state dictionary contains a value that is associated with the specified key; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Copies the values from the specified ModelStateDictionary object into this dictionary, overwriting existing values if keys are the same.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
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

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, <c>false</c>. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)this.innerDictionary).Remove(item);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the element is successfully removed; otherwise, <c>false</c>.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(string key)
        {
            return this.innerDictionary.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(string key, out ModelState value)
        {
            return this.innerDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
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
