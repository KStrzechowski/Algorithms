using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    class WidePathTestCase : TestCase
    {
        protected Graph G, G_copy;
        protected int start;
        protected int end;
        protected int expected;
        protected List<int> result;
        protected int[] weights;
        protected int maxWeight;

        public WidePathTestCase(Graph G, int start, int end, int expected, double timeLimit, string description)
            : base(timeLimit, null, description)
        {
            this.G = G;
            this.G_copy = G.Clone();
            this.start = start;
            this.end = end;
            this.expected = expected;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab06)prototypeObject).WidePath(G, start, end);
        }


        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!G.IsEqual(G_copy))
                return (Result.WrongResult, "Wejściowy graf się zmienił!");
            if (result == null)
                return (Result.WrongResult, "Brak rozwiązania!");
            if (result.Count == 0 && expected == -1)
                return (Result.Success,
                    "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
            if (result.Count == 0)
                return (Result.WrongResult, "Nie znaleziono istniejącej ścieżki");
            if (result.Count > 0 && expected == -1)
                return (Result.WrongResult, "Ścieżka nie istnieje, a zwrócono listę rozmiaru " + result.Count.ToString());
            if (result[0] != start)
                return (Result.WrongResult, "Zły wierzchołek początkowy");
            if (result[result.Count - 1] != end)
                return (Result.WrongResult, "Zły wierzchołek końcowy");
            var numericResult = WidestEdgeInPath(result);
            if (numericResult == Int32.MinValue)
                return (Result.WrongResult, "Podana ścieżka nie istnieje");
            if (numericResult < expected)
                return (Result.WrongResult, "Zbyt wąska ścieżka");
            if (numericResult > expected)
                return (Result.WrongResult, "Znaleziona ściażka lepsza od oczekiwanej. Zgłoś to prowadzącemu");
            return (Result.Success,
                "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
        }

        protected int WidestEdgeInPath(List<int> verticesList)
        {
            int minWeight = Int32.MaxValue;
            for (int i = 0; i < verticesList.Count - 1; i++)
            {
                var weight = G.GetEdgeWeight(verticesList[i], verticesList[i + 1]);
                if (weight.IsNaN())
                    return Int32.MinValue;
                if (weight < minWeight)
                    minWeight = (int)weight;
            }

            return minWeight;
        }
    }

    class WeightWidePathTestCase : WidePathTestCase
    {
        public WeightWidePathTestCase(Graph G, int start, int end, int[] weights, int maxWeight, int expected,
            double timeLimit, string description)
            : base(G, start, end, expected, timeLimit, description)
        {
            this.weights = weights;
            this.maxWeight = maxWeight;
        }


        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab06)prototypeObject).WeightedWidePath(G, start, end, weights, maxWeight);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!G.IsEqual(G_copy))
                return (Result.WrongResult, "Wejściowy graf się zmienił!");
            if (result == null)
                return (Result.WrongResult, "Brak rozwiązania!");
            if (result.Count == 0 && expected == Int32.MinValue)
                return (Result.Success,
                    "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
            if (result.Count == 0)
                return (Result.WrongResult, "Nie znaleziono istniejącej ścieżki");
            if (result.Count > 0 && expected == Int32.MinValue)
                return (Result.WrongResult, "Ścieżka nie istnieje, a zwrócono listę rozmiaru " + result.Count.ToString());
            if (result[0] != start)
                return (Result.WrongResult, "Zły wierzchołek początkowy");
            if (result[result.Count - 1] != end)
                return (Result.WrongResult, "Zły wierzchołek końcowy");
            var numericResult = WeightedResult(result);
            if (numericResult == Int32.MinValue)
                return (Result.WrongResult, "Podana ścieżka nie istnieje" + numericResult);
            if (numericResult < expected)
                return (Result.WrongResult, "Zbyt wąska ścieżka (" + numericResult + " zamiast " + expected.ToString() + ")");
            if (numericResult > expected)
                return (Result.WrongResult, "Popsuły się testy :o" + numericResult);
            return (Result.Success,
                "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
        }

        private int WeightedResult(List<int> verticesList)
        {
            int minWeight = Int32.MaxValue;
            int verticesWeightSum = 0;
            for (int i = 0; i < verticesList.Count - 1; i++)
            {
                verticesWeightSum += weights[verticesList[i]];
                var weight = G.GetEdgeWeight(verticesList[i], verticesList[i + 1]);
                if (weight.IsNaN())
                    return Int32.MinValue;
                if (weight < minWeight)
                    minWeight = (int)weight;
            }

            return minWeight - verticesWeightSum;
        }
    }

    class Lab06TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            TestSets["SmallWidePath "] = makeSmallWidePath();
            TestSets["BigWidePath"] = makeBigWidePath();
            TestSets["SmallWeightedWidePath"] = makeSmallWeightWidePath();
            TestSets["BigWeightedWidePath"] = makeBigWeightWidePath();
        }

        TestSet makeSmallWidePath()
        {
            TestSet set = new TestSet(new Lab06(), "Część I, testy laboratoryjne małe");
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(true, 7);
                path.AddEdge(0, 1, 10);
                path.AddEdge(1, 3, 8);
                path.AddEdge(0, 2, 9);
                path.AddEdge(2, 3, 9);
                set.TestCases.Add(new WidePathTestCase(
                    G: path,
                    start: 0,
                    end: 3,
                    expected: 9,
                    timeLimit: 1,
                    description: "Prosty graf"));
            }
            {
                set.TestCases.Add(new WidePathTestCase(
                    G: new AdjacencyListsGraph<AVLAdjacencyList>(true, 10),
                    start: 0,
                    end: 7,
                    expected: -1,
                    timeLimit: 1,
                    description: "Same wierzcholki izolowane"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 6);
                star.AddEdge(0, 1, 4);
                star.AddEdge(0, 2, 2);
                star.AddEdge(0, 3, 7);
                star.AddEdge(0, 4, 3);
                star.AddEdge(0, 5, 6);
                star.AddEdge(1, 2, 2);
                star.AddEdge(1, 3, 7);
                star.AddEdge(1, 4, 3);
                star.AddEdge(1, 5, 9);
                star.AddEdge(2, 1, 2);
                star.AddEdge(2, 3, 7);
                star.AddEdge(2, 4, 3);
                star.AddEdge(2, 5, 9);
                star.AddEdge(3, 1, 2);
                star.AddEdge(3, 2, 7);
                star.AddEdge(3, 4, 3);
                star.AddEdge(3, 5, 9);
                set.TestCases.Add(new WidePathTestCase(
                    G: star,
                    start: 0,
                    end: 5,
                    expected: 7,
                    timeLimit: 1,
                    description: "Gęsty graf z prostą ścieżką"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(true, 7);
                path.AddEdge(0, 1, 3);
                path.AddEdge(1, 2, 5);
                path.AddEdge(2, 3, 3);
                path.AddEdge(3, 4, 4);
                path.AddEdge(4, 5, 6);
                path.AddEdge(5, 6, 3);
                set.TestCases.Add(new WidePathTestCase(
                    G: path,
                    start: 0,
                    end: 6,
                    expected: 3,
                    timeLimit: 1,
                    description: "Ścieżka"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(true, 8);
                path.AddEdge(0, 1, 1);
                path.AddEdge(0, 5, 10);
                path.AddEdge(0, 3, 3);
                path.AddEdge(2, 1, 5);
                path.AddEdge(3, 2, 7);
                path.AddEdge(3, 1, 4);
                path.AddEdge(4, 3, 8);
                path.AddEdge(4, 1, 3);
                path.AddEdge(5, 4, 9);
                path.AddEdge(5, 1, 2);

                set.TestCases.Add(new WidePathTestCase(
                    G: path,
                    start: 0,
                    end: 1,
                    expected: 5,
                    timeLimit: 1,
                    description: "Pięciokąt ze środkiem z drogą po obwodzie"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(true, 8);
                path.AddEdge(0, 1, 9);
                path.AddEdge(0, 5, 8);
                path.AddEdge(1, 2, 7);
                path.AddEdge(1, 3, 9);
                path.AddEdge(2, 3, 12);
                path.AddEdge(3, 4, 9);
                path.AddEdge(4, 5, 8);

                set.TestCases.Add(new WidePathTestCase(
                    G: path,
                    start: 0,
                    end: 5,
                    expected: 8,
                    timeLimit: 1,
                    description: "Dwie ścieżki o równej szerokości"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(true, 12);
                path.AddEdge(0, 1, 9);
                path.AddEdge(0, 5, 8);
                path.AddEdge(1, 2, 7);
                path.AddEdge(1, 3, 9);
                path.AddEdge(2, 3, 12);
                path.AddEdge(3, 4, 9);
                path.AddEdge(4, 5, 8);
                path.AddEdge(6, 7, 9);
                path.AddEdge(6, 11, 8);
                path.AddEdge(7, 8, 7);
                path.AddEdge(7, 9, 9);
                path.AddEdge(8, 9, 12);
                path.AddEdge(9, 10, 9);
                path.AddEdge(10, 11, 8);

                set.TestCases.Add(new WidePathTestCase(
                    G: path,
                    start: 0,
                    end: 11,
                    expected: -1,
                    timeLimit: 1,
                    description: "Graf bez ścieżki"));
            }
            return set;
        }

        TestSet makeBigWidePath()
        {
            TestSet set = new TestSet(new Lab06(), "Część I, testy laboratoryjne duże");
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(13);

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 100));
                    }

                set.TestCases.Add(new WidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 1000,
                    expected: 36,
                    timeLimit: 1,
                    description: "Rzadki losowy graf"));
            }
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(13);

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < 20; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 100));
                    }

                set.TestCases.Add(new WidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 1000,
                    expected: 92,
                    timeLimit: 1,
                    description: "Gęstszy graf losowy"));
            }
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(13);

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < 60; j++)
                    {
                        int dest = r.Next(n - 1);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 100));
                    }

                set.TestCases.Add(new WidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 19999,
                    expected: -1,
                    timeLimit: 5,
                    description: "Jeszcze gęstszy graf losowy z wierzchołkiem izolowanym"));
            }
            return set;
        }

        TestSet makeSmallWeightWidePath()
        {
            TestSet set = new TestSet(new Lab06(), "Część II, testy laboratoryjne małe");
            {
                int[] weights = { 0, 3, 12, 2, 1, 1 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: new AdjacencyListsGraph<AVLAdjacencyList>(true, 10),
                    start: 0,
                    end: 2,
                    weights: weights,
                    maxWeight: 20,
                    expected: Int32.MinValue,
                    timeLimit: 5,
                    description: "Same wierzcholki izolowane"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 6);
                star.AddEdge(0, 1, 5);
                star.AddEdge(0, 2, 20);
                star.AddEdge(1, 3, 5);
                star.AddEdge(2, 3, 20);
                star.AddEdge(3, 4, 5);
                star.AddEdge(3, 5, 20);

                int[] weights = { 0, 3, 12, 2, 0, 0 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 4,
                    weights: weights,
                    maxWeight: 20,
                    expected: 0,
                    timeLimit: 5,
                    description: "Rozdział po złączeniu wersja 1"));
            }

            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 6);
                star.AddEdge(0, 1, 5);
                star.AddEdge(0, 2, 20);
                star.AddEdge(1, 3, 5);
                star.AddEdge(2, 3, 20);
                star.AddEdge(3, 4, 5);
                star.AddEdge(3, 5, 20);

                int[] weights = { 0, 3, 12, 2, 0, 0 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 5,
                    weights: weights,
                    maxWeight: 20,
                    expected: 6,
                    timeLimit: 5,
                    description: "Rozdział po złączeniu wersja 2"));
            }

            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 8);
                star.AddEdge(0, 1, 5);
                star.AddEdge(0, 2, 20);
                star.AddEdge(1, 3, 5);
                star.AddEdge(2, 3, 20);
                star.AddEdge(3, 4, 5);
                star.AddEdge(3, 5, 20);
                star.AddEdge(6, 3, 2);
                star.AddEdge(0, 6, 3);
                star.AddEdge(3, 7, 2);

                int[] weights = { 0, 3, 12, 2, 0, 0, 1, 0 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 7,
                    weights: weights,
                    maxWeight: 20,
                    expected: -1,
                    timeLimit: 5,
                    description: "Trzy ścieżki prowadzące do celu wersja 1"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 8);
                star.AddEdge(0, 1, 5);
                star.AddEdge(0, 2, 20);
                star.AddEdge(1, 3, 5);
                star.AddEdge(2, 3, 20);
                star.AddEdge(3, 4, 5);
                star.AddEdge(3, 5, 20);
                star.AddEdge(6, 3, 2);
                star.AddEdge(0, 6, 3);
                star.AddEdge(3, 7, 2);

                int[] weights = { 0, 3, 12, 2, 0, 0, 1, 0 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 4,
                    weights: weights,
                    maxWeight: 20,
                    expected: 0,
                    timeLimit: 5,
                    description: "Trzy ścieżki prowadzące do celu wersja 2"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 19);
                star.AddEdge(0, 1, 10);
                star.AddEdge(1, 2, 9);
                star.AddEdge(2, 3, 8);
                star.AddEdge(3, 4, 7);
                star.AddEdge(4, 5, 7);
                star.AddEdge(5, 6, 6);
                star.AddEdge(6, 7, 5);
                star.AddEdge(7, 8, 4);
                star.AddEdge(8, 9, 3);
                star.AddEdge(0, 18, 4);
                star.AddEdge(18, 17, 4);
                star.AddEdge(17, 16, 4);
                star.AddEdge(16, 15, 4);
                star.AddEdge(15, 14, 4);
                star.AddEdge(14, 13, 4);
                star.AddEdge(13, 12, 4);
                star.AddEdge(12, 10, 4);
                star.AddEdge(10, 9, 4);
                int[] weights = { 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 9,
                    weights: weights,
                    maxWeight: 10,
                    expected: -4,
                    timeLimit: 5,
                    description: "Dwie równie długie ścieżki, jedna z nich się zwężą"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(true, 19);
                star.AddEdge(0, 1, 10);
                star.AddEdge(1, 2, 9);
                star.AddEdge(2, 3, 8);
                star.AddEdge(3, 4, 7);
                star.AddEdge(4, 5, 7);
                star.AddEdge(5, 6, 6);
                star.AddEdge(6, 7, 5);
                star.AddEdge(7, 8, 4);
                star.AddEdge(8, 9, 3);
                star.AddEdge(0, 11, 10);
                star.AddEdge(11, 12, 9);
                star.AddEdge(12, 13, 8);
                star.AddEdge(13, 14, 7);
                star.AddEdge(14, 15, 7);
                star.AddEdge(15, 16, 6);
                star.AddEdge(16, 17, 5);
                star.AddEdge(17, 18, 4);
                star.AddEdge(18, 9, 3);
                int[] weights = { 0, 13, 10, 5, 3, 17, 13, 11, 1, 0, 0, 20, 19, 11, 4, 4, 4, 4, 8 };
                set.TestCases.Add(new WeightWidePathTestCase(
                    G: star,
                    start: 0,
                    end: 9,
                    weights: weights,
                    maxWeight: 10,
                    expected: -70,
                    timeLimit: 5,
                    description: "Dwie równie szerokie ścieżki, jedna ma większe wagi od drugiej"));
            }
            return set;
        }
        TestSet makeBigWeightWidePath()
        {
            TestSet set = new TestSet(new Lab06(), "Część II, testy laboratoryjne duże");
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(16);
                int[] weights = new int[20000];
                for (int i = 0; i < n; i++)
                {
                    weights[i] = r.Next(100);
                    for (int j = 0; j < 2; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 10));
                    }
                }

                set.TestCases.Add(new WeightWidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 19000,
                    expected: -638,
                    weights: weights,
                    maxWeight: 10,
                    timeLimit: 5,
                    description: "Rzadki losowy graf"));
            }
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(16);
                int[] weights = new int[20000];
                for (int i = 0; i < n; i++)
                {
                    weights[i] = r.Next(10);
                    for (int j = 0; j < 2; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 100));
                    }
                }

                set.TestCases.Add(new WeightWidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 19000,
                    expected: -52,
                    weights: weights,
                    maxWeight: 100,
                    timeLimit: 15,
                    description: "Rzadki losowy graf, duże wagi"));
            }
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(true, n);
                Random r = new Random(13);
                int[] weights = new int[20000];
                for (int i = 0; i < n; i++)
                {
                    weights[i] = r.Next(10);
                    for (int j = 0; j < 100; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 5));
                    }
                }

                set.TestCases.Add(new WeightWidePathTestCase(
                    G: Gr,
                    start: 0,
                    end: 1000,
                    expected: -1,
                    weights: weights,
                    maxWeight: 5,
                    timeLimit: 30,
                    description: "Gęsty losowy graf, małe wagi"));
            }
            return set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Lab06TestModule Lab06test = new Lab06TestModule();
            Lab06test.PrepareTestSets();
            foreach (var ts in Lab06test.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
        }
    }
}