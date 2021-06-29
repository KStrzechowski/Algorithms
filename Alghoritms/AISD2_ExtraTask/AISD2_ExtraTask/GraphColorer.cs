using System;
using System.Collections.Generic;
using ASD.Graphs;
using System.Linq;
using System.Threading;
using System.Collections;

namespace ASD2
{
    public class GraphColorer : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>

        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            int V = g.VerticesCount;
            var color = new int[V];
            int[] correctColoring = new int[V];
            List<int> sortedVertices = new List<int>();
            bool[] used = new bool[V];
            int[] priorities = new int[V];
            PriorityQueue<int, int> queue = new PriorityQueue<int, int>((x, y) => x.Value >= y.Value);
            int max = g.OutDegree(0), positionMax = 0;

            for (int i = 0; i < V; ++i)
            {
                if (max <= g.OutDegree(i))
                {
                    max = g.OutDegree(i);
                    positionMax = i;
                }
            }
            sortedVertices.Add(positionMax);
            used[positionMax] = true;
            for (int i = 0; i < V; ++i)
            {
                if (i == positionMax)
                    continue;
                queue.Put(i, 0);
            }
            int j = 0;
            while (j != V - 1)
            {
                foreach (var edge in g.OutEdges(sortedVertices[j]))
                {
                    if (!used[edge.To])
                    {
                        ++priorities[edge.To];
                        queue.ImprovePriority(edge.To, priorities[edge.To]);
                    }
                }

                positionMax = queue.Get();
                used[positionMax] = true;
                sortedVertices.Add(positionMax);
                ++j;
            }

            color[sortedVertices[0]] = 1;
            int min = V + 1;
            min = ColorGraph(g, color, 1, 1, V - 1, sortedVertices, ref correctColoring, ref min);
            return (min, correctColoring);
        }

        bool isSafeToColor(Graph g, int[] color, int c, int v)
        {
            foreach (var edge in g.OutEdges(v))
            {
                if (c == color[edge.To])
                    return false;
            }

            return true;
        }

        int ColorGraph(Graph g, int[] color, int m, int v, int left, List<int> sortedVertices, ref int[] correctColoring, ref int min)
        {
            if (m >= min)
                return m;
            if (left == 0)
            {
                for (int j = 0; j < g.VerticesCount; j++)
                    correctColoring[j] = color[j];
                return m;
            }
            int temp;

            for (int i = 1; i <= m + 1; ++i)
            {
                if (isSafeToColor(g, color, i, sortedVertices[v]))
                {
                    color[sortedVertices[v]] = i;
                    if (m > i)
                        temp = ColorGraph(g, color, m, v + 1, left - 1, sortedVertices, ref correctColoring, ref min);
                    else
                        temp = ColorGraph(g, color, i, v + 1, left - 1, sortedVertices, ref correctColoring, ref min);
                    if (min > temp)
                    {
                        min = temp;
                        if (m == min)
                        {
                            break;
                        }
                    }
                }
            }
            color[sortedVertices[v]] = 0;
            return min;
        }
    }
}