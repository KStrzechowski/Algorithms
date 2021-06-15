using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public class Lab14 : System.MarshalByRefObject
    {
        public interface EditOperation { string Modify(string word); };
        public class RemoveOperation : System.MarshalByRefObject, EditOperation
        {
            int index;
            public RemoveOperation(int index) { this.index = index; }
            public string Modify(string word) { return new StringBuilder(word).Remove(index, 1).ToString(); }
            public override string ToString() => $"Remove at {index}";
        }
        public class AddOperation : System.MarshalByRefObject, EditOperation
        {
            int index;
            char letter;
            public AddOperation(int index, char letter) { this.index = index; this.letter = letter; }
            public string Modify(string word) { return new StringBuilder(word).Insert(index, letter).ToString(); }
            public override string ToString() => $"Add at {index} '{letter}'";
        };
        public class ChangeOperation : System.MarshalByRefObject, EditOperation
        {
            int index;
            char letter;
            public ChangeOperation(int index, char letter) { this.index = index; this.letter = letter; }
            public string Modify(string word) { return new StringBuilder(word) { [index] = letter }.ToString(); }
            public override string ToString() => $"Change at {index} to '{letter}'";
        }
        public class TranspositionOperation : System.MarshalByRefObject, EditOperation
        {
            int index;
            public TranspositionOperation(int index) { this.index = index; }
            public string Modify(string word) { return new StringBuilder(word) { [index] = word[index + 1], [index + 1] = word[index] }.ToString(); }
            public override string ToString() => $"Transposition at {index} and {index + 1}";
        };

        /// <summary>
        /// Calculate distance between two words.
        /// </summary>
        /// <param name="word1">Word 1</param>
        /// <param name="word2">Word 2</param>
        /// <param name="allowTranspositions">If true transpositions of adjacent letters is possible</param>
        /// <param name="possibleEditSequences">List of all possible edit operations sequences.</param>
        /// <returns>Distance between two words.</returns>

        public float EditingDistance(string word1, string word2, bool allowTranspositions, out List<List<EditOperation>> possibleEditSequences)
        {
            possibleEditSequences = new List<List<EditOperation>>();
            int n = word1.Length + 1;
            int m = word2.Length + 1;
            if (n < 1 || m < 1)
                return 1;

            var distance = new float[n, m];
            for (int i = 0; i < n; i++)
                distance[i, 0] = i;
            for (int i = 0; i < m; i++)
                distance[0, i] = i;

            for(int i = 1; i < n; i++)
            {
                for(int j = 1; j < m; j++)
                {
                    // Wyszukujemy minimalny koszt, który pozwoli nam dojść do wybranego miejsca.
                    if (word1[i - 1] == word2[j - 1])
                        distance[i, j] = distance[i - 1, j - 1];
                    else
                        distance[i, j] = FindMinimum(distance[i - 1, j], distance[i, j - 1], distance[i - 1, j - 1]) + 1;

                    // Sprawdzamy czy nie da się dojść do tego miejsca szybciej za pomocą transpozycji.
                    if (allowTranspositions)
                    {
                        if (i > 1 && j > 1 && word1[i - 1] == word2[j - 2] && word1[i - 2] == word2[j - 1] && word1[i - 1] != word2[j - 1])
                        {
                            if (distance[i, j] > distance[i - 2, j - 2] + 1)
                                distance[i, j] = distance[i - 2, j - 2] + 1;

                            continue;
                        }
                    }
                }
            }
            List<EditOperation> currentList = new List<EditOperation>();
            Recursion(word1, word2, distance, n - 1, m - 1, ref possibleEditSequences, currentList, allowTranspositions);

            return distance[n - 1, m - 1];
        }

        public float FindMinimum(float a, float b, float c) => Math.Min(Math.Min(a, b), c);

        public void Recursion(string word1, string word2, float[,] distance, int i, int j, 
            ref List<List<EditOperation>> result, List<EditOperation> currentList, bool allowTranspositions)
        {
            float min;
            // Sprawdzamy czy doszliśmy do końca, jeśli nie to wybieramy minimalną długość drogi
            if (i == 0 && j == 0)
            {
                result.Add(currentList);
                return;
            }
            else if (i == 0)
                min = distance[i, j - 1];
            else if (j == 0)
                min = distance[i - 1, j];
            else
                min = FindMinimum(distance[i - 1, j], distance[i, j - 1], distance[i - 1, j - 1]);

            // Sprawdzamy czy trzeba zmieniać litery, jeśli nie to nic nie zmieniamy i przechodzimy na skos.
            if (i > 0 && j > 0 && word1[i - 1] == word2[j - 1])
            {
                List<EditOperation> nextList = new List<EditOperation>(currentList);
                Recursion(word1, word2, distance, i - 1, j - 1, ref result, nextList, allowTranspositions);
                return;
            }

            // Sprawdzamy czy można wykonać transpozycję
            if (allowTranspositions)
            {
                if (i > 1 && j > 1 && word1[i - 1] == word2[j - 2] && word1[i - 2] == word2[j - 1])
                {

                    List<EditOperation> nextList = new List<EditOperation>(currentList);
                    nextList.Add(new TranspositionOperation(i - 2));
                    Recursion(word1, word2, distance, i - 2, j - 2, ref result, nextList, allowTranspositions);
                    min = distance[i - 2, j - 2];
                }
            }

            // Idziemy w każdą stronę, której wartość jest równa minimalnej odległości.
            if (i > 0 && min == distance[i - 1, j])
            {
                List<EditOperation> nextList = new List<EditOperation>(currentList) ;
                nextList.Add(new RemoveOperation(i - 1));
                Recursion(word1, word2, distance, i - 1, j, ref result, nextList, allowTranspositions);
            }
            if (j > 0 && min == distance[i, j - 1])
            {
                List<EditOperation> nextList = new List<EditOperation>(currentList);
                nextList.Add(new AddOperation(i, word2[j - 1]));
                Recursion(word1, word2, distance, i, j - 1, ref result, nextList, allowTranspositions);
            }
            if (i > 0 && j > 0 && min == distance[i - 1, j - 1])
            {
                List<EditOperation> nextList = new List<EditOperation>(currentList);
                nextList.Add(new ChangeOperation(i - 1, word2[j - 1]));
                Recursion(word1, word2, distance, i - 1, j - 1, ref result, nextList, allowTranspositions);
            }

            return;
        }
    }
}