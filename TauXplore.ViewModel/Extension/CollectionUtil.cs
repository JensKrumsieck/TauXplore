namespace TauXplore.ViewModel.Extension;
public static class CollectionUtil
{
    public static IEnumerable<IEnumerable<T>> KCombs<T>(this IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { Array.Empty<T>() } :
          elements.SelectMany((e, i) =>
            elements.Skip(i + 1).KCombs(k - 1).Select(c => (new[] { e }).Concat(c)));
    }
}
