﻿using GraphAhoi.Extensions;

namespace GraphAhoi.Example;

class SomeEdge
{
    public SomeNode From;
    public SomeNode To;
    public int EdgeData;
}

class SomeNode
{
    public List<SomeEdge> Inlinks;
    public List<SomeEdge> Outlinks;
    public int NodeData;
}

/// <summary>
/// Adapt to some node and edge that we don't have control over. 
/// If we had control, we could implement INode and IEdge, and then use GraphTracer directly.
/// But here we override 4 methods to adapt.
/// </summary>
class GraphTracerAdapter : GraphTracerBase<SomeNode, SomeEdge>
{
    protected override SomeNode GetSourceNode(SomeEdge e) => e.From;
    protected override SomeNode GetTargetNode(SomeEdge e) => e.To;
    protected override IEnumerable<SomeEdge> GetInEdges(SomeNode n) => n.Inlinks;
    protected override IEnumerable<SomeEdge> GetOutEdges(SomeNode n) => n.Outlinks;
}

internal class RetrofitExample
{
    public void Example()
    {
        // All nodes in graph
        var graphNodes = GetGraphNodes();
        // Selection: one or more nodes from the graph
        var selection = new List<SomeNode>() { graphNodes[42], graphNodes[666] };

        // Create tracer-adapter and select algo
        var tracer = new GraphTracerAdapter();

        // Trace the selection
        var trace1 = tracer.TraceCompletely(Traversal.BFS, selection);

        var trace2 = tracer.TraceBackward(Traversal.BFS, selection);

        var trace3 = tracer.TraceForward(Traversal.BFS, selection);
        var trace4 = tracer.TraceBackwardAndForward(Traversal.BFS, selection);

		// Control tracing with delegates
		var trace5 = tracer.TraceCompletely(Traversal.BFS, selection,
			shouldTrace: (edg, node, _) => edg.EdgeData == 42 || node.NodeData == 666);
		var trace6 = tracer.TraceCompletely(Traversal.BFS, selection,
			shouldTrace: (edg, node, direction) => 
				(direction == Direction.Forward && edg.EdgeData == 42) || (node.NodeData == 666 && direction == Direction.Backward));

        // Extension method
        var ordered = selection.TopologicalOrder(n => n.Inlinks.Select(f => f.From));
    }

	private List<SomeNode> GetGraphNodes()
    {
        throw new NotImplementedException();
    }

    public async Task ExampleAsync()
    {
        var nodes = new List<SomeNode>();
        // Extension method async
        var ordered = nodes.TopologicalOrderAsync(GetDepsAsync);
        await foreach (var o in ordered)
        {
            Console.WriteLine(o);
        }
    }

    private async IAsyncEnumerable<SomeNode> GetDepsAsync(SomeNode node)
    {
        foreach (var x in node.Inlinks.Select(f => f.From))
            yield return x;
    }
}
