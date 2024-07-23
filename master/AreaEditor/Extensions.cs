public static class Extensions {
    public static bool FirstOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, out T? result) {
        try {
            result = enumerable.First<T>(predicate);
            return true;
        } catch {
            result = default;
            return false;
        }
    }
}