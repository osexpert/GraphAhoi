using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphAhoi.Extensions;
using System.Text;

namespace GraphAhoi.Tests
{
	[TestClass]
	public class Tests
    {
		[TestMethod]
		public void TestStartNodes()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceStartNodes(nodes));
			Assert.AreEqual("1", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceStartNodes(nodes));
			Assert.AreEqual("1", str2);
		}

		[TestMethod]
		public void TestEndNodes()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceEndNodes(nodes));
			Assert.AreEqual("4,6,9", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceEndNodes(nodes));
			Assert.AreEqual("9,6,4", str2);
		}

		[TestMethod]
		public void TestForwardNodes()
		{
			List<Node> nodes = CreateTestGraph();

			var n7 = nodes[6];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceForward([n7]));
			Assert.AreEqual("7,9", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceForward([n7]));
			Assert.AreEqual("7,9", str2);
		}

		[TestMethod]
		public void TestBackdNodes()
		{
			List<Node> nodes = CreateTestGraph();

			var n7 = nodes[6];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceBackward([n7]));
			Assert.AreEqual("7,2,8,1,5", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceBackward([n7]));
			Assert.AreEqual("7,2,1,8,5", str2);
		}

		[TestMethod]
		public void TestBackAndFwddNodes()
		{
			List<Node> nodes = CreateTestGraph();

			var n7 = nodes[6];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceBackwardAndForward([n7]));
			Assert.AreEqual("7,2,8,1,5,9", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceBackwardAndForward([n7]));
			Assert.AreEqual("7,2,1,8,5,9", str2);
		}

		[TestMethod]
		public void TestrCompleteTrace()
		{
			List<Node> nodes = CreateTestGraph();

			var n7 = nodes[6];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceCompletely([n7]));
			Assert.AreEqual("7,2,8,9,1,5,6,3,4", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceCompletely([n7]));
			Assert.AreEqual("7,2,1,3,4,5,8,6,9", str2);
		}


		/// <summary>
		/// https://www.banterly.net/2020/02/09/why-what-you-have-been-thaught-about-dfs-is-wrong-at-least-partially/
		/// </summary>
		[TestMethod]
		public void TestrCompleteTrace_Root()
		{
			List<Node> nodes = CreateTestGraph();

			var n1 = nodes[0];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceCompletely([n1]));
			Assert.AreEqual("1,2,3,4,5,6,7,8,9", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceCompletely([n1]));
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", str2);
		}

		/// <summary>
		/// https://www.banterly.net/2020/02/09/why-what-you-have-been-thaught-about-dfs-is-wrong-at-least-partially/
		/// </summary>
		[TestMethod]
		public void TestrForward_Root()
		{
			List<Node> nodes = CreateTestGraph();

			var n1 = nodes[0];

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);
			var str = GetIdString(gt.TraceForward([n1]));
			Assert.AreEqual("1,2,3,4,5,6,7,8,9", str);

			var gt2 = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);
			var str2 = GetIdString(gt2.TraceForward([n1]));
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", str2);
		}

		[TestMethod]
		public void TestTopoDFS()
		{
			List<Node> nodes = CreateTestGraph();
			// depend on inlinks
			var list = nodes.TopologicalOrder(deps => deps.InEdges.Select(ie => ie.Source)).ToList();
			Assert.AreEqual("1,2,3,4,5,6,8,7,9", GetIdString(list));
			var list2 = list.TopologicalOrder(deps => deps.OutEdges.Select(ie => ie.Target)).ToList();
			Assert.AreEqual("9,7,8,5,6,2,4,3,1", GetIdString(list2));
			var list3 = list2.TopologicalOrder(deps => deps.InEdges.Select(ie => ie.Source)).ToList();
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", GetIdString(list3));
			var list4 = list3.TopologicalOrder(deps => deps.OutEdges.Select(ie => ie.Target)).ToList();
			Assert.AreEqual("9,7,8,5,6,2,4,3,1", GetIdString(list4));
			var list5 = list4.TopologicalOrder(deps => deps.InEdges.Select(ie => ie.Source)).ToList();
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", GetIdString(list5));
		}

		private string? GetIdString(IEnumerable<Node> list)
		{
			List<string> ss = new();
			foreach (var n in list)
				ss.Add(n.Id.ToString());
			return string.Join(",", ss);
		}

		[TestMethod]
		public async Task TestTopoDFSAsync()
		{
			List<Node> nodes = CreateTestGraph();
			// depend on inlinks
			var list = nodes.TopologicalOrderAsync(deps => GetInNodes(deps)).ToBlockingEnumerable();
			Assert.AreEqual("1,2,3,4,5,6,8,7,9", GetIdString(list));
			var list2 = list.TopologicalOrderAsync(deps => GetOutNodes(deps)).ToBlockingEnumerable();
			Assert.AreEqual("9,7,8,5,6,2,4,3,1", GetIdString(list2));
			var list3 = list2.TopologicalOrderAsync(deps => GetInNodes(deps)).ToBlockingEnumerable();
			Assert.AreEqual("1,2,5,8,7,9,6,3,4", GetIdString(list3));
			var list4 = list3.TopologicalOrderAsync(deps => GetOutNodes(deps)).ToBlockingEnumerable();
			Assert.AreEqual("9,7,8,5,6,2,4,3,1", GetIdString(list4));
		}


		private static async IAsyncEnumerable<Node> GetOutNodes(Node deps)
		{
			foreach (var v in deps.OutEdges.Select(ie => ie.Target))
				yield return v;
		}

		private static async IAsyncEnumerable<Node> GetInNodes(Node deps)
		{
			foreach (var v in deps.InEdges.Select(ie => ie.Source))
				yield return v;
		}

		[TestMethod]
		public void TestBFS()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.BFS);

			var root = nodes[0];
			var fwdTrace = gt.TraceForward([root]).ToList();

			//Level 0: 1
			//Level 1: 2-> 3-> 4
			//Level 2: 5-> 6-> 7
			//Level 3: 8-> 9

			Assert.AreEqual("1,2,3,4,5,6,7,8,9", GetIdString(fwdTrace));
		}

		[TestMethod]
		public void TestDFS()
		{
			List<Node> nodes = CreateTestGraph();

			var gt = new GraphTracer<Node, Edge>(GraphTracerAlgo.DFS);

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

			nodes.AddRange([n1,n2,n3,n4,n5,n6,n7,n8,n9]);
			n1.AddOutNodes(n2, n3, n4);
			n3.AddOutNodes(n4);
			n2.AddOutNodes(n5, n6, n7);
			n5.AddOutNodes(n8);
			n8.AddOutNodes(n7);
			n7.AddOutNodes(n9);

			return nodes;
		}



		class Node : INode
		{
			public int Id { get; }

			public List<Edge> InEdges { get; } = new();
			public List<Edge> OutEdges { get; } = new();

			IEnumerable<IEdge> INode.InEdges => InEdges;
			IEnumerable<IEdge> INode.OutEdges => OutEdges;

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

		class Edge : IEdge
		{
			public Node Source { get; }

			public Node Target { get; }

			INode IEdge.Source => Source;
			INode IEdge.Target => Target;

			public Edge(Node src, Node dst)
			{
				Source = src;
				Target = dst;
			}
		}



	}
}
