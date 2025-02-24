using System.Collections.Generic;
using System.Xml.Linq;

namespace GraphAhoi.Extensions;

public static class EnumerableExtensions
{
    //public static IEnumerable<T> Yield<T>(this T source)

    //{
    //    yield return source;
    //}



    /// <summary>
    /// Uses TrueDFS
    /// </summary>
    public static IEnumerable<T> TopologicalOrder<T>(this IEnumerable<T> sources, Func<T, IEnumerable<T>> getDependencies)
    {
        var yielded = new HashSet<T>();
        var visited = new HashSet<T>();
        var stack = new Stack<(T, IEnumerator<T>)>();

        try
        {
            foreach (T source in sources)
            {
                if (visited.Add(source))
                    stack.Push((source, getDependencies(source).GetEnumerator()));

                while (stack.Any())
                {
                    var (node, enumerator) = stack.Peek();
                    bool depsPushed = false;

                    while (enumerator.MoveNext())
                    {
                        var curr = enumerator.Current;
                        if (visited.Add(curr))
                        {
                            stack.Push((curr, getDependencies(curr).GetEnumerator()));
                            depsPushed = true;
                            break;
                        }
                        else if (!yielded.Contains(curr))
                            throw new Exception($"Cycle detected at {curr}");
                    }

                    if (!depsPushed)
                    {
                        stack.Pop();
                        enumerator.Dispose();
                        if (!yielded.Add(node))
                            throw new Exception($"Bug detected: {node} yielded twice");
                        yield return node;
                    }
                }
            }
        }
        finally
        {
            // Only wanted to get here if exception, but catch is not allowed when using yield. But finally works too.
            while (stack.Any())
            {
                var (_, enumerator) = stack.Pop();
                enumerator.Dispose();
            }
        }
    }

    /// <summary>
    /// Uses TrueDFS
    /// If you have async deps, you can use this method.
    /// </summary>
    public static async IAsyncEnumerable<T> TopologicalOrderAsync<T>(this IEnumerable<T> sources, Func<T, IAsyncEnumerable<T>> deps)//, bool allowCycles = false)
    {
        var yielded = new HashSet<T>();
        var visited = new HashSet<T>();
        var stack = new Stack<(T, IAsyncEnumerator<T>)>();

        try
        {
            foreach (T source in sources)
            {
                if (visited.Add(source))
                    stack.Push((source, deps(source).GetAsyncEnumerator()));

                while (stack.Any())
                {
                    var (node, enumerator) = stack.Peek();
                    bool depsPushed = false;

                    while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        var curr = enumerator.Current;
                        if (visited.Add(curr))
                        {
                            stack.Push((curr, deps(curr).GetAsyncEnumerator()));
                            depsPushed = true;
                            break;
                        }
                        else if (!yielded.Contains(curr))
                            throw new Exception($"Cycle detected at {curr}");
                    }

                    if (!depsPushed)
                    {
                        stack.Pop();
                        await enumerator.DisposeAsync().ConfigureAwait(false);
                        if (!yielded.Add(node))
                            throw new Exception($"Bug detected: {node} yielded twice");
                        yield return node;
                    }
                }
            }
        }
        finally
        {
            // Only wanted to get here if exception, but catch is not allowed when using yield. But finally works too.
            while (stack.Any())
            {
                var (_, enumerator) = stack.Pop();
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
