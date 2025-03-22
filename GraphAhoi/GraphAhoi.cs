
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
	protected override IEnumerable<TEdge> GetInEdges(TNode node)
        => node.InEdges.Cast<TEdge>();

    protected override IEnumerable<TEdge> GetOutEdges(TNode node)
        => node.OutEdges.Cast<TEdge>();

    protected override TNode GetSourceNode(TEdge edge)
        => (TNode)edge.Source;

    protected override TNode GetTargetNode(TEdge edge)
        => (TNode)edge.Target;
}

public enum Traversal
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

public abstract class GraphTracerBase<TNode, TEdge> where TNode : notnull where TEdge : notnull
{

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
       Func<TEdge, TNode, Direction, bool>? shouldTrace = null
       )
    {
		IEnumerable<TNode> GetDeps(TNode node)
		{
			if (traceInEdges)
				foreach (var inEdge in GetInEdges(node))
				{
					var sourceNode = GetSourceNode(inEdge);
					if (shouldTrace == null || shouldTrace(inEdge, sourceNode, Direction.Backward))
					{
						yield return sourceNode;
					}
				}

			if (traceOutEdges)
				foreach (var outEdge in GetOutEdges(node))
				{
					var targetNode = GetTargetNode(outEdge);
					if (shouldTrace == null || shouldTrace(outEdge, targetNode, Direction.Forward))
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

	}

	private IEnumerable<TNode> TraceInternalBFS(
		IEnumerable<TNode> sources,
		bool traceInEdges,
		bool traceOutEdges,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
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
				{
					var sourceNode = GetSourceNode(inEdge);
					if (shouldTrace == null || shouldTrace(inEdge, sourceNode, Direction.Backward))
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
				{
					var targetNode = GetTargetNode(outEdge);
					if (shouldTrace == null || shouldTrace(outEdge, targetNode, Direction.Forward))
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
		Traversal t,
		IEnumerable<TNode> sources,
		bool traceInEdges,
		bool traceOutEdges,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		)
	{
		return t switch
		{
			Traversal.BFS => TraceInternalBFS(sources, traceInEdges, traceOutEdges, shouldTrace),
			//            GraphTracerAlgo.PseudoDFS => TraceInternalPseudoDFS(sources, traceInEdges, traceOutEdges, edgeFilter, nodeFilter),
			Traversal.DFS => TraceInternalTrueDFS(sources, traceInEdges, traceOutEdges, shouldTrace),
			_ => throw new Exception($"Unknown traversal: {t}")
		};
	}

	/// <summary>
	/// Trace sources back as far as possible and return those without any inEdges.
	/// </summary>
	public IEnumerable<TNode> TraceStartNodes(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		) => TraceBackward(t, sources, shouldTrace).Where(node => !GetInEdges(node).Any());

	/// <summary>
	/// Trace sources forward as far as possible and return those without any outEdges.
	/// </summary>
	public IEnumerable<TNode> TraceEndNodes(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		) => TraceForward(t, sources, shouldTrace).Where(node => !GetOutEdges(node).Any());

	/// <summary>
	/// Trace source nodes in all directions and return all connected nodes.
	/// </summary>
	public IEnumerable<TNode> TraceCompletely(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		) => TraceInternal(t, sources, true, true, shouldTrace);

	/// <summary>
	/// Trace all source nodes in forward direction, following only outEdges
	/// </summary>
	public IEnumerable<TNode> TraceForward(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		) => TraceInternal(t, sources, false, true, shouldTrace);

	/// <summary>
	/// Trace all source nodes in backward direction, folowing only inEdges
	/// </summary>
	public IEnumerable<TNode> TraceBackward(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		) => TraceInternal(t, sources, true, false, shouldTrace);

	/// <summary>
	/// The distinct/unique union of TraceBackward and TraceForward
	/// </summary>
	public IEnumerable<TNode> TraceBackwardAndForward(Traversal t, IEnumerable<TNode> sources,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null
		)
    {
        HashSet<TNode> backwardVisited = new();
        foreach (var node in TraceBackward(t, sources, shouldTrace))
        {
            yield return node;
            backwardVisited.Add(node);
        }
        foreach (var node in TraceForward(t, sources, shouldTrace))
        {
            if (!backwardVisited.Contains(node))
                yield return node;
        }
    }


	//public IEnumerable<IEnumerable<TEdge>> GetPathsForward(TNode earlier,
	//	TNode later,
	//	Func<TEdge, TNode, Direction, bool>? shouldTrace = null)
	//{

	//	foreach (var n in TraceForward([earlier], (edge, dir) =>
	//	{

	//		return true;
	//	},
	//	(node, dir) =>
	//	{

	//		return true;
	//	}))
	//	{
	//		if (n.Equals(later))
	//		{
	//			yield return new();
	//		}
	//	}
		
		
	//}

	/// <summary>
	/// TraceBackwardAndAggregateEdges (currently only support/use BFS))
	/// </summary>
	/// <typeparam name="TAgg"></typeparam>
	/// <param name="endNode"></param>
	/// <param name="defAgg">Default initial value</param>
	/// <param name="getEdgeAgg">Get aggregate value from edge</param>
	/// <param name="aggEdges">Aggregate the components of a path (the edges)</param>
	/// <param name="aggPaths">Aggregate the paths (a path in this context is a value returned from aggEdges)</param>
	/// <param name="shouldTrace"></param>
	/// <returns></returns>
	public IDictionary<TNode, TAgg> TraceBackwardAndAggregateEdges<TAgg>(TNode endNode,
		TAgg defAgg,
		Func<TEdge, TAgg> getEdgeAgg,
		Func<TAgg, TAgg, TAgg> aggEdges,
		Func<TAgg, TAgg, TAgg> aggPaths,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null)
	{
		var result = new Dictionary<TNode, TAgg>();
		var queue = new Queue<(TNode, TAgg)>();
		var edges = new HashSet<TEdge>();
		queue.Enqueue((endNode, defAgg));

		while (queue.Count > 0)
		{
			var (currentNode, currentShare) = queue.Dequeue();

			if (result.TryGetValue(currentNode, out var exAgg))
			{
				result[currentNode] = aggPaths(exAgg, currentShare);
			}
			else
			{
				result.Add(currentNode, currentShare);
			}

			foreach (var inEdge in GetInEdges(currentNode))
			{
				var srcNode = GetSourceNode(inEdge);
				if (shouldTrace == null || shouldTrace(inEdge, srcNode, Direction.Backward))
				{
					queue.Enqueue((GetSourceNode(inEdge), aggEdges(currentShare, getEdgeAgg(inEdge))));
				}
			}
		}

		return result;
	}


	public IEnumerable<IEnumerable<TEdge>> GetPathsBackward(TNode earlier,
	TNode later,
	Func<TEdge, Direction, bool>? shouldTraceEdge = null,
	Func<TNode, Direction, bool>? shouldTraceNode = null)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Get paths between two nodes.
	/// Currently uses BFS or PseudoDFS.
	/// Direction: if Forward it, traces from earlier to later, and vice versa. The result will be the same in both cases,
	/// but if you eg. know there may be a lot more node splits forward than node merged backwards, its faster to use backward tracing.
	/// </summary>
	public IEnumerable<IEnumerable<TEdge>> GetPaths(Traversal t, TNode earlier, 
		TNode later, 
		Direction direction,
		Func<TEdge, TNode, Direction, bool>? shouldTrace = null)
	{
		if (t == Traversal.DFS)
		{
			var start = direction == Direction.Forward ? earlier : later;
			var end = direction == Direction.Forward ? later : earlier;

			var stack = new Stack<(TNode, TEdge[], HashSet<TNode>)>();
			stack.Push((start, [], []));

			while (stack.Count > 0)
			{
				var (current, currentPath, visited) = stack.Pop();

				// If we reached the destination, add the path to the result
				if (current.Equals(end))
				{
					yield return currentPath;
					continue; // Move on to the next path
				}

				visited.Add(current);

				IEnumerable<TEdge> edgesToExplore = direction == Direction.Forward ?
					GetOutEdges(current) : GetInEdges(current);

				foreach (TEdge edge in edgesToExplore)
				{
					TNode nextNode = direction == Direction.Forward ? GetTargetNode(edge) : GetSourceNode(edge);
					if (shouldTrace == null || shouldTrace(edge, nextNode, direction))
					{
						if (!visited.Contains(nextNode))
						{
							// Create a new path for this branch using modern syntax
							TEdge[] newPath = [.. currentPath, edge];

							// Create a new visited set for this branch
							var newVisited = new HashSet<TNode>(visited);
							stack.Push((nextNode, newPath, newVisited));
						}
					}
				}
			}
		}
		else
		{

			var start = direction == Direction.Forward ? earlier : later;
			var end = direction == Direction.Forward ? later : earlier;

			var queue = new Queue<(TNode, TEdge[], HashSet<TNode>)>();
			queue.Enqueue((start, [], []));

			while (queue.Count > 0)
			{
				var (current, currentPath, visited) = queue.Dequeue();

				// If we reached the destination, add the path to the result
				if (current.Equals(end))
				{
					yield return currentPath;
					continue; // Move on to the next path
				}

				visited.Add(current);

				IEnumerable<TEdge> edgesToExplore = direction == Direction.Forward ?
					GetOutEdges(current) : GetInEdges(current);

				foreach (TEdge edge in edgesToExplore)
				{
					TNode nextNode = direction == Direction.Forward ? GetTargetNode(edge) : GetSourceNode(edge);
					if (shouldTrace == null || shouldTrace(edge, nextNode, direction))
					{
						if (!visited.Contains(nextNode))
						{
							// Create a new path for this branch
							TEdge[] newPath = [.. currentPath, edge];
							// Create a new visited set for this branch
							var newVisited = new HashSet<TNode>(visited);
							queue.Enqueue((nextNode, newPath, newVisited));
						}
					}
				}
			}
		}



	}
}
