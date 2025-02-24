using GraphAhoi.Extensions;

namespace GraphAhoi.Example;

class SomeEdge
{
    public SomeNode From;
    public SomeNode To;
}

class SomeNode
{
    public List<SomeEdge> Inlinks;
    public List<SomeEdge> Outlinks;
}

/// <summary>
/// Adapt to some node and edge that we don't have control over. 
/// If we had control, we could implement INode and IEdge, and then use GraphTracer directly.
/// But here we override 4 methods to adapt.
/// </summary>
class GraphTracerAdapter : GraphTracerBase<SomeNode, SomeEdge>
{
	public GraphTracerAdapter(GraphTracerAlgo algo) : base(algo)
	{ }

	protected override SomeNode GetSourceNode(SomeEdge e)
        => e.From;

    protected override SomeNode GetTargetNode(SomeEdge e)
        => e.To;

    protected override IEnumerable<SomeEdge> GetInEdges(SomeNode n)
        => n.Inlinks;

    protected override IEnumerable<SomeEdge> GetOutEdges(SomeNode n)
        => n.Outlinks;
}


internal class adaptExample
{
    public void example()
    {
        var nodes = new List<SomeNode>();

        var t = new GraphTracerAdapter(GraphTracerAlgo.BFS);
        var trace = t.TraceCompletely(nodes);

        
        

        var ordered = nodes.TopologicalOrder(n => n.Inlinks.Select(f => f.From));

       // GraphTracer.TraceCompletely(new[] { p });
    }
}
