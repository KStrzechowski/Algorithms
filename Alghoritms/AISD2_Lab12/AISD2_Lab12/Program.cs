using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ASD.TestCase;

namespace ASD
{
    class Program
    {
        static class Utils
        {
            static public string print_solution(List<(double, double)[]> res)
            {
                System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";

                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

                StringBuilder sb = new StringBuilder();

                foreach ((double, double)[] tr in res)
                {
                    sb.Append($"expected.Add(new[] {{ ({tr[0].Item1:f}, {tr[0].Item2:f}), ({tr[1].Item1:f}, {tr[1].Item2:f}), ({tr[2].Item1:f}, {tr[2].Item2:f})}});\n");
                }

                return sb.ToString();
            }

            static public bool Compare_triangles((double, double)[] t1, (double, double)[] t2)
            {
                bool[] used_t1 = new bool[3];
                bool[] used_t2 = new bool[3];

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!used_t1[i] && !used_t2[j] && Math.Abs(t1[i].Item1 - t2[j].Item1) <= 0.001 && Math.Abs(t1[i].Item2 - t2[j].Item2) <= 0.001)
                        {
                            used_t1[i] = true;
                            used_t2[j] = true;
                            break; // tak, to o 1 break za malo, powinno byc continue w drugiej
                        }
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    if (!used_t1[i] || !used_t2[i])
                        return false;
                }
                return true;
            }

            static public (Result resultCode, string message) Compare_triangles_lists(List<(double, double)[]> _expected, List<(double, double)[]> _result)
            {
                bool[] used_expected = new bool[_expected.Count];
                bool[] used_result = new bool[_result.Count];

                for (int i = 0; i < _expected.Count; i++)
                {
                    for (int j = 0; j < _result.Count; j++)
                    {
                        if (!used_expected[i] && !used_result[j] && Compare_triangles(_expected[i], _result[j]))
                        {
                            used_expected[i] = true;
                            used_result[j] = true;
                            break; // tak, to o 1 break za malo, powinno byc continue w drugiej
                        }
                    }
                }

                for (int i = 0; i < _expected.Count; i++)
                {
                    if (!used_expected[i] || !used_result[i])
                        return (Result.WrongResult, "[FAILED] Some triangles do not match");
                }
                return (Result.Success, "[PASSED] OK");
            }
        }

        class IsYMonotoneTestCase : TestCase
        {
            private readonly (double, double)[] _points;
            private readonly bool _expected;

            private bool _result;

            public IsYMonotoneTestCase(double timeLimit, Exception expectedException, string description,
                                            (double, double)[] points,
                                            bool expected) : base(timeLimit, expectedException, description)
            {
                _points = points;
                _expected = expected;
            }

            protected override void PerformTestCase(object prototypeObject)
            {
                _result = ((Lab12)prototypeObject).IsYMonotonePolygon(_points);
            }

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                if (_result != _expected && _expected)
                    return (Result.WrongResult, "[FAILED] Expected true, obtained false");

                if (_result != _expected && !_expected)
                    return (Result.WrongResult, "[FAILED] Expected false, obtained true");

                return (Result.Success, "[PASSED] OK");
            }
        }

        class TriangulationForEdgeVisiblePolygonTestCase : TestCase
        {
            private readonly (double, double)[] _points;
            int _edge_i;
            int _edge_j;
            private readonly List<(double, double)[]> _expected;

            private List<(double, double)[]> _result;

            public TriangulationForEdgeVisiblePolygonTestCase(double timeLimit, Exception expectedException, string description,
                                            (double, double)[] points, int edge_i, int edge_j,
                                            List<(double, double)[]> expected) : base(timeLimit, expectedException, description)
            {
                _points = points;
                _edge_i = edge_i;
                _edge_j = edge_j;
                _expected = expected;
            }

            protected override void PerformTestCase(object prototypeObject)
            {
                _result = ((Lab12)prototypeObject).TriangulationForEdgeVisiblePolygon(_points, _edge_i, _edge_j);
            }

            

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                if (_result == null)
                    return (Result.WrongResult, "[FAILED] Null returned");

                if (_result.Count != _expected.Count)
                    return (Result.WrongResult, $"[FAILED] Number of triangles in your solution: {_result.Count}, expected: {_expected.Count}");

                return Utils.Compare_triangles_lists(_expected, _result);
            }
        }

        class TriangulationForYMonotonePolygonTestCase : TestCase
        {
            private readonly (double, double)[] _points;
            private readonly List<(double, double)[]> _expected;

            private List<(double, double)[]> _result;

            public TriangulationForYMonotonePolygonTestCase(double timeLimit, Exception expectedException, string description,
                                            (double, double)[] points, 
                                            List<(double, double)[]> expected) : base(timeLimit, expectedException, description)
            {
                _points = points;
                _expected = expected;
            }

            protected override void PerformTestCase(object prototypeObject)
            {
                _result = ((Lab12)prototypeObject).TriangulationForYMonotonePolygon(_points);
            }



            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                if (_result == null)
                    return (Result.WrongResult, "[FAILED] Null returned");

                if (_result.Count != _expected.Count)
                    return (Result.WrongResult, $"[FAILED] Number of triangles in your solution: {_result.Count}, expected: {_expected.Count}");

                return Utils.Compare_triangles_lists(_expected, _result);
            }
        }
        class TriangulationTester : TestModule
        {
            private const int TIME_MULTIPLIER = 1;

            private double ScoreStage1()
            {
                if (TestSets["IsYMonotone"].PassedCount == TestSets["IsYMonotone"].TestCases.Count)
                    return 0.5;
                return 0;
            }

            private double ScoreStage2()
            {
                if (TestSets["TriangulationForEdgeVisiblePolygon"].PassedCount == TestSets["TriangulationForEdgeVisiblePolygon"].TestCases.Count)
                    return 1.0;
                return 0;
            }

            private double ScoreStage3()
            {
                if (TestSets["TriangulationForYMonotonePolygonTests"].PassedCount == TestSets["TriangulationForYMonotonePolygonTests"].TestCases.Count)
                    return 1.0;
                return 0;
            }

            public override double ScoreResult()
            {
                return ScoreStage1() + ScoreStage2() + ScoreStage3();
            }


            public override void PrepareTestSets()
            {
                TestSets["IsYMonotone"] = IsYMonotoneTests();
                TestSets["TriangulationForEdgeVisiblePolygon"] = TriangulationForEdgeVisiblePolygonTests();
                TestSets["TriangulationForYMonotonePolygonTests"] = TriangulationForYMonotonePolygonTests();
            }


            private TestSet IsYMonotoneTests()
            {
                var isYMonotoneTestSet = new TestSet(new Lab12(), "Is y-monotone");

                isYMonotoneTestSet.TestCases.Add(new IsYMonotoneTestCase(TIME_MULTIPLIER, null, "Simple quadrangle",
                    new[] { (0.0, 0.0), (0.0, 1.0), (1.0, 0.75), (1.0, 0.25)},
                    true));

                isYMonotoneTestSet.TestCases.Add(new IsYMonotoneTestCase(TIME_MULTIPLIER, null, "Square",
                    new[] { (0.0, 0.0), (0.0, 1.0), (1.0, 1.0), (1.0, 0.0) },
                    true));

                isYMonotoneTestSet.TestCases.Add(new IsYMonotoneTestCase(TIME_MULTIPLIER, null, "Concave quadrangle",
                    new[] { (0.0, 1.0), (0.5, 0.5), (1.0, 0.9), (0.5, 0.0) },
                    false));

                return isYMonotoneTestSet;
            }

            private TestSet TriangulationForEdgeVisiblePolygonTests()
            {
                var triangulationForEdgeVisiblePolygonTestSet = new TestSet(new Lab12(), "Triangulation for edge visible polygon");


                List<(double, double)[]> expected = new List<(double, double)[]>();
                expected.Add(new[] { (2.0, 1.0), (1.0, 0.0), (0.0, 1.0) });
                expected.Add(new[] { (2.0, 1.0), (0.0, 1.0), (0.0, 2.0) });
                expected.Add(new[] { (2.0, 1.0), (0.0, 2.0), (1.0, 3.0) });
                expected.Add(new[] { (2.0, 1.0), (1.0, 3.0), (2.0, 3.0) });

                triangulationForEdgeVisiblePolygonTestSet.TestCases.Add(new TriangulationForEdgeVisiblePolygonTestCase(TIME_MULTIPLIER, null, "Case 1",
                    new[] { (2.0, 1.0), (1.0, 0.0), (0.0, 1.0), (0.0, 2.0), (1.0, 3.0), (2.0, 3.0) }, 0, 5, expected));


                expected = new List<(double, double)[]>();
                expected.Add(new[] { (0.0, 0.0), (1.0, 1.0), (1.0, 2.0) });
                expected.Add(new[] { (1.0, 1.0), (1.0, 2.0), (2.0, 1.0) });
                expected.Add(new[] { (0.0, 3.0), (1.0, 2.0), (2.0, 4.0) });
                expected.Add(new[] { (2.0, 1.0), (1.0, 2.0), (2.0, 4.0) });

                triangulationForEdgeVisiblePolygonTestSet.TestCases.Add(new TriangulationForEdgeVisiblePolygonTestCase(TIME_MULTIPLIER, null, "Case 2",
                    new[] { (2.0, 1.0), (1.0, 1.0), (0.0, 0.0), (1.0, 2.0), (0.0, 3.0), (2.0, 4.0) }, 0, 5, expected));


                expected = new List<(double, double)[]>();
                expected.Add(new[] { (1.0, 0.0), (0.0, 1.0), (0.5, 2.0) });
                expected.Add(new[] { (0.5, 2.0), (2.0, 4.0), (1.0, 0.0) });
                expected.Add(new[] { (0.5, 2.0), (0.0, 3.0), (2.0, 4.0) });
                expected.Add(new[] { (1.0, 0.0), (3.0, 5.0), (2.0, 4.0) });

                triangulationForEdgeVisiblePolygonTestSet.TestCases.Add(new TriangulationForEdgeVisiblePolygonTestCase(TIME_MULTIPLIER, null, "Case 3",
                    new[] { (0.0, 3.0), (2.0, 4.0), (3.0, 5.0), (1.0, 0.0), (0.0, 1.0), (0.5, 2.0) }, 3, 2, expected));


                return triangulationForEdgeVisiblePolygonTestSet;
            }

            private TestSet TriangulationForYMonotonePolygonTests()
            {
                var triangulationForYMonotonePolygonTestSet = new TestSet(new Lab12(), "Triangulation for y-monotone polygon");

                // ta sama figura odbita 4x przez wszystkie osie

                List<(double, double)[]> expected = new List<(double, double)[]>();
                expected.Add(new[] { (1.0, 0.0), (0.0, 1.0), (0.5, 2.0) });
                expected.Add(new[] { (1.0, 0.0), (0.5, 2.0), (2.0, 4.0) });
                expected.Add(new[] { (1.0, 0.0), (2.0, 4.0), (3.0, 5.0) });
                expected.Add(new[] { (0.5, 2.0), (0.0, 3.0), (2.0, 4.0) });

                expected.Add(new[] { (2.0, 4.0), (3.0, 5.0), (0.5, 6.5) });
                expected.Add(new[] { (0.5, 6.5), (3.0, 5.0), (2.5, 5.5) });
                expected.Add(new[] { (0.5, 6.5), (2.75, 5.75), (2.5, 5.5) });
                
                expected.Add(new[] { (1.0, 8.0), (1.25, 7.0), (0.5, 6.5) });
                expected.Add(new[] { (2.75, 5.75), (1.25, 7.0), (0.5, 6.5) });

                triangulationForYMonotonePolygonTestSet.TestCases.Add(new TriangulationForYMonotonePolygonTestCase(TIME_MULTIPLIER, null, "Case 1",
                    new[] { (3.0, 5.0), (1.0, 0.0), (0.0, 1.0), (0.5, 2.0), (0.0, 3.0), (2.0, 4.0), (0.5, 6.5), (1.0, 8.0), (1.25, 7), (2.75, 5.75), (2.5, 5.5) }, expected));


                expected = new List<(double, double)[]>();
                expected.Add(new[] { (-2.00, 4.00), (0.00, 3.00), (-0.50, 2.00) });
                expected.Add(new[] { (-3.00, 5.00), (-2.00, 4.00), (-0.50, 2.00) });
                expected.Add(new[] { (-3.00, 5.00), (-0.50, 2.00), (0.00, 1.00) });
                expected.Add(new[] { (-3.00, 5.00), (0.00, 1.00), (-1.00, 0.00) });
                expected.Add(new[] { (-2.00, 4.00), (-3.00, 5.00), (-2.50, 5.50) });
                expected.Add(new[] { (-2.50, 5.50), (-2.75, 5.75), (-0.50, 6.50) });
                expected.Add(new[] { (-2.00, 4.00), (-2.50, 5.50), (-0.50, 6.50) });
                expected.Add(new[] { (-1.25, 7.00), (-1.00, 8.00), (-0.50, 6.50) });
                expected.Add(new[] { (-1.25, 7.00), (-0.50, 6.50), (-2.75, 5.75) });

                triangulationForYMonotonePolygonTestSet.TestCases.Add(new TriangulationForYMonotonePolygonTestCase(TIME_MULTIPLIER, null, "Case 2",
                    new[] { (-3.0, 5.0), (-2.5, 5.5), (-2.75, 5.75), (-1.25, 7), (-1, 8), (-0.5, 6.5), (-2.0, 4.0), (0.0, 3.0), (-0.5, 2.0), (0.0, 1.0), (-1.0, 0.0) }, expected));

                expected = new List<(double, double)[]>();
                expected.Add(new[] { (-1.25, -7.00), (-0.50, -6.50), (-1.00, -8.00) });
                expected.Add(new[] { (-0.50, -6.50), (-1.25, -7.00), (-2.75, -5.75) });
                expected.Add(new[] { (-0.50, -6.50), (-2.75, -5.75), (-2.50, -5.50) });
                expected.Add(new[] { (-0.50, -6.50), (-2.50, -5.50), (-3.00, -5.00) });
                expected.Add(new[] { (-0.50, -6.50), (-3.00, -5.00), (-2.00, -4.00) });
                expected.Add(new[] { (-1.00, 0.00), (0.00, -1.00), (-0.50, -2.00) });
                expected.Add(new[] { (-0.50, -2.00), (0.00, -3.00), (-2.00, -4.00) });
                expected.Add(new[] { (-1.00, 0.00), (-0.50, -2.00), (-2.00, -4.00) });
                expected.Add(new[] { (-1.00, 0.00), (-2.00, -4.00), (-3.00, -5.00) });

                triangulationForYMonotonePolygonTestSet.TestCases.Add(new TriangulationForYMonotonePolygonTestCase(TIME_MULTIPLIER, null, "Case 3",
                    new[] { (-3.0, -5.0), (-1.0, 0.0), (0.0, -1.0), (-0.5, -2.0), (0.0, -3.0), (-2.0, -4.0), (-0.5, -6.5), (-1, -8), (-1.25, -7), (-2.75, -5.75), (-2.5, -5.5)}, expected));



                expected = new List<(double, double)[]>();
                expected.Add(new[] { (0.50, -6.50), (1.25, -7.00), (1.00, -8.00) });
                expected.Add(new[] { (2.75, -5.75), (1.25, -7.00), (0.50, -6.50) });
                expected.Add(new[] { (2.00, -4.00), (3.00, -5.00), (2.50, -5.50) });
                expected.Add(new[] { (2.50, -5.50), (2.75, -5.75), (0.50, -6.50) });
                expected.Add(new[] { (2.00, -4.00), (2.50, -5.50), (0.50, -6.50) });
                expected.Add(new[] { (2.00, -4.00), (0.00, -3.00), (0.50, -2.00) });
                expected.Add(new[] { (3.00, -5.00), (2.00, -4.00), (0.50, -2.00) });
                expected.Add(new[] { (3.00, -5.00), (0.50, -2.00), (0.00, -1.00) });
                expected.Add(new[] { (3.00, -5.00), (0.00, -1.00), (1.00, 0.00) });

                triangulationForYMonotonePolygonTestSet.TestCases.Add(new TriangulationForYMonotonePolygonTestCase(TIME_MULTIPLIER, null, "Case 4",
                    new[] { (3.0, -5.0), (2.5, -5.5), (2.75, -5.75), (1.25, -7), (1, -8), (0.5, -6.5), (2.0, -4.0), (0.0, -3.0), (0.5, -2.0), (0.0, -1.0), (1.0, 0.0) }, expected));


                return triangulationForYMonotonePolygonTestSet;
            }
        }

            static void Main(string[] args)
        {
            var tests = new TriangulationTester();
            tests.PrepareTestSets();
            foreach (var testSet in tests.TestSets.Values)
                testSet.PerformTests(false);

            //Lab12 l = new Lab12();

            //var r = l.TriangulationForEdgeVisiblePolygon(new[] { (2.0, 1.0), (1.0, 0.0), (0.0, 1.0), (0.0, 2.0), (1.0, 3.0), (2.0, 3.0) }, 0, 5);
            //Console.WriteLine(r);

            //r = l.TriangulationForEdgeVisiblePolygon(new[] { (2.0, 1.0), (1.0, 1.0), (0.0, 0.0), (1.0, 2.0), (0.0, 3.0), (2.0, 4.0) }, 0, 5);
            //Console.WriteLine(r);


            //r = l.TriangulationForYMonotonePolygon(new[] { (3.0, 5.0), (1.0, 0.0), (0.0, 1.0), (0.5, 2.0), (0.0, 3.0), (2.0, 4.0), (0.5, 6.5), (1, 8), (1.25, 7), (2.75, 5.75), (2.5, 5.5) });
            //Console.WriteLine(r);

            //var r = l.TriangulationForYMonotonePolygon(new[] { (-3.0, 5.0), (-2.5, 5.5), (-2.75, 5.75), (-1.25, 7), (-1, 8), (-0.5, 6.5), (-2.0, 4.0), (0.0, 3.0), (-0.5, 2.0), (0.0, 1.0), (-1.0, 0.0) });
            //string rrr = Utils.print_solution(r);
            //Console.WriteLine(rrr);

            //var r = l.TriangulationForYMonotonePolygon(new[] { (-3.0, -5.0), (-1.0, 0.0), (0.0, -1.0), (-0.5, -2.0), (0.0, -3.0), (-2.0, -4.0), (-0.5, -6.5), (-1, -8), (-1.25, -7), (-2.75, -5.75), (-2.5, -5.5) });
            //string rrr = Utils.print_solution(r);
            //Console.WriteLine(rrr);

            //var r = l.TriangulationForYMonotonePolygon(new[] { (3.0, -5.0), (2.5, -5.5), (2.75, -5.75), (1.25, -7), (1, -8), (0.5, -6.5), (2.0, -4.0), (0.0, -3.0), (0.5, -2.0), (0.0, -1.0), (1.0, 0.0) });
            //string rrr = Utils.print_solution(r);
            //Console.WriteLine(rrr);



        }
    }
}
