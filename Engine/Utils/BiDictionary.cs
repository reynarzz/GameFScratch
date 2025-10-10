using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class BiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _keyToValue = new();
        private readonly Dictionary<TValue, TKey> _valueToKey = new();

        public int Count => _keyToValue.Count;

        public void Add(TKey key, TValue value)
        {
            _keyToValue[key] = value;
            _valueToKey[value] = key;
        }

        public bool RemoveByKey(TKey key)
        {
            if (!_keyToValue.TryGetValue(key, out TValue value))
                return false;

            _keyToValue.Remove(key);
            _valueToKey.Remove(value);
            return true;
        }

        public bool RemoveByValue(TValue value)
        {
            if (!_valueToKey.TryGetValue(value, out TKey key))
                return false;

            _valueToKey.Remove(value);
            _keyToValue.Remove(key);
            return true;
        }

        public TValue GetByKey(TKey key) => _keyToValue[key];
        public TKey GetByValue(TValue value) => _valueToKey[value];

        public bool TryGetByKey(TKey key, out TValue value) => _keyToValue.TryGetValue(key, out value);
        public bool TryGetByValue(TValue value, out TKey key) => _valueToKey.TryGetValue(value, out key);

        public bool ContainsKey(TKey key) => _keyToValue.ContainsKey(key);
        public bool ContainsValue(TValue value) => _valueToKey.ContainsKey(value);

        public void Clear()
        {
            _keyToValue.Clear();
            _valueToKey.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _keyToValue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
