namespace FruitToolbox.Utils;

public class BidirectionalDictionary<K, V>: Dictionary<K, V> {
    readonly Dictionary<V, K> reverse = [];

    public new V this[K key] {
        get => base[key];
        set {
            base[key] = value;
            reverse[value] = key;
        }
    }

    public bool TryGetKey(V value, out K key) =>
        reverse.TryGetValue(value, out key);

    public new bool Remove(K key) {
        reverse.Remove(base[key]);
        return base.Remove(key);
    }

    public new void Add(K key, V value) {
        base.Add(key, value);
        reverse.Add(value, key);
    }

    public new void TryAdd(K key, V value) {
        base.TryAdd(key, value);
        reverse.TryAdd(value, key);
    }

    public new void EnsureCapacity(int capacity) {
        base.EnsureCapacity(capacity);
        reverse.EnsureCapacity(capacity);
    }

    public new void TrimExcess() {
        base.TrimExcess();
        reverse.TrimExcess();
    }

    public new void Clear() {
        base.Clear();
        reverse.Clear();
    }
}