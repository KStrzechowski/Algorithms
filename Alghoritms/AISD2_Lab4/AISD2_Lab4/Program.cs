using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    class QuarantineTestCase : TestCase
    {
        protected Graph G, G_copy;
        protected List<int> Z, Z_copy;
        protected List<int> expected;
        protected List<int> result;

        public QuarantineTestCase(Graph G, List<int> Z, List<int> expected, double timeLimit, string description)
            : base(timeLimit, null, description)
        {
            this.G = G;
            this.G_copy = G.Clone();
            this.Z = Z;
            this.Z_copy = new List<int>(Z);
            this.expected = expected;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).QuarantineTargets(Z, G);
        }


        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!G.IsEqual(G_copy))
                return (Result.WrongResult, "Wejściowy graf się zmienił!");
            if (result == null)
                return (Result.WrongResult, "Brak rozwiązania!");
            if (result.Count != expected.Count)
                return (Result.WrongResult, "Błędny rozmiar rozwiązania (jest " + result.Count.ToString() + ", powinno być " + expected.Count.ToString() + ")");
            for (int i = 0; i < result.Count - 1; i++)
                if (result[i] >= result[i + 1])
                    return (Result.WrongResult, "Rozwiązanie nie jest uporządkowane rosnąco!");
            for (int i = 0; i < result.Count; i++)
                if (result[i] != expected[i])
                    return (Result.WrongResult, "Błędne rozwiązanie!");
            return (Result.Success, "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
        }
    }

    class PatientZeroTestCase : QuarantineTestCase
    {
        public PatientZeroTestCase(Graph G, List<int> S, List<int> expected, double timeLimit, string description)
            : base(G, S, expected, timeLimit, description) { }


        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).PotentialPatientsZero(Z, G);
        }

    }

    class Lab04TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            TestSets["SmallQuarantime"] = makeSmallQuarantine();
            TestSets["BigQuarantime"] = makeBigQuarantine();
            TestSets["SmallPatientZero"] = makeSmallPatientZero();
            TestSets["BigPatientZero"] = makeBigPatientZero();
        }

        TestSet makeSmallQuarantine()
        {
            TestSet set = new TestSet(new Lab04(), "Część I, testy laboratoryjne małe");
            {
                set.TestCases.Add(new QuarantineTestCase(
                    G: new AdjacencyListsGraph<AVLAdjacencyList>(false, 10),
                    Z: new List<int> { 0, 3, 5, 7 },
                    expected: new List<int> { 0, 3, 5, 7 },
                    timeLimit: 1,
                    description: "Same wierzcholki izolowane"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(false, 6);
                star.AddEdge(0, 1, 4);
                star.AddEdge(0, 2, 2);
                star.AddEdge(0, 3, 7);
                star.AddEdge(0, 4, 3);
                star.AddEdge(0, 5, 9);
                set.TestCases.Add(new QuarantineTestCase(
                    G: star,
                    Z: new List<int> { 1 },
                    expected: new List<int> { 0, 1, 3, 5 },
                    timeLimit: 1,
                    description: "Gwiazda, chory liść"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(false, 7);
                path.AddEdge(0, 1, 3);
                path.AddEdge(1, 2, 5);
                path.AddEdge(2, 3, 3);
                path.AddEdge(3, 4, 4);
                path.AddEdge(4, 5, 6);
                path.AddEdge(5, 6, 3);
                set.TestCases.Add(new QuarantineTestCase(
                    G: path,
                    Z: new List<int> { 0, 6 },
                    expected: new List<int> { 0, 1, 2, 4, 5, 6 },
                    timeLimit: 1,
                    description: "Ścieżka, chore końce"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(false, 7);
                path.AddEdge(0, 1, 3);
                path.AddEdge(1, 2, 3);
                path.AddEdge(2, 3, 3);
                path.AddEdge(3, 4, 3);
                path.AddEdge(4, 5, 3);
                path.AddEdge(5, 6, 3);
                set.TestCases.Add(new QuarantineTestCase(
                    G: path,
                    Z: new List<int> { 0, 6 },
                    expected: new List<int> { 0, 1, 5, 6 },
                    timeLimit: 1,
                    description: "Ścieżka, chore końce, jednoczesne spotkania"));
            }
            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 8);
                Gr.AddEdge(0, 2, 4);
                Gr.AddEdge(0, 4, 3);
                Gr.AddEdge(0, 6, 3);
                Gr.AddEdge(2, 3, 2);
                Gr.AddEdge(4, 5, 4);
                Gr.AddEdge(6, 7, 2);
                Gr.AddEdge(3, 1, 6);
                Gr.AddEdge(5, 1, 5);
                Gr.AddEdge(7, 1, 3);
                set.TestCases.Add(new QuarantineTestCase(
                    G: Gr,
                    Z: new List<int> { 0 },
                    expected: new List<int> { 0, 1, 2, 3, 4, 5, 6 },
                    timeLimit: 1,
                    description: "Trzy ścieżki od 0 do 1"));
            }
            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 13);
                Gr.AddEdge(0, 1, 2);
                Gr.AddEdge(0, 2, 1);
                Gr.AddEdge(0, 3, 2);
                Gr.AddEdge(1, 4, 1);
                Gr.AddEdge(2, 4, 4);
                Gr.AddEdge(3, 4, 1);
                Gr.AddEdge(4, 5, 3);
                Gr.AddEdge(4, 6, 3);
                Gr.AddEdge(4, 7, 5);
                Gr.AddEdge(5, 8, 7);
                Gr.AddEdge(6, 8, 4);
                Gr.AddEdge(7, 8, 6);
                Gr.AddEdge(8, 9, 5);
                Gr.AddEdge(8, 10, 7);
                Gr.AddEdge(8, 11, 3);
                Gr.AddEdge(9, 12, 8);
                Gr.AddEdge(10, 12, 4);
                Gr.AddEdge(11, 12, 5);
                set.TestCases.Add(new QuarantineTestCase(
                    G: Gr,
                    Z: new List<int> { 0 },
                    expected: new List<int> { 0, 1, 2, 3, 4, 5, 7, 8, 10 },
                    timeLimit: 1,
                    description: "Trzy diamenty"));
            }

            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 6);
                Gr.AddEdge(0, 1, 5);
                Gr.AddEdge(1, 2, 3);
                Gr.AddEdge(1, 5, 2);
                Gr.AddEdge(1, 3, 6);
                Gr.AddEdge(2, 3, 2);
                Gr.AddEdge(2, 5, 1);
                Gr.AddEdge(3, 4, 4);
                Gr.AddEdge(4, 5, 1);
                set.TestCases.Add(new QuarantineTestCase(
                    G: Gr,
                    Z: new List<int> { 0, 3 },
                    expected: new List<int> { 0, 1, 2, 3, 4 },
                    timeLimit: 1,
                    description: "Dwóch zarażonych"));
            }

            return set;
        }


        TestSet makeBigQuarantine()
        {
            TestSet set = new TestSet(new Lab04(), "Część I, testy laboratoryjne duże");
            {
                int n = 80000;
                Graph M = new AdjacencyListsGraph<AVLAdjacencyList>(false, 2 * n);
                List<int> infected = new List<int>();
                List<int> result = new List<int>();
                Random r = new Random(7);
                for (int i = 0; i < n; i++)
                {
                    M.AddEdge(2 * i, 2 * i + 1, r.NextDouble());
                    if (r.Next(200) < 1)
                    {
                        result.Add(2 * i);
                        result.Add(2 * i + 1);
                        infected.Add(r.Next(3) == 2 ? 2 * i : 2 * i + 1);
                    }
                }
                set.TestCases.Add(new QuarantineTestCase(
                    G: M,
                    Z: infected,
                    expected: result,
                    timeLimit: 5,
                    description: "Duże skojarzenie"));
            }
            {
                int n = 20000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);
                Random r = new Random(13);

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        int dest = r.Next(n);
                        if (dest != i)
                            Gr.AddEdge(i, dest, r.Next(1, 10));
                    }

                List<int> infected = new List<int> { 10, 50, 71, 300, 700, 1000, 1500, 1700, 1899, 1930, 1987 };


                List<int> result = new List<int>() { 10, 34, 50, 71, 132, 143, 162, 166, 226, 246, 300, 412, 503, 580, 596, 608,
                    700, 804, 1000, 1120, 1166, 1332, 1365, 1386, 1415, 1465, 1500, 1510, 1567, 1700, 1844, 1854, 1899, 1914, 1930,
                    1962, 1969, 1987, 2039, 2203, 2246, 2392, 2396, 2444, 2587, 2589, 2867, 3083, 3101, 3116, 3185, 3390, 3462, 3491,
                    3642, 3680, 3864, 3879, 3975, 4021, 4106, 4158, 4196, 4253, 4533, 4667, 4727, 4774, 4891, 5034, 5269, 5326, 5413,
                    5535, 5579, 5628, 5648, 5654, 5805, 5827, 5966, 6115, 6281, 6346, 6893, 6954, 6997, 7034, 7097, 7323, 7324, 7525,
                    7528, 7559, 7580, 7786, 7912, 7943, 8132, 8201, 8319, 8363, 8484, 8549, 8570, 8685, 8758, 8801, 9113, 9116, 9220,
                    9227, 9240, 9339, 9703, 9738, 9841, 9898, 9920, 10169, 10232, 10315, 10404, 10470, 10639, 10757, 10902, 10963,
                    10971, 11035, 11137, 11226, 11277, 11358, 11360, 11367, 11446, 11447, 11463, 11527, 11550, 11688, 11703, 11709,
                    11743, 11785, 11821, 11848, 11874, 11887, 11975, 11982, 11983, 12041, 12046, 12199, 12383, 12513, 12604, 12633,
                    12711, 12757, 12819, 12851, 12949, 13043, 13071, 13115, 13121, 13180, 13318, 13351, 13544, 13583, 13655, 13680,
                    13725, 13774, 13993, 14067, 14073, 14177, 14216, 14297, 14331, 14367, 14426, 14489, 14505, 14570, 14575, 14697,
                    14916, 14918, 14951, 15029, 15142, 15171, 15402, 15415, 15554, 15667, 15703, 15711, 15793, 15798, 15831, 15855,
                    15878, 16051, 16064, 16092, 16274, 16375, 16386, 16496, 16549, 16554, 16585, 16598, 16705, 16771, 16987, 17018,
                    17044, 17220, 17248, 17286, 17291, 17301, 17336, 17338, 17531, 17778, 17997, 18060, 18390, 18454, 18484, 18534,
                    18580, 18737, 18801, 18852, 18854, 18863, 18974, 18981, 19239, 19243, 19357, 19414, 19676, 19750, 19941, 19947, 19956 };


                set.TestCases.Add(new QuarantineTestCase(
                    G: Gr,
                    Z: infected,
                    expected: result,
                    timeLimit: 2,
                    description: "Losowy rzadki graf"));
            }

            {
                int n = 40000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);

                List<int> infected = new List<int>() { 5 * 127 };
                List<int> result = new List<int>();

                for (int i = 0; i < n - 1; i++)
                {
                    Gr.AddEdge((127 * i) % n, (127 * i + 127) % n, i / 8.0);
                    if (i >= 3)
                        result.Add((127 * i + 127) % n);
                }
                result.Sort();

                set.TestCases.Add(new QuarantineTestCase(
                    G: Gr,
                    Z: infected,
                    expected: result,
                    timeLimit: 2,
                    description: "Długa ścieżka"));
            }

            return set;
        }

        TestSet makeSmallPatientZero()
        {
            TestSet set = new TestSet(new Lab04(), "Część II, testy laboratoryjne małe");
            {
                set.TestCases.Add(new PatientZeroTestCase(
                    G: new AdjacencyListsGraph<AVLAdjacencyList>(false, 10),
                    S: new List<int> { 0, 3, 5, 7 },
                    expected: new List<int> { },
                    timeLimit: 5,
                    description: "Same wierzcholki izolowane"));
            }
            {
                Graph star = new AdjacencyListsGraph<AVLAdjacencyList>(false, 6);
                star.AddEdge(0, 1, 4);
                star.AddEdge(0, 2, 2);
                star.AddEdge(0, 3, 7);
                star.AddEdge(0, 4, 3);
                star.AddEdge(0, 5, 9);
                set.TestCases.Add(new PatientZeroTestCase(
                    G: star,
                    S: new List<int> { 1 },
                    expected: new List<int> { 0, 1, 2, 4 },
                    timeLimit: 5,
                    description: "Gwiazda, chory liść"));
            }
            {
                Graph path = new AdjacencyListsGraph<AVLAdjacencyList>(false, 7);
                path.AddEdge(0, 1, 3);
                path.AddEdge(1, 2, 5);
                path.AddEdge(2, 3, 3);
                path.AddEdge(3, 4, 4);
                path.AddEdge(4, 5, 6);
                path.AddEdge(5, 6, 3);
                set.TestCases.Add(new PatientZeroTestCase(
                    G: path,
                    S: new List<int> { 0, 6 },
                    expected: new List<int> { },
                    timeLimit: 5,
                    description: "Ścieżka, chore końce"));
            }
            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 13);
                Gr.AddEdge(0, 1, 2);
                Gr.AddEdge(0, 2, 1);
                Gr.AddEdge(0, 3, 2);
                Gr.AddEdge(1, 4, 1);
                Gr.AddEdge(2, 4, 4);
                Gr.AddEdge(3, 4, 1);
                Gr.AddEdge(4, 5, 3);
                Gr.AddEdge(4, 6, 3);
                Gr.AddEdge(4, 7, 5);
                Gr.AddEdge(5, 8, 7);
                Gr.AddEdge(6, 8, 4);
                Gr.AddEdge(7, 8, 6);
                Gr.AddEdge(8, 9, 5);
                Gr.AddEdge(8, 10, 7);
                Gr.AddEdge(8, 11, 3);
                Gr.AddEdge(9, 12, 8);
                Gr.AddEdge(10, 12, 4);
                Gr.AddEdge(11, 12, 5);
                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: new List<int> { 5, 7, 10, 12 },
                    expected: new List<int> { 1, 3, 4, 6, 8, 9, 11 },
                    timeLimit: 5,
                    description: "Trzy diamenty"));
            }
            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 6);
                Gr.AddEdge(0, 1, 5);
                Gr.AddEdge(1, 2, 3);
                Gr.AddEdge(1, 5, 2);
                Gr.AddEdge(1, 3, 6);
                Gr.AddEdge(2, 3, 2);
                Gr.AddEdge(2, 5, 1);
                Gr.AddEdge(3, 4, 4);
                Gr.AddEdge(4, 5, 1);
                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: new List<int> { 0, 3 },
                    expected: new List<int> { 0, 1, 2, 3, 4, 5 },
                    timeLimit: 5,
                    description: "Dwóch zarażonych"));
            }
            {
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, 10);
                Gr.AddEdge(0, 6, 3);
                Gr.AddEdge(0, 8, 3);
                Gr.AddEdge(1, 7, 3);
                Gr.AddEdge(2, 6, 3);
                Gr.AddEdge(2, 7, 3);
                Gr.AddEdge(2, 8, 3);
                Gr.AddEdge(3, 7, 3);
                Gr.AddEdge(3, 8, 3);
                Gr.AddEdge(3, 9, 3);
                Gr.AddEdge(4, 6, 3);
                Gr.AddEdge(4, 7, 3);
                Gr.AddEdge(5, 6, 3);
                Gr.AddEdge(5, 9, 3);
                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: new List<int> { 2, 4 },
                    expected: new List<int> { 6, 7 },
                    timeLimit: 5,
                    description: "Wszystkie spotkania jednocześnie"));
            }

            return set;
        }

        TestSet makeBigPatientZero()
        {
            TestSet set = new TestSet(new Lab04(), "Część II, testy laboratoryjne duże");

            {
                int n = 40000;
                Graph Gr = new AdjacencyListsGraph<AVLAdjacencyList>(false, n);

                List<int> infected = new List<int>() { 5 * 127, 8 * 127, (30 * 127) % n, (1000 * 127) % n, (30000 * 127) % n };
                for (int i = 35000; i < 35020; i++)
                    infected.Add(i);
                List<int> result = new List<int>() { 0, 127, 2 * 127, 3 * 127, 4 * 127, 5 * 127, 6 * 127 };

                for (int i = 0; i < n - 1; i++)
                {
                    Gr.AddEdge((127 * i) % n, (127 * i + 127) % n, i / 8.0);
                }

                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: infected,
                    expected: result,
                    timeLimit: 6,
                    description: "Długa ścieżka"));
            }

            {
                int n = 4000;
                RandomGraphGenerator rgg = new RandomGraphGenerator(13);
                Random r = new Random(17);
                Graph Gr = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<AVLAdjacencyList>), n, 1.0 / 300, 1, 10, false);

                List<int> infected = new List<int>();

                for (int i = 0; i < 30; i++)
                {
                    int inf = r.Next(n);
                    if (!infected.Contains(inf))
                        infected.Add(inf);
                }
                infected.Sort();

                List<int> result = new List<int> { 18, 23, 41, 73, 76, 78, 95, 146, 162, 179, 185, 188, 192, 202, 228, 238,
                    293, 321, 364, 404, 415, 495, 516, 528, 549, 553, 620, 655, 687, 695, 699, 735, 749, 789, 806, 807, 850,
                    912, 917, 919, 1001, 1058, 1080, 1109, 1135, 1171, 1172, 1194, 1207, 1249, 1255, 1264, 1268, 1285, 1330,
                    1390, 1410, 1529, 1536, 1546, 1555, 1560, 1584, 1615, 1626, 1662, 1674, 1717, 1720, 1737, 1775, 1803, 1953,
                    1964, 1968, 1970, 1981, 2019, 2057, 2062, 2144, 2145, 2165, 2167, 2212, 2278, 2296, 2340, 2354, 2362, 2369,
                    2400, 2402, 2407, 2449, 2500, 2518, 2565, 2686, 2694, 2698, 2714, 2730, 2762, 2766, 2768, 2813, 2854, 2868,
                    2878, 2890, 2937, 2945, 2958, 3010, 3021, 3031, 3034, 3058, 3081, 3091, 3099, 3124, 3151, 3179, 3182, 3190,
                    3193, 3198, 3258, 3289, 3317, 3368, 3417, 3450, 3487, 3488, 3494, 3505, 3521, 3570, 3595, 3633, 3639, 3668,
                    3685, 3705, 3720, 3733, 3761, 3766, 3791, 3805, 3809, 3830, 3833, 3856, 3882, 3886, 3927, 3983 };

                //foreach (int i in result)
                //    Console.Write(i.ToString() + ", ");

                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: infected,
                    expected: result,
                    timeLimit: 5,
                    description: "Losowy niezbyt gęsty graf"));
            }


            {
                int n = 5000;
                RandomGraphGenerator rgg = new RandomGraphGenerator(13);
                Random r = new Random(31);
                Graph Gr = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), n, 1.0 / 500, 1, 10, false);

                List<int> infected = new List<int>();

                for (int i = 0; i < 30; i++)
                {
                    int inf = r.Next(n);
                    if (!infected.Contains(inf))
                        infected.Add(inf);
                }
                infected.Sort();

                List<int> result = new List<int> { 128, 145, 286, 289, 291, 374, 470, 490, 540, 569, 600, 641, 690, 746, 834,
                    866, 871, 943, 1008, 1010, 1014, 1096, 1268, 1295, 1427, 1434, 1498, 1504, 1623, 1682, 1707, 1807, 1876,
                    1944, 2035, 2080, 2134, 2158, 2159, 2170, 2393, 2444, 2464, 2568, 2654, 2742, 2887, 3076, 3085, 3146, 3153,
                    3257, 3291, 3312, 3416, 3539, 3564, 3598, 3741, 3750, 3773, 3932, 3963, 4077, 4269, 4298, 4426, 4673, 4693,
                    4735, 4821, 4842, 4911 };

                //foreach (int i in result)
                //    Console.Write(i.ToString() + ", ");

                set.TestCases.Add(new PatientZeroTestCase(
                    G: Gr,
                    S: infected,
                    expected: result,
                    timeLimit: 4,
                    description: "Inny losowy niezbyt gęsty graf, w naiwnej reprezentacji listowej"));
            }

            return set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Lab04TestModule lab04test = new Lab04TestModule();
            lab04test.PrepareTestSets();
            foreach (var ts in lab04test.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
        }
    }
}
