//\usepackage{ulem}\usepackage{ulem}using System.Linq;

namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab06 : System.MarshalByRefObject
    {
        /// <summary>
        /// Część I: wyznaczenie najszerszej ścieżki grafu.
        /// </summary>
        /// <param name="G">informacja o przejazdach między punktami; wagi krawędzi są całkowite i nieujemne i oznaczają szerokość trasy między dwoma punktami</param>
        /// <param name="start">informacja o wierzchołku początkowym</param>
        /// <param name="end">informacja o wierzchołku końcowym</param>
        /// <returns>najszersza ścieżka między wierzchołkiem początkowym a końcowym lub pusta lista, jeśli taka ścieżka nie istnieje</returns>
        public List<int> WidePath(Graph G, int start, int end)
        {
            var result = new List<int>();
            int currentVert;
            var visited = new bool[G.VerticesCount]; // Tablica, w której przechowujemy odwiedzone już wierzchołki.
            var prev = new int[G.VerticesCount]; // Tablica, w której przechowamy poprzedni wierzchołek położony na najlepszej ścieżce.
            var minValues = new double[G.VerticesCount]; // Tablica, w której przechowamy szerokość najlepszej ścieżki do każdego wierzchołka.
            minValues[start] = double.MaxValue;

            // Tworzymy kolejkę priorytetową, dzięki której będziemy zawsze wybierać najbardziej opłacalny wierzchołek w danym momencie.
            var verticesQ = new PriorityQueue<int, double>((a, b) => (
                a.Value > b.Value
            ));


            verticesQ.Put(start, 1);
            while (!verticesQ.Empty)
            {
                currentVert = verticesQ.Get();
                visited[currentVert] = true;
                if (visited[end])
                    break;

                foreach (Edge Edge in G.OutEdges(currentVert))
                {
                    if (!visited[Edge.To] && minValues[Edge.To] < minValues[currentVert] && minValues[Edge.To] < Edge.Weight)
                    {
                        prev[Edge.To] = currentVert;
                        minValues[Edge.To] = minValues[currentVert] < Edge.Weight ? minValues[currentVert] : Edge.Weight;
                        if (verticesQ.Contains(Edge.To))
                            verticesQ.ImprovePriority(Edge.To, minValues[Edge.To]);
                        else
                            verticesQ.Put(Edge.To, minValues[Edge.To]);
                    }
                }
            }
           if (!visited[end])
                return result;

            // Zapisujemy wybraną ścieżkę.
            currentVert = end;
            while (currentVert != start)
            {
                result.Add(currentVert);
                currentVert = prev[currentVert];
            }
            result.Add(start);
            result.Reverse();

            return result;
        }

        /// <summary>
        /// Część II: wyznaczenie najszerszej epidemicznej ścieżki.
        /// </summary>
        /// <param name="G">informacja o przejazdach między punktami; wagi krawędzi są całkowite i nieujemne i oznaczają szerokość trasy między dwoma punktami</param>
        /// <param name="start">informacja o wierzchołku początkowym</param>
        /// <param name="end">informacja o wierzchołku końcowym</param>
        /// <param name="weights">wagi wierzchołków odpowiadające czasom oczekiwania na bramkach wjzadowych do poszczególnych miejsc. Wagi są nieujemne i całkowite</param>
        /// <param name="maxWeight">maksymalna waga krawędzi w grafie</param>
        /// <returns>ścieżka dla której różnica między jej najwęższą krawędzią a sumą wag wierzchołków przez które przechodzi jest maksymalna.</returns>
        public List<int> WeightedWidePath(Graph G, int start, int end, int[] weights, int maxWeight)
        {
            var result = new List<int>();
            int currentVert;
            int endFor = 0;
            var visited = new bool[G.VerticesCount]; // Tablica, w której przechowujemy odwiedzone już wierzchołki.
            (int, double) bestPath = (-1, double.MinValue);
            var prev = new int[G.VerticesCount]; // Tablica, w której przechowamy poprzedni wierzchołek położony na najlepszej ścieżce.
            var minValues = new (double, double)[G.VerticesCount]; // Tablica, w której przechowamy szerokość oraz łączny czas najlepszej ścieżki do każdego wierzchołka.

            // Tworzymy kolejkę priorytetową, dzięki której będziemy zawsze wybierać najbardziej opłacalny wierzchołek w danym momencie.
            var verticesQ = new PriorityQueue<int, double>((a, b) => (
                a.Value > b.Value
            ));

            // Rozwiązanie jest podobne do etapu I. Dodatkowo szukamy najlepszej ścieżki dla każdego k. Gdzie k oznacza maksymalną wagę użytej krawędzi.
            for (int k = maxWeight; k >= endFor; k--)
            {
                // Za każdym razem zaczynamy od wierzchołka startowego i tych samym wartości w tablicach pomocniczych.
                verticesQ.Put(start, 1);
                for (int i = 0; i < G.VerticesCount; i++)
                {
                    visited[i] = false;
                    minValues[i].Item1 = -1;
                    minValues[i].Item2 = double.MaxValue;
                }
                minValues[start] = (double.MaxValue, weights[start]);

                // W ostatnim przejściu po pętli wybieramy k, dla którego znaleziona ścieżka była najbardziej optymalna.
                if (k == 0)
                {
                    k = bestPath.Item1;
                    endFor = maxWeight + 1;
                }

                while (!verticesQ.Empty)
                {
                    currentVert = verticesQ.Get();
                    visited[currentVert] = true;
                    if (visited[end])
                        break;
                    foreach (Edge Edge in G.OutEdges(currentVert))
                    {
                        if (Edge.Weight <= k)
                        {
                            if (minValues[currentVert].Item1 < Edge.Weight) // Sprawdzamy czy obecnie rozpatrywana krawędź będzie najwęższą w całej ścieżce.
                            {
                                // Sprawdzamy czy doszliśmy już do danego wierzchołka w optymalniejszy sposób.
                                if (minValues[Edge.To].Item1 - minValues[Edge.To].Item2 <
                                    minValues[currentVert].Item1 - minValues[currentVert].Item2 - weights[Edge.To])
                                {
                                    // Uaktualniamy skąd dotarliśmy do danego wierzchołka, oraz wartości w tablicach pomocniczych.
                                    prev[Edge.To] = currentVert;
                                    minValues[Edge.To] = (minValues[currentVert].Item1, minValues[currentVert].Item2 + weights[Edge.To]);
                                    verticesQ.Put(Edge.To, minValues[Edge.To].Item1 - minValues[Edge.To].Item2);
                                }
                            }
                            // Sprawdzamy czy doszliśmy już do danego wierzchołka w optymalniejszy sposób.
                            else if (minValues[Edge.To].Item1 - minValues[Edge.To].Item2 <
                                Edge.Weight - minValues[currentVert].Item2 - weights[Edge.To])
                            {
                                // Uaktualniamy skąd dotarliśmy do danego wierzchołka, oraz wartości w tablicach pomocniczych.
                                prev[Edge.To] = currentVert;
                                minValues[Edge.To] = (Edge.Weight, minValues[currentVert].Item2 + weights[Edge.To]);
                                verticesQ.Put(Edge.To, minValues[Edge.To].Item1 - minValues[Edge.To].Item2);
                            }
                        }
                    }
                }
                // Sprawdzamy czy ścieżka dla danego k jest najbardziej optymalna.
                if (minValues[end].Item1 - minValues[end].Item2 > bestPath.Item2)
                {
                    bestPath = (k, minValues[end].Item1 - minValues[end].Item2);
                }
                if (k == maxWeight && !visited[end])
                    return result;
            }

            // Zapisujemy wybraną ścieżkę.
            currentVert = end;
            while (currentVert != start)
            {
                result.Add(currentVert);
                currentVert = prev[currentVert];
            }
            result.Add(start);
            result.Reverse();

            return result;
        }
    }
}