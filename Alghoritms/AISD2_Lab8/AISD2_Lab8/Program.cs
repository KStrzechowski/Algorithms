using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    class WinnersTestCase : TestCase
    {
        protected int[] points, points_clone;
        protected int[,] vs, vs_clone;
        protected List<int> expectedTeams;
        protected List<int> result;
        protected int[][] finalResult;

        public WinnersTestCase(int[] points, int[,] vs, List<int> expectedTeams, double timeLimit, string description)
            : base(timeLimit, null, description)
        {
            this.points = points;
            this.points_clone = (int[])points.Clone();
            this.vs = vs;
            this.vs_clone = (int[,])vs.Clone();
            this.expectedTeams = expectedTeams;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab08)prototypeObject).FindPossibleWinners(points, vs, out finalResult);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (result == null)
            {
                return (Result.WrongResult, "Brak rozwiązania!");
            }

            if (result.Count == expectedTeams.Count && !expectedTeams.SequenceEqual(result))
            {
                return (Result.WrongResult, "Wynik zły, ale odpowiedniej długości.");
            }

            if (result.Count < expectedTeams.Count)
            {
                return (Result.WrongResult, "Zbyt krótka lista drużyn.");
            }

            if (result.Count > expectedTeams.Count)
            {
                return (Result.WrongResult, "Zbyt długa lista drużyn.");
            }

            if (expectedTeams.SequenceEqual(result))
            {
                int n = points.Length;
                int[] remaining = new int[n];
                for (int i = 0; i < n; ++i)
                {
                    for (int j = 0; j < n; ++j)
                    {
                        remaining[i] += vs[i, j];
                    }
                }
                for (int team = 0; team < n; ++team)
                {
                    var result = finalResult[team];
                    if (expectedTeams.Contains(team) && result == null)
                    {
                        return (Result.WrongResult, "OK lista drużyn. Pusta tabela końcowa dla wygrywającej drużyny");
                    }

                    if (!expectedTeams.Contains(team) && result != null)
                    {
                        return (Result.WrongResult, "OK lista drużyn. Zwrócona tabela dla przegrywającej drużyny");
                    }

                    if (!expectedTeams.Contains(team)) continue;

                    int pointsToAcquire = 0;
                    for (int i = 0; i < n; ++i)
                        for (int j = i + 1; j < n; ++j)
                            pointsToAcquire += vs[i, j];
                    int[] pointsWonByTeam = new int[n];
                    int maximumPoints = 0;
                    int pointsWonByAllTeams = 0;
                    for (int i = 0; i < n; ++i)
                    {
                        pointsWonByTeam[i] = result[i] - points[i];
                        maximumPoints = Math.Max(maximumPoints, result[i]);
                        if (pointsWonByTeam[i] < 0)
                            return (Result.WrongResult, "OK lista drużyn. Drużyna kończy z mniejszą ilością punktów niż zaczynała");
                        int possiblePointsToGet = 0;
                        for (int j = 0; j < n; ++j)
                        {
                            possiblePointsToGet += vs[i, j];
                        }
                        if (pointsWonByTeam[i] > possiblePointsToGet)
                            return (Result.WrongResult, "OK lista drużyn. Drużyna zdobywa więcej punktów niż pozostało meczy");
                        pointsWonByAllTeams += pointsWonByTeam[i];
                    }
                    if (maximumPoints != result[team])
                        return (Result.WrongResult, "OK lista drużyn. Wygrywająca drużyna nie jest liderem w tabeli końcowej");

                    if (pointsWonByAllTeams != pointsToAcquire)
                        return (Result.WrongResult, "OK lista drużyn. Suma zdobytych punktów inna niż liczba pozostałych spotkań");

                    return (Result.Success,
                        "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
                }
            }
            return (Result.WrongResult, "Zły wynik");
        }

    }

    class SatisfactionTestCase : TestCase
    {
        protected int[] points, points_clone;
        protected int[,] vs, vs_clone;
        protected int[,] fondness, fondness_clone;
        protected int[] expectedSatisfaction;
        protected int[] result;


        public SatisfactionTestCase(int[] points, int[,] vs, int[,] fondness, int[] expectedSatisfaction, double timeLimit, string description)
            : base(timeLimit, null, description)
        {
            this.points = points;
            this.points_clone = (int[])points.Clone();
            this.vs = vs;
            this.vs_clone = (int[,])vs.Clone();
            this.fondness = fondness;
            this.fondness_clone = (int[,])fondness.Clone();
            this.expectedSatisfaction = expectedSatisfaction;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab08)prototypeObject).FindMaxSatisfaction(points, vs, fondness);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (result == null)
            {
                return (Result.WrongResult, "Brak rozwiązania!");
            }

            if (result.Length != expectedSatisfaction.Length)
            {
                return (Result.WrongResult, "Rozwiązanie złej długości");
            }

            if (result.Any(v => v < 0 && v != -1))
            {
                return (Result.WrongResult, "Ujemna satysfakcja");
            }

            if (expectedSatisfaction.Zip(result, (e, r) => (e, r)).Any(pair => pair.ToTuple().Item1 == -1 && pair.ToTuple().Item2 != -1))
            {
                return (Result.WrongResult, "Policzona satysfakcja dla drużyny, która nie może wygrać");
            }

            if (expectedSatisfaction.Zip(result, (e, r) => (e, r)).Any(pair => pair.ToTuple().Item1 != -1 && pair.ToTuple().Item2 == -1))
            {
                return (Result.WrongResult, "Niepoliczona satysfakcja dla drużyny, która może wygrać");
            }

            if (expectedSatisfaction.Zip(result, (e, r) => (e, r)).Any(pair => pair.ToTuple().Item1 < pair.ToTuple().Item2))
            {
                return (Result.WrongResult, "Zbyt duża satysfakcja dla którejś z drużyn");
            }

            if (expectedSatisfaction.Zip(result, (e, r) => (e, r)).Any(pair => pair.ToTuple().Item1 > pair.ToTuple().Item2))
            {
                return (Result.WrongResult, "Zbyt mała satysfakcja dla którejś z drużyn");
            }

            return (Result.Success,
                "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");


        }

    }


    class Lab08TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            TestSets["SmallFindWinner "] = makeFindWinnerSmall();
            TestSets["BigFindWinner"] = makeFindWinnerBig();
            TestSets["SmallFindSatisfaction"] = makeSmallFindSatisfaction();
            TestSets["BigFindSatisfaction"] = makeBigFindSatisfaction();

        }

        TestSet makeFindWinnerSmall()
        {
            TestSet set = new TestSet(new Lab08(), "Część I, testy laboratoryjne małe");
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 2, 2, 2, 2 },
                    vs: new int[,] {
                        { 0, 2, 2, 2},
                        { 2, 0, 2, 2},
                        { 2, 2, 0, 2},
                        { 2, 2, 2, 0}
                        },
                    expectedTeams: new List<int> { 0, 1, 2, 3 },
                    timeLimit: 1,
                    description: "Początek fazy grupowej MŚ"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 0, 15, 14, 13 },
                    vs: new int[,] {
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0}
                        },
                    expectedTeams: new List<int> { 1 },
                    timeLimit: 1,
                    description: "Koniec sezonu, jeden zwycięzca"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 15, 15, 0, 15 },
                    vs: new int[,] {
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0}
                    },
                    expectedTeams: new List<int> { 0, 1, 3 },
                    timeLimit: 1,
                    description: "Koniec sezonu, zwycięzcy ex aequo"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 15, 15 },
                    vs: new int[,] {
                        { 0, 2 },
                        { 2, 0 },
                    },
                    expectedTeams: new List<int> { 0, 1 },
                    timeLimit: 1,
                    description: "Dwudrużynowa liga"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 7 },
                    vs: new int[,] {
                        { 0 }
                    },
                    expectedTeams: new List<int> { 0 },
                    timeLimit: 1,
                    description: "Jednodrużynowa liga"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 15, 14, 13, 12 },
                    vs: new int[,] {
                        { 0, 0, 0, 0},
                        { 0, 0, 5, 5},
                        { 0, 5, 0, 5},
                        { 0, 5, 5, 0}
                    },
                    expectedTeams: new List<int> { 1, 2, 3 },
                    timeLimit: 1,
                    description: "Lider przegrywa"));
            }
            {
                set.TestCases.Add(new WinnersTestCase(
                    points: new int[] { 15, 14, 13, 12 },
                    vs: new int[,] {
                        { 0, 2, 4, 1},
                        { 2, 0, 5, 1},
                        { 4, 5, 0, 2},
                        { 1, 1, 2, 0}
                    },
                    expectedTeams: new List<int> { 0, 1, 2 },
                    timeLimit: 1,
                    description: "Ostatnia drużyna przegrywa pomimo szans na pierwszy rzut oka"));
            }
            return set;
        }

        TestSet makeFindWinnerBig()
        {
            TestSet set = new TestSet(new Lab08(), "Część I, testy laboratoryjne losowe");
            {
                Random r = new Random(16);
                int n = 50;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(100);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(2);
                        vs[i, j] = vs[j, i] = remaining;
                    }
                }

                set.TestCases.Add(new WinnersTestCase(
                    points: points,
                    vs: vs,
                    expectedTeams: new List<int> { 2, 9, 10, 15, 17, 19, 25,
                    26, 32, 34, 36, 37, 39, 40, 47},
                    timeLimit: 1,
                    description: "Duży test I"));
            }
            {
                Random r = new Random(19);
                int n = 75;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(150);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(3);
                        vs[i, j] = vs[j, i] = remaining;
                    }
                }

                set.TestCases.Add(new WinnersTestCase(
                    points: points,
                    vs: vs,
                    expectedTeams: new List<int> { 0, 2, 3, 4, 7, 8, 10, 14, 20,
                    21, 22, 23, 28, 29, 30, 34, 36, 38, 40, 41, 44, 47,
                    48, 49, 54, 55, 58, 60, 62, 64, 66, 69, 72, 73 },
                    timeLimit: 10,
                    description: "Duży test II"));
            }
            {
                Random r = new Random(19);
                int n = 80;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(500);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(20);
                        vs[i, j] = vs[j, i] = remaining;
                    }
                }
                set.TestCases.Add(new WinnersTestCase(
                    points: points,
                    vs: vs,
                    expectedTeams: Enumerable.Range(0, n).ToList(),
                    timeLimit: 15,
                    description: "Duży test III, wszyscy mogą wygrać"));
            }
            return set;
        }

        TestSet makeSmallFindSatisfaction()
        {
            TestSet set = new TestSet(new Lab08(), "Część II, testy laboratoryjne małe");
            {
                set.TestCases.Add(new SatisfactionTestCase(
                    points: new int[] { 2, 2, 2, 2 },
                    vs: new int[,] {
                        { 0, 2, 2, 2},
                        { 2, 0, 2, 2},
                        { 2, 2, 0, 2},
                        { 2, 2, 2, 0}
                    },
                    fondness: new int[,] {
                        { 0, 2, 2, 2},
                        { 2, 0, 2, 2},
                        { 2, 2, 0, 2},
                        { 2, 2, 2, 0}
                    },
                    expectedSatisfaction: new int[] { 0, 0, 0, 0 },
                    timeLimit: 1,
                    description: "Początek fazy grupowej MŚ, nikt nikogo specjalnie nie lubi"));
            }
            {
                set.TestCases.Add(new SatisfactionTestCase(
                    points: new int[] { 2, 2, 2, 2 },
                    vs: new int[,] {
                        { 0, 2, 2, 2},
                        { 2, 0, 2, 2},
                        { 2, 2, 0, 2},
                        { 2, 2, 2, 0}
                    },
                    fondness: new int[,] {
                        { 0, 100, 100, 100},
                        { 100, 0, 100, 100},
                        { 100, 100, 0, 100},
                        { 100, 100, 100, 0}
                    },
                    expectedSatisfaction: new int[] { 0, 0, 0, 0 },
                    timeLimit: 1,
                    description: "Początek fazy grupowej MŚ, wszyscy się uwielbiają"));
            }
            {
                set.TestCases.Add(new SatisfactionTestCase(
                    points: new int[] { 0, 15, 14, 13 },
                    vs: new int[,] {
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0}
                        },
                    fondness: new int[,] {
                        { 0, 1, 2, 4},
                        { 1, 0, 3, 5},
                        { 2, 3, 0, 6},
                        { 4, 5, 6, 0}
                    },
                    expectedSatisfaction: new int[] { -1, 0, -1, -1 },
                    timeLimit: 1,
                    description: "Koniec sezonu"));
            }
            {
                set.TestCases.Add(new SatisfactionTestCase(
                    points: new int[] { 15, 15, 0, 15 },
                    vs: new int[,] {
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0}
                    },
                    fondness: new int[,] {
                        { 0, 1, 2, 4},
                        { 1, 0, 3, 5},
                        { 2, 3, 0, 6},
                        { 4, 5, 6, 0}
                    },
                    expectedSatisfaction: new int[] { 0, 0, -1, 0 },
                    timeLimit: 1,
                    description: "Koniec sezonu, zwycięzcy ex aequo"));
            }
            return set;
        }

        TestSet makeBigFindSatisfaction()
        {
            TestSet set = new TestSet(new Lab08(), "Część II, testy laboratoryjne losowe");
            {
                Random r = new Random(4);
                int n = 10;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                int[,] fondness = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(10);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(2);
                        int randomFondness = r.Next(5);
                        vs[i, j] = vs[j, i] = remaining;
                        fondness[i, j] = fondness[j, i] = randomFondness;
                    }
                }
                set.TestCases.Add(new SatisfactionTestCase(
                    points: points,
                    vs: vs,
                    fondness: fondness,
                    expectedSatisfaction: new int[] { 21, 39, -1, 35, -1, 34, -1, -1, -1, -1 },
                    timeLimit: 1,
                    description: "Losowy I"));
            }
            {
                Random r = new Random(4);
                int n = 12;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                int[,] fondness = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(10);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(2);
                        int randomFondness = r.Next(5);
                        vs[i, j] = vs[j, i] = remaining;
                        fondness[i, j] = fondness[j, i] = randomFondness;
                    }
                }
                set.TestCases.Add(new SatisfactionTestCase(
                    points: points,
                    vs: vs,
                    fondness: fondness,
                    expectedSatisfaction: new int[] { 29, 29, -1, -1, 16, -1, -1, 46, -1, 38, -1, 34 },
                    timeLimit: 1,
                    description: "Losowy II"));
            }
            {
                Random r = new Random(4);
                int n = 13;
                int[] points = new int[n];
                int[,] vs = new int[n, n];
                int[,] fondness = new int[n, n];
                for (int i = 0; i < n; ++i)
                {
                    points[i] = r.Next(10);
                    for (int j = i + 1; j < n; ++j)
                    {
                        int remaining = r.Next(2);
                        int randomFondness = r.Next(5);
                        vs[i, j] = vs[j, i] = remaining;
                        fondness[i, j] = fondness[j, i] = randomFondness;
                    }
                }
                set.TestCases.Add(new SatisfactionTestCase(
                    points: points,
                    vs: vs,
                    fondness: fondness,
                    expectedSatisfaction: new int[] { 43, -1, 55, 45, -1, 44, 47, -1, -1, 21, 43, -1, 34 },
                    timeLimit: 1,
                    description: "Losowy III"));
            }
            return set;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Lab08TestModule Lab08Test = new Lab08TestModule();
            Lab08Test.PrepareTestSets();
            foreach (var ts in Lab08Test.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
        }
    }
}