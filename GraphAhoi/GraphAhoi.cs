
using System.Xml.Linq;
using GraphAhoi.Extensions;

namespace GraphAhoi;

public enum Direction
{
    Forward,
    Backward
}



//public interface IGraph
//{
//    public IEnumerable<INode> Nodes { get; }
//}

public interface INode
{
    public IEnumerable<IEdge> InEdges { get; }
    public IEnumerable<IEdge> OutEdges { get; }
}

public interface IEdge
{
    public INode Source { get; }
    public INode Target { get; }
}

/// <summary>
/// If TNode implement INode and TEdge implement IEdge, you can use this one directly.
/// Else you must create your own subclass of GraphTracerBase.
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TEdge"></typeparam>
public class GraphTracer<TNode, TEdge> : GraphTracerBase<TNode, TEdge>
    where TNode : INode where TEdge : IEdge
{
	public GraphTracer(GraphTracerAlgo algo) : base(algo)
	{ }

	protected override IEnumerable<TEdge> GetInEdges(TNode node)
        => node.InEdges.Cast<TEdge>();

    protected override IEnumerable<TEdge> GetOutEdges(TNode node)
        => node.OutEdges.Cast<TEdge>();

    protected override TNode GetSourceNode(TEdge edge)
        => (TNode)edge.Source;

    protected override TNode GetTargetNode(TEdge edge)
        => (TNode)edge.Target;
}

public enum GraphTracerAlgo
{
    /// <summary>
    /// Breath first search
    /// Trace using a queue. Explore in layers/levels, away from the sources.
    /// </summary>
    BFS,
    /// <summary>
    /// Depth first search
    /// Tracing using a stack of iterators. One neighbour/branch fully explored at a time.
    /// This is TrueDFS and produces same order as DFS implemented with recusive method calls.
    /// </summary>
    DFS,
    /// <summary>
    /// Tracing using a stack. All neighbours pushed (fully explored) at once.
    /// Does not produce same order as DFS implemented with recusive method calls.
    /// </summary>
//    PseudoDFS,
}

public abstract class GraphTracerBase<TNode, TEdge>
{
    GraphTracerAlgo _algo;

    protected GraphTracerBase(GraphTracerAlgo algo)
    {
        _algo = algo;
    }

    protected abstract IEnumerable<TEdge> GetInEdges(TNode node);
    protected abstract IEnumerable<TEdge> GetOutEdges(TNode node);
    protected abstract TNode GetSourceNode(TEdge edge);
    protected abstract TNode GetTargetNode(TEdge edge);

    //private IEnumerable<TNode> TraceInternalPseudoDFS(
    //    IEnumerable<TNode> sources,
    //    bool traceInEdges,
    //    bool traceOutEdges,
    //    Func<TEdge, Direction, bool>? edgeFilter = null,
    //    Func<TNode, bool>? nodeFilter = null
    //    )
    //{
    //    HashSet<TNode> visited = new();
    //    Stack<TNode> stack = new(sources);

    //    while (stack.Any())
    //    {
    //        var node = stack.Pop();
    //        if (visited.Add(node)) // true = was added to visited
    //        {
    //            yield return node;

    //            if (traceInEdges)
    //                foreach (var inEdge in GetInEdges(node))
    //                    if (edgeFilter == null || edgeFilter(inEdge, Direction.Backward))
    //                    {
    //                        var sourceNode = GetSourceNode(inEdge);
    //                        if (nodeFilter == null || nodeFilter(sourceNode))
    //                            stack.Push(sourceNode);
    //                    }

    //            if (traceOutEdges)
    //                foreach (var outEdge in GetOutEdges(node))
    //                    if (edgeFilter == null || edgeFilter(outEdge, Direction.Forward))
    //                    {
    //                        var targetNode = GetTargetNode(outEdge);
    //                        if (nodeFilter == null || nodeFilter(targetNode))
    //                            stack.Push(targetNode);
    //                    }
    //        }
    //    }
    //}

    private IEnumerable<TNode> TraceInternalTrueDFS(
       IEnumerable<TNode> sources,
       bool traceInEdges,
       bool traceOutEdges,
       Func<TEdge, Direction, bool>? shouldTraceEdge = null,
       Func<TNode, bool>? shouldTraceNode = null
       )
    {
		IEnumerable<TNode> GetDeps(TNode node)
		{
			if (traceInEdges)
				foreach (var inEdge in GetInEdges(node))
					if (shouldTraceEdge == null || shouldTraceEdge(inEdge, Direction.Backward))
					{
						var sourceNode = GetSourceNode(inEdge);
						if (shouldTraceNode == null || shouldTraceNode(sourceNode))
						{
							yield return sourceNode;
						}
					}

			if (traceOutEdges)
				foreach (var outEdge in GetOutEdges(node))
					if (shouldTraceEdge == null || shouldTraceEdge(outEdge, Direction.Forward))
					{
						var targetNode = GetTargetNode(outEdge);
						if (shouldTraceNode == null || shouldTraceNode(targetNode))
						{
							yield return targetNode;
						}
					}
		}

		//var yielded = new HashSet<TNode>();
		var visited = new HashSet<TNode>();
		var stack = new Stack<(TNode, IEnumerator<TNode>)>();

		foreach (TNode source in sources)
		{
			if (visited.Add(source))
			{
				yield return source;

				stack.Push((source, GetDeps(source).GetEnumerator()));
			}

			while (stack.Any())
			{
				var (node, enumerator) = stack.Peek();
				bool depsPushed = false;

				while (enumerator.MoveNext())
				{
					var curr = enumerator.Current;
					if (visited.Add(curr))
					{
						yield return curr;

						stack.Push((curr, GetDeps(curr).GetEnumerator()));

						depsPushed = true;
						break;
					}
					//else if (!(allowCycles || yielded.Contains(curr)))
					//	throw new Exception($"Cycle detected at {curr}");
				}

				if (!depsPushed)
				{
					stack.Pop();
				}
			}
		}

		//var w = new GetDepsWrapper(this, traceInEdges, traceOutEdges, shouldTraceEdge, shouldTraceNode);
		//return sources.TopologicalOrder(w.GetDeps
		////n =>
		////    (
		////    traceInEdges ?
		////        GetInEdges(n)
		////        .Where(inEdge => edgeFilter == null || edgeFilter(inEdge, Direction.Backward))
		////        .Select(inEdge => GetSourceNode(inEdge))
		////        .Where(sourceNode => nodeFilter == null || nodeFilter(sourceNode))

		////        : Enumerable.Empty<TNode>())

		////    .Concat((
		////    traceOutEdges ?
		////        GetOutEdges(n)
		////        .Where(outEdge => edgeFilter == null || edgeFilter(outEdge, Direction.Forward))
		////        .Select(outEdge => GetSourceNode(outEdge))
		////        .Where(targetNode => nodeFilter == null || nodeFilter(targetNode))

		////        : Enumerable.Empty<TNode>())
		////    )

		//// allow cycles since the other 2 also does not fail on cycles...
		//, allowCycles: true);
	}

    /// <summary>
    /// Can't use yield return insode lambda
    /// </summary>
    //class GetDepsWrapper
    //{
    //    bool _traceInEdges;
    //    bool _traceOutEdges;
    //    Func<TEdge, Direction, bool>? _shouldTraceEdge = null;
    //    Func<TNode, bool>? _shouldTraceNode = null;
    //    GraphTracerBase<TNode, TEdge> _graphTracerBase;

    //    public GetDepsWrapper(GraphTracerBase<TNode, TEdge> graphTracerBase, bool traceInEdges, bool traceOutEdges, 
    //        Func<TEdge, Direction, bool>? shouldTraceEdge, Func<TNode, bool>? shouldTraceNode)
    //    {
    //        _graphTracerBase = graphTracerBase;
    //        _traceInEdges = traceInEdges;
    //        _traceOutEdges = traceOutEdges;
    //        _shouldTraceEdge = shouldTraceEdge;
    //        _shouldTraceNode = shouldTraceNode;
    //    }

    //    internal IEnumerable<TNode> GetDeps(TNode node)
    //    {
    //        if (_traceInEdges)
    //            foreach (var inEdge in _graphTracerBase.GetInEdges(node))
    //                if (_shouldTraceEdge == null || _shouldTraceEdge(inEdge, Direction.Backward))
    //                {
    //                    var sourceNode = _graphTracerBase.GetSourceNode(inEdge);
    //                    if (_shouldTraceNode == null || _shouldTraceNode(sourceNode))
    //                        yield return sourceNode;
    //                }

    //        if (_traceOutEdges)
    //            foreach (var outEdge in _graphTracerBase.GetOutEdges(node))
    //                if (_shouldTraceEdge == null || _shouldTraceEdge(outEdge, Direction.Forward))
    //                {
    //                    var targetNode = _graphTracerBase.GetTargetNode(outEdge);
    //                    if (_shouldTraceNode == null || _shouldTraceNode(targetNode))
    //                        yield return targetNode;
    //                }
    //    }
    //}

    private IEnumerable<TNode> TraceInternalBFS(
        IEnumerable<TNode> sources,
        bool traceInEdges,
        bool traceOutEdges,
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        )
    {
        HashSet<TNode> visited = new();
        Queue<TNode> queue = new();

        foreach (var source in sources)
        {
            if (visited.Add(source))
            {
                yield return source;
                queue.Enqueue(source);
            }
        }

        while (queue.Any())
        {
            var node = queue.Dequeue();

            if (traceInEdges)
                foreach (var inEdge in GetInEdges(node))
                    if (shouldTraceEdge == null || shouldTraceEdge(inEdge, Direction.Backward))
                    {
                        var sourceNode = GetSourceNode(inEdge);
                        if (shouldTraceNode == null || shouldTraceNode(sourceNode))
                        {
                            if (visited.Add(sourceNode))
                            {
                                yield return sourceNode;
                                queue.Enqueue(sourceNode);
                            }
                        }
                    }

            if (traceOutEdges)
                foreach (var outEdge in GetOutEdges(node))
                    if (shouldTraceEdge == null || shouldTraceEdge(outEdge, Direction.Forward))
                    {
                        var targetNode = GetTargetNode(outEdge);
                        if (shouldTraceNode == null || shouldTraceNode(targetNode))
                        {
                            if (visited.Add(targetNode))
                            {
                                yield return targetNode;
                                queue.Enqueue(targetNode);
                            }
                        }
                    }
        }
    }

    private IEnumerable<TNode> TraceInternal(
        IEnumerable<TNode> sources,
        bool traceInEdges,
        bool traceOutEdges,
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        )
    {
        return _algo switch
        {
            GraphTracerAlgo.BFS => TraceInternalBFS(sources, traceInEdges, traceOutEdges, shouldTraceEdge, shouldTraceNode),
//            GraphTracerAlgo.PseudoDFS => TraceInternalPseudoDFS(sources, traceInEdges, traceOutEdges, edgeFilter, nodeFilter),
            GraphTracerAlgo.DFS => TraceInternalTrueDFS(sources, traceInEdges, traceOutEdges, shouldTraceEdge, shouldTraceNode),
            _ => throw new Exception($"Unknown algo: {_algo}")
        };
    }
      
    /// <summary>
    /// Trace sources back as far as possible and return those without any inEdges.
    /// </summary>
    public IEnumerable<TNode> TraceStartNodes(IEnumerable<TNode> sources, 
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        ) => TraceBackward(sources, shouldTraceEdge, shouldTraceNode).Where(node => !GetInEdges(node).Any());

    /// <summary>
    /// Trace sources forward as far as possible and return those without any outEdges.
    /// </summary>
    public IEnumerable<TNode> TraceEndNodes(IEnumerable<TNode> sources, 
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        ) => TraceForward(sources, shouldTraceEdge, shouldTraceNode).Where(node => !GetOutEdges(node).Any());

    /// <summary>
    /// Trace source nodes in all directions and return all connected nodes.
    /// </summary>
    public IEnumerable<TNode> TraceCompletely(IEnumerable<TNode> sources, 
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        ) => TraceInternal(sources, true, true, shouldTraceEdge, shouldTraceNode);

    /// <summary>
    /// Trace all source nodes in forward direction, following only outEdges
    /// </summary>
    public IEnumerable<TNode> TraceForward(IEnumerable<TNode> sources,
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        ) => TraceInternal(sources, false, true, shouldTraceEdge, shouldTraceNode);

    /// <summary>
    /// Trace all source nodes in backward direction, folowing only inEdges
    /// </summary>
    public IEnumerable<TNode> TraceBackward(IEnumerable<TNode> sources, 
        Func<TEdge, Direction, bool>? shouldTraceEdge = null,
        Func<TNode, bool>? shouldTraceNode = null
        ) => TraceInternal(sources, true, false, shouldTraceEdge, shouldTraceNode);

    /// <summary>
    /// The distinct/unique union of TraceBackward and TraceForward
    /// </summary>
    public IEnumerable<TNode> TraceBackwardAndForward(IEnumerable<TNode> sources,
        Func<TEdge, Direction, bool>? shouldTraceEdge = null, 
        Func<TNode, bool>? shouldTraceNode = null
        )
    {
        HashSet<TNode> backwardVisited = new();
        foreach (var node in TraceBackward(sources, shouldTraceEdge))
        {
            yield return node;
            backwardVisited.Add(node);
        }
        foreach (var node in TraceForward(sources, shouldTraceEdge))
        {
            if (!backwardVisited.Contains(node))
                yield return node;
        }
    }
}
