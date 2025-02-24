using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphAhoi.Tests
{
	[TestClass]
	public class AdapterGraphTests
	{
		[TestMethod]
		public void TestBFS()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracerAdapter(GraphTracerAlgo.BFS);

			var root = nodes[0];
			var fwdTrace = gt.TraceForward([root]).ToList();

			//Level 0: 1
			//Level 1: 2-> 3-> 4
			//Level 2: 5-> 6-> 7
			//Level 3: 8-> 9

			Assert.AreEqual("1,2,3,4,5,6,7,8,9", GetIdString(fwdTrace));
		}

		private string? GetIdString(IEnumerable<Node> list)
		{
			List<string> ss = new();
			foreach (var n in list)
				ss.Add(n.Id.ToString());
			return string.Join(",", ss);
		}

		[TestMethod]
		public void TestDFS()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracerAdapter(GraphTracerAlgo.DFS);

			var root = nodes[0];
			var fwdTrace = gt.TraceForward([root]).ToList();

			//Branch 1: 1-> 2-> 5-> 8-> 7 > 9
			//Branch 2: 6
			//Branch 3: 3-> 4
		
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", GetIdString(fwdTrace));
		}

		private List<Node> CreateTestGraph()
		{
			List<Node> nodes = new();

			var n1 = new Node(1);
			var n2 = new Node(2);
			var n3 = new Node(3);
			var n4 = new Node(4);
			var n5 = new Node(5);
			var n6 = new Node(6);
			var n7 = new Node(7);
			var n8 = new Node(8);
			var n9 = new Node(9);

			nodes.AddRange([n1, n2, n3, n4, n5, n6, n7, n8, n9]);
			n1.AddOutNodes(n2, n3, n4);
			n3.AddOutNodes(n4);
			n2.AddOutNodes(n5, n6, n7);
			n5.AddOutNodes(n8);
			n8.AddOutNodes(n7);
			n7.AddOutNodes(n9);

			return nodes;
		}

		class GraphTracerAdapter : GraphTracerBase<Node, Edge>
		{
			public GraphTracerAdapter(GraphTracerAlgo algo) : base(algo)
			{
				
			}

			protected override IEnumerable<Edge> GetInEdges(Node node)
				=> node.InEdges;

			protected override IEnumerable<Edge> GetOutEdges(Node node)
				=> node.OutEdges;

			protected override Node GetSourceNode(Edge edge)
				=> edge.Source;

			protected override Node GetTargetNode(Edge edge)
				=> edge.Target;
		}

		class Node
		{
			public int Id { get; }

			public List<Edge> InEdges { get; } = new();
			public List<Edge> OutEdges { get; } = new();

			public Node(int id)
			{
				Id = id;
			}

			internal void AddOutNodes(params Node[] nodes)
			{
				foreach (var n in nodes)
				{
					var e = new Edge(this, n);
					OutEdges.Add(e);
					n.InEdges.Add(e);
				}
			}
		}

		class Edge
		{
			public Node Source { get; }

			public Node Target { get; }

			public Edge(Node src, Node dst)
			{
				Source = src;
				Target = dst;
			}
		}



	}
}
