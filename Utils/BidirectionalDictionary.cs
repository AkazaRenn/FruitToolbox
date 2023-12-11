namespace FruitToolbox.Utils;

public class BidirectionalDictionary<TKey, TValue> {
    readonly Dictionary<TKey, TValue> forward = [];
    readonly Dictionary<TValue, HashSet<TKey>> reverse = [];

    public TValue this[TKey key] {
        get => forward[key];
        set {
            forward[key] = value;
            reverse.TryAdd(value, []);
            reverse[value].Add(key);
        }
    }

    public bool TryGet(TKey key, out TValue value) =>
        forward.TryGetValue(key, out value);

    public bool TryGet(TValue value, out HashSet<TKey> key) =>
        reverse.TryGetValue(value, out key);

    public bool Remove(TKey key) {
        if (reverse.TryGetValue(forward[key], out HashSet<TKey> keySet)) {
            keySet.Remove(key);
            if (keySet.Count == 0) {
                reverse.Remove(forward[key]);
            }
        }
        return forward.Remove(key);
    }
}