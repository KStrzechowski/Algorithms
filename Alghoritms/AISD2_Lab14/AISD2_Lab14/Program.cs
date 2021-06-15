using System;
using System.Collections.Generic;
using System.Text;
using static ASD.TestCase;

namespace ASD
{

    class Program
    {
        class EditDistanceTester : TestModule
        {
            private const int TIME_MULTIPLIER = 1;

            private readonly List<(string, string, float, int)> wordsBasic = new List<(string, string, float, int)>
            {
                ("", "a", 1.0f, 1),
                ("aaa", "aaab", 1.0f, 1),
                ("aaa", "baaa", 1.0f, 1),
                ("abc", "ab", 1.0f, 1),
                ("abc", "ac", 1.0f, 1),
                ("abc", "bc", 1.0f, 1),
                ("abc", "cb", 2.0f, 1),
                ("abcd", "dacb", 3.0f, 2),
                ("asdf", "asdg", 1.0f, 1),
                ("asdf", "adsf", 2.0f, 3),
            };

            private readonly List<(string, string, float, int)> wordsTransposition = new List<(string, string, float, int)>
            {
                ("", "a", 1.0f, 1),
                ("aaa", "aaab", 1.0f, 1),
                ("aaa", "baaa", 1.0f, 1),
                ("abc", "ab", 1.0f, 1),
                ("abc", "ac", 1.0f, 1),
                ("abc", "bc", 1.0f, 1),
                ("abc", "cb", 2.0f, 2),
                ("abcd", "dacb", 3.0f, 3),
                ("asdf", "asdg", 1.0f, 1),
                ("asdf", "adsf", 1.0f, 1),
            };

            public override void PrepareTestSets()
            {
                TestSets["Stage 1. Basic editing distance without transposition"] = BasicTestsWithoutSequences();
                TestSets["Stage 2. Basic editing distance with sequence without transposition"] = BasicTestsWithSequences();
                TestSets["Stage 3. Editing distance with transposition"] = TranspositionTests();
            }

            private TestSet BasicTestsWithoutSequences()
            {
                var testSet = new TestSet(new Lab14(), "Stage 1. Basic editing distance without transposition");

                int i = 1;
                foreach (var test in wordsBasic)
                {
                    testSet.TestCases.Add(new EditDistanceTestCase(TIME_MULTIPLIER, null, $"Test {i++}", test, false, false));
                }
                return testSet;
            }

            private TestSet BasicTestsWithSequences()
            {
                var testSet = new TestSet(new Lab14(), "Stage 2. Basic editing distance with sequences without transposition");

                int i = 1;
                foreach (var test in wordsBasic)
                {
                    var testDescription = $"Test {i++}. '{test.Item1}' -> '{test.Item2}'";
                    testSet.TestCases.Add(new EditDistanceTestCase(TIME_MULTIPLIER, null, testDescription, test, true, false));
                }
                return testSet;
            }

            private TestSet TranspositionTests()
            {
                var testSet = new TestSet(new Lab14(), "Stage 3. Editing distance with transposition");

                int i = 1;
                foreach (var test in wordsTransposition)
                {
                    var testDescription = $"Test {i++}. '{test.Item1}' -> '{test.Item2}'";
                    testSet.TestCases.Add(new EditDistanceTestCase(TIME_MULTIPLIER, null, testDescription, test, true, true));
                }
                return testSet;
            }
        }

        class EditDistanceTestCase : TestCase
        {
            private readonly (string, string, float, int) test;
            private readonly bool checkSequence;
            private readonly bool allowTranspositions;
            // Test results
            private float distance;
            private List<List<Lab14.EditOperation>> possibleEditSequences;
            public EditDistanceTestCase(double timeLimit, Exception expectedException, string description,
                                            (string, string, float, int) test, bool checkSequence, bool allowTranspositions)
                : base(timeLimit, expectedException, description)
            {
                this.test = test;
                this.checkSequence = checkSequence;
                this.allowTranspositions = allowTranspositions;
            }

            protected override void PerformTestCase(object prototypeObject)
            {
                this.distance = ((Lab14)prototypeObject).EditingDistance(test.Item1, test.Item2, allowTranspositions, out this.possibleEditSequences);
            }

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                if (distance != test.Item3)
                    return (Result.WrongResult, $"[FAILED] Distance for words '{test.Item1}' and '{test.Item2}' should be {test.Item3} insted of {distance}");

                if (checkSequence)
                {
                    bool allEditSequencesCorrect = true;
#if SHOW_SEQUENCES
                    Console.WriteLine();
#endif
                    foreach (var changeList in possibleEditSequences)
                    {
                        string sequenceString = test.Item1;
                        var word = test.Item1;
                        foreach (var change in changeList)
                        {
                            sequenceString += " --> " + change;
                            word = change.Modify(word);
                            sequenceString += $" --> {word}";
                        }
                        if (word != test.Item2)
                        {
                            allEditSequencesCorrect = false;
                            sequenceString += " ERROR!";
                        }
#if SHOW_SEQUENCES
                        Console.WriteLine("\t" + sequenceString);
#endif
                    }
                    if (possibleEditSequences.Count == 0)
                    {
                        return (Result.WrongResult, $"[FAILED] No edit sequences returned");
                    }
                    else if (allEditSequencesCorrect)
                    {
                        if (test.Item4 != possibleEditSequences.Count)
                            return (Result.WrongResult, $"[FAILED] Missing edit sequences, returned {possibleEditSequences.Count}, should be: {test.Item4}");
                    }
                    else
                    {
                        return (Result.WrongResult, $"[FAILED] Sequence is incorrect");
                    }
                }

                return (Result.Success, $"[PASSED] OK, distance for words '{test.Item1}' and '{test.Item2}' is {distance}");
            }
        }

        static void Main(string[] args)
        {
            var tests = new EditDistanceTester();
            tests.PrepareTestSets();
            foreach (var testSet in tests.TestSets.Values)
                testSet.PerformTests(false);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey();
            }
        }
    }
}