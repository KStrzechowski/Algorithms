using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASD;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColoringTestCase : TestCase
    {
        protected readonly Graph inputGraph;
        protected readonly int expectedAnswer;

        protected (int numberOfColors, int[] coloring) result;

        public GraphColoringTestCase(Graph graph, int answer, double timeLimit, string description)
            : base(timeLimit, null, description)
        {
            inputGraph = graph;
            expectedAnswer = answer;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((GraphColorer)prototypeObject).FindBestColoring(inputGraph);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result res = Result.NotPerformed;
            string msg = "";
            string timeInfo = " (czas " + PerformanceTime.ToString("F4") + " limit " + TimeLimit.ToString("F4") + ")";

            if (result.numberOfColors != expectedAnswer)
            {
                res = Result.WrongResult;
                msg = "Błędna liczba kolorów (jest " + result.numberOfColors.ToString() + ", powinno być " + expectedAnswer.ToString() + ")";
            }
            else
            {
                if (result.coloring == null)
                {
                    res = Result.WrongResult;
                    msg = "Liczba kolorów ok, brak kolorowania";
                }
                else if (result.coloring.Length != inputGraph.VerticesCount)
                {
                    res = Result.WrongResult;
                    msg = "Liczba kolorów ok, błędne kolorowanie (tablica wynikowa rozmiaru " + result.coloring.Length.ToString() + " zamiast " + inputGraph.VerticesCount.ToString() + ")";
                }
                else
                {
                    SortedSet<int> colors = new SortedSet<int>();
                    for (int i = 0; i < result.coloring.Length; i++)
                        if (!colors.Contains(result.coloring[i]))
                            colors.Add(result.coloring[i]);
                    if (colors.Count != expectedAnswer)
                    {
                        res = Result.WrongResult;
                        msg = "Liczba kolorów ok, błędne kolorowanie (używa " + colors.Count.ToString() + " kolorów zamiast zadeklarowanych " + expectedAnswer.ToString() + ")";
                    }
                    else
                    {
                        bool badEdge = false;
                        for (int u = 0; u < inputGraph.VerticesCount; u++)
                            foreach (Edge e in inputGraph.OutEdges(u))
                                if (result.coloring[e.From] == result.coloring[e.To])
                                {
                                    badEdge = true;
                                    res = Result.WrongResult;
                                    msg = "Liczba kolorów ok, błędne kolorowanie (sąsiadujące wierzchołki " + e.From.ToString() + " i " + e.To.ToString() + " mają ten sam kolor " + result.coloring[e.To].ToString() + ")";
                                }
                        if(!badEdge)
                        {
                            res = Result.Success;
                            msg = "OK" + timeInfo;
                        }
                    }
                }
            }

            return (res, msg);
        }
    }

    public class Lab10TestModule : TestModule
    {

        public override double ScoreResult()
        {
            return 2.5;
        }

        public override void PrepareTestSets()
        {
            var smallTestCases = BuildSmallTestCases();
            var largeTestCases = BuildLargeTestCases();
            var homeTestCases = BuildHomeTestCases();

            TestSets["SmallTestCases"] = smallTestCases;
            TestSets["LargeTestCases"] = largeTestCases;
            TestSets["HomeTestCases"] = homeTestCases;
        }

        private TestSet BuildSmallTestCases()
        {
            var tests = new TestSet(new GraphColorer(), "Małe testy");
            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 5);
                tests.TestCases.Add(new GraphColoringTestCase(G, 1, 1, "Zbiór niezależny"));
            }

            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 5);
                G.AddEdge(2, 0);
                G.AddEdge(2, 1);
                G.AddEdge(2, 3);
                G.AddEdge(2, 4);
                tests.TestCases.Add(new GraphColoringTestCase(G, 2, 1, "Gwiazda K_1,4"));
            }


            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 4);
                G.AddEdge(0, 1);
                G.AddEdge(0, 2);
                G.AddEdge(0, 3);
                G.AddEdge(1, 2);
                G.AddEdge(1, 3);
                G.AddEdge(2, 3);
                tests.TestCases.Add(new GraphColoringTestCase(G, 4, 1, "Klika K_4"));
            }


            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 5);
                G.AddEdge(0, 1);
                G.AddEdge(1, 2);
                G.AddEdge(2, 3);
                G.AddEdge(3, 4);
                G.AddEdge(4, 0);
                tests.TestCases.Add(new GraphColoringTestCase(G, 3, 1, "Cykl C_5"));
            }


            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 7);
                G.AddEdge(0, 1);
                G.AddEdge(1, 2);
                G.AddEdge(2, 3);
                G.AddEdge(3, 4);
                G.AddEdge(4, 5);
                G.AddEdge(5, 6);
                G.AddEdge(6, 0);
                G.AddEdge(0, 2);
                G.AddEdge(1, 3);
                G.AddEdge(2, 4);
                G.AddEdge(3, 5);
                G.AddEdge(4, 6);
                G.AddEdge(5, 0);
                G.AddEdge(6, 1);
                tests.TestCases.Add(new GraphColoringTestCase(G, 4, 1, "Kwadrat cyklu C_7"));
            }


            {
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 10);
                G.AddEdge(0, 2);
                G.AddEdge(1, 2);
                G.AddEdge(3, 4);
                G.AddEdge(2, 4);
                G.AddEdge(4, 5);
                G.AddEdge(4, 6);
                G.AddEdge(6, 7);
                G.AddEdge(6, 8);
                G.AddEdge(8, 9);
                tests.TestCases.Add(new GraphColoringTestCase(G, 2, 1, "Drzewo na 10 wierzchołkach"));
            }

            return tests;
        }

        private TestSet BuildLargeTestCases()
        {
            var tests = new TestSet(new GraphColorer(), "Duże testy");

            {
                int n = 1500;
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 2 * n);
                for (int i = 0; i < 2 * n; i++)
                    G.AddEdge(i, (i + 1) % (2 * n));
                tests.TestCases.Add(new GraphColoringTestCase(G, 2, 2, "Duży parzysty cykl"));
            }
            {
                int n = 1000;
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, 2 * n + 1);
                for (int i = 0; i < 2 * n + 1; i++)
                    G.AddEdge(i, (i + 1) % (2 * n + 1));
                tests.TestCases.Add(new GraphColoringTestCase(G, 3, 2, "Duży nieparzysty cykl"));
            }
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(123);
                Graph G = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<AVLAdjacencyList>), 54, 0.2);
                tests.TestCases.Add(new GraphColoringTestCase(G, 5, 4, "Graf losowy 1"));
            }
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(124);
                Graph G = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<AVLAdjacencyList>), 32, 0.4);
                tests.TestCases.Add(new GraphColoringTestCase(G, 6, 6, "Graf losowy 2"));
            }
            {
                int n = 80;
                int m = 300;
                Random rand = new Random(123);
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);
                for(int i=0;i<m;i++)
                {
                    int u = (int)Math.Floor(Math.Sqrt(rand.Next(n * n)));
                    int v = (int)Math.Floor(Math.Sqrt(rand.Next(n * n)));
                    if (u != v)
                        G.AddEdge(u, v);
                }
                RandomGraphGenerator rgg = new RandomGraphGenerator(125);
                G = rgg.Permute(G);
                tests.TestCases.Add(new GraphColoringTestCase(G, 5, 5, "Graf losowy z nierównymi stopniami"));
            }

           /* {
                int n = 100;
                int m = 600;
                Random rand = new Random(123);
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);
                for (int i = 0; i < m; i++)
                {
                    int u = rand.Next(n);
                    int v = rand.Next(n);
                    if (u != v)
                    {
                        bool ok = true;
                        for (int k = 0; k < n; k++)
                            if (!double.IsNaN(G.GetEdgeWeight(u, k)) && !double.IsNaN(G.GetEdgeWeight(v, k)))
                                ok = false;
                        if (ok)
                            G.AddEdge(u, v);
                    }
                }
                RandomGraphGenerator rgg = new RandomGraphGenerator(125);
                G = rgg.Permute(G);
                tests.TestCases.Add(new GraphColoringTestCase(G, 4, 2, "Losowy graf bez trójkątów"));
            }*/


            {
                int n = 8;
                int m = 30;
                Random rand = new Random(125);
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);
                for (int i = 0; i < m; i++)
                {
                    int u = rand.Next(n);
                    int v = rand.Next(n);
                    if (u != v)
                        G.AddEdge(u, v);
                }
                G = Mycielskian(Mycielskian(G));
                RandomGraphGenerator rgg = new RandomGraphGenerator(125);
                G = rgg.Permute(G);
                tests.TestCases.Add(new GraphColoringTestCase(G, 6, 2, "Mycielskian losowego grafu"));
            }


            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(126);
                Graph G = rgg.UndirectedGraph(typeof(AdjacencyMatrixGraph), 12, 0.9);
                tests.TestCases.Add(new GraphColoringTestCase(G, 10, 2, "Losowy bardzo gęsty graf"));
            }

            // Tu się zaczynają największe i najtrudniejsze testy
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(127);
                Graph G = rgg.UndirectedGraph(typeof(AdjacencyMatrixGraph), 35, 0.9);
                tests.TestCases.Add(new GraphColoringTestCase(G, 16, 2, "Losowy bardzo gęsty i duży graf"));
            }

            {
                int n = 49;
                int d = 2;
                Random rand = new Random(127);
                Graph G = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);
                for (int v = 0; v < n; v++)
                {
                    for (int i = 0; i < d; i++)
                    {
                        int u = rand.Next(n);
                        if (u != v)
                            G.AddEdge(u, v);
                    }
                }
                G = Kwadrat(G);
                tests.TestCases.Add(new GraphColoringTestCase(G, 8, 8, "Kwadrat losowego rzadkiego grafu"));
            }


            {
                int n = 80;
                Graph G = new AdjacencyMatrixGraph(false, n);
                for (int v = 0; v < n; v++)
                    for (int u = v + 1; u < n; u++)
                        G.AddEdge(u, v);
                tests.TestCases.Add(new GraphColoringTestCase(G, n, 2, "Duża klika"));
            }

            return tests;
        }

        private TestSet BuildHomeTestCases()
        {
            var tests = new TestSet(new GraphColorer(), "Utajnione testy");
            return tests;
        }

        Graph Mycielskian(Graph G)
        {
            Graph H = G.IsolatedVerticesGraph(false, 2 * G.VerticesCount + 1);
            for (int v = 0; v < G.VerticesCount; v++)
            {
                foreach (Edge e in G.OutEdges(v))
                    if (e.To > e.From)
                    {
                        H.AddEdge(e.From, e.To);
                        H.AddEdge(e.From, e.To + G.VerticesCount);
                        H.AddEdge(e.From + G.VerticesCount, e.To);
                    }
                H.AddEdge(2 * G.VerticesCount, v + G.VerticesCount);
            }
            return H;
        }
        Graph Kwadrat(Graph G)
        {
            Graph H = G.IsolatedVerticesGraph(false, G.VerticesCount);
            for (int v = 0; v < G.VerticesCount; v++)
            {
                List<int> neighbors = new List<int>();
                foreach (Edge e in G.OutEdges(v))
                {
                    H.AddEdge(e);
                    neighbors.Add(e.To);
                }
                for (int i = 0; i < neighbors.Count; i++)
                    for (int j = i + 1; j < neighbors.Count; j++)
                        H.AddEdge(neighbors[i], neighbors[j]);
            }
            return H;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var verifyTimeLimits = false;

            var testModule = new Lab10TestModule();
            testModule.PrepareTestSets();
            foreach (var ts in testModule.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: verifyTimeLimits);
        }
    }
}
