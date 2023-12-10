namespace FruitToolbox.Utils;

public class BidirectionalDictionary<TKey, TValue> {
    readonly Dictionary<TKey, TValue> forward = [];
    readonly Dictionary<TValue, TKey> reverse = [];

    public TValue this[TKey key] {
        get => forward[key];
        set {
            forward[key] = value;
            reverse[value] = key;
        }
    }

    public bool TryGet(TKey key, out TValue value) =>
        forward.TryGetValue(key, out value);

    public bool TryGet(TValue value, out TKey key) =>
        reverse.TryGetValue(value, out key);

    public bool Remove(TKey key) {
        reverse.Remove(forward[key]);
        return forward.Remove(key);
    }
}