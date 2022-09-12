using System.Runtime.Serialization;

namespace Cycles.Old
{
    public class CreateDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull where TValue : new()
    {
        public CreateDictionary()
        {
        }

        public CreateDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public CreateDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
        {
        }

        public CreateDictionary(IEqualityComparer<TKey>? comparer) : base(comparer)
        {
        }

        public CreateDictionary(int capacity) : base(capacity)
        {
        }

        public CreateDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer) : base(dictionary, comparer)
        {
        }

        public CreateDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) : base(collection, comparer)
        {
        }

        public CreateDictionary(int capacity, IEqualityComparer<TKey>? comparer) : base(capacity, comparer)
        {
        }

        protected CreateDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                {
                    value = new TValue();
                    Add(key, value);
                }
                return value;
            }
            set
            {
                this[key] = value;
            }
        }
    }
}
