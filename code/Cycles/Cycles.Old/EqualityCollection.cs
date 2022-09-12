using System.Collections;

namespace Cycles.Old
{
    public class EqualityCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IEquatable<EqualityCollection<T>?> where T : notnull
    {
        private readonly ICollection<T> _collection;
        private int hashCode;

        public EqualityCollection(ICollection<T> collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public void Add(T item)
        {
            _collection.Add(item);
            RegenerateHashCode();
        }

        public void Clear()
        {
            _collection.Clear();
            RegenerateHashCode();
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EqualityCollection<T>);
        }

        public virtual bool Equals(EqualityCollection<T>? other)
        {
            if (other is not null && GetHashCode() == other.GetHashCode() && Count == other.Count)
            {
                foreach (var item in this)
                {
                    if (!other.Contains(item))
                        return false;
                }
                return true;
            }
            return false;
        }

        protected virtual void RegenerateHashCode()
        {
            hashCode = 0;
            foreach (var itemHashCode in _collection.Select(i => i.GetHashCode()).OrderBy(i => i))
            {
                hashCode = HashCode.Combine(hashCode, itemHashCode);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public bool Remove(T item)
        {
            var r = _collection.Remove(item);
            if (r)
                RegenerateHashCode();
            return r;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }

        public static bool operator ==(EqualityCollection<T>? left, EqualityCollection<T>? right)
        {
            return EqualityComparer<EqualityCollection<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(EqualityCollection<T>? left, EqualityCollection<T>? right)
        {
            return !(left == right);
        }
    }

    public class EqualitySet<T> : EqualityCollection<T>, ISet<T> where T : notnull
    {
        private readonly ISet<T> _set;

        public EqualitySet(ISet<T> set) : base(set)
        {
            _set = set;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _set.ExceptWith(other);
            RegenerateHashCode();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _set.IntersectWith(other);
            RegenerateHashCode();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _set.SymmetricExceptWith(other);
            RegenerateHashCode();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _set.UnionWith(other);
            RegenerateHashCode();
        }

        bool ISet<T>.Add(T item)
        {
            var r = _set.Add(item);
            if (r)
                RegenerateHashCode();
            return r;
        }
    }
}
