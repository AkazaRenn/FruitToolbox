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
}