using System;
using System.Collections.Generic;
using ASD.Graphs;
using System.Linq;

namespace Lab10
{
    public class Lab10 : MarshalByRefObject
    {
        private readonly (double?, Edge[]) _emptySolutionResult = (null, new Edge[0]);

        /// <summary>
        /// Calculate minimal path between all locations that are still in use
        /// </summary>
        /// <param name="g">Graph representing mountain village with all locations and paths between locations </param>
        /// <param name="terminalVertices">identified locations that are still in use</param>
        /// <returns>
        ///     "sumOfEdgesWeight"- represents minimal path length between all terminal locations
        ///     "path" - represents list of all edges used to build connection between all terminal locations
        ///         NOT VERIFIED IN VERSION1 
        /// </returns>
        double minCost;

        // Wyznaczamy ograniczenie górne początkowe dla kosztu szukanej drogi. 
        bool Estimate(Graph g, int[] terminalVertices)
        {
            HashSet<Edge> edges = new HashSet<Edge>();
            g.DijkstraShortestPaths(terminalVertices[terminalVertices.Length / 2], out PathsInfo[] d);
            foreach (var x in terminalVertices)
            {
                var info = d[x];
                if (x == terminalVertices[terminalVertices.Length / 2])
                    continue;
                if (!info.Last.HasValue)
                    return false;

                minCost += info.Dist;
                 // Przechodzimy przez całą znalezioną ścieżkę.
                 // Jeśli przechodzimy przez daną krawędź więcej niż raz to pomniejszamy minCost o jej wartość uzyskując lepsze ograniczenie górne.
                Edge last =  info.Last.Value;
                while (last.From != terminalVertices[terminalVertices.Length / 2])
                {
                    if (edges.Contains(last))
                        minCost -= last.Weight;
                    else
                        edges.Add(last);
                    last = d[last.From].Last.Value;
                }
            }
            if (terminalVertices.Length != 1 && minCost == 0)
                return false;
            return true;
        }

        public (double? sumOfEdgesWeight, Edge[] path) Version1(Graph g, int[] terminalVertices)
        {
            minCost = 0;
            if (!Estimate(g, terminalVertices))
                return _emptySolutionResult;
            int left = terminalVertices.Length - 1;

          
            List<Edge> edgesList = new List<Edge>();
            bool[] visited = new bool[g.VerticesCount];
            visited[terminalVertices[0]] = true; 
            // Wstawiamy krawędzie z pierwszego wierzchołka, do którego musimy dotrzeć. Od niego zaczniemy.
            foreach (var edge in g.OutEdges(terminalVertices[0]))
                edgesList.Add(edge);

            Find(g, terminalVertices, ref edgesList, visited, 0, 0, left);

            return (minCost, new Edge[0]);
        }

        void Find(Graph g, int[] terminalVertices, ref List<Edge> edgesList, bool[] visited, double currentCost, int i, int left)
        {
            // Sprawdzamy czy dotarliśmy już do wszystkich potrzebnych wierzchołków.
            if (left == 0)
            {
                if (minCost > currentCost)
                    minCost = currentCost;
                return;
            }
                
            if (edgesList.Count <= i || currentCost + left >= minCost)
                return;

            // Wywołujemy naszą funkcję rekurencyjnie dla przypadku, gdy wybrana krawędź nie jest użyta.
            Find(g, terminalVertices, ref edgesList, visited, currentCost, i + 1, left);

            Edge currentEdge = edgesList[i];
            if (visited[currentEdge.To])
                return;
            foreach (var edge in g.OutEdges(currentEdge.To))
                if (edge != currentEdge)
                    edgesList.Add(edge);

            // Kolejny raz wywołujemy naszą funkcję dla przypadku, gdy wybrana krawędź jest użyta.
            visited[currentEdge.To] = true;
            if (terminalVertices.Contains(currentEdge.To)) // Jeśli dotarliśmy do terminalVertice to zmniejszamy ilość nieodwiedzonych potrzebnych miejsc.
                left--;
            Find(g, terminalVertices, ref edgesList, visited, currentCost + currentEdge.Weight, i + 1, left);
            visited[currentEdge.To] = false;

            // Przed powrotem z funkcji usuwamy dodane krawędzie.
            foreach (var edge in g.OutEdges(currentEdge.To))
                if (edge != currentEdge)
                    edgesList.Remove(edge);
        }


        /// <summary>
        /// Calculate minimal path between all locations that are still in use
        /// </summary>
        /// <param name="g">Graph representing mountain village with all locations and paths between locations </param>
        /// <param name="terminalVertices">identified locations that are still in use</param>
        /// <returns>
        ///     "sumOfEdgesWeight"- represents minimal path length between all terminal locations
        ///     "path" - represents list of all edges used to build connection between all terminal locations
        /// </returns>
        
        List<Edge> path = new List<Edge>(); // Tworzymy listę, która przechowuje path dla najkrótszej ścieżki.

        public (double? sumOfEdgesWeight, Edge[] path) Version2(Graph g, int[] terminalVertices)
        {
            minCost = 1;
            int left = terminalVertices.Length - 1;
            if (!Estimate(g, terminalVertices))
                return _emptySolutionResult;

            List<Edge> edgesList = new List<Edge>();
            bool[] visited = new bool[g.VerticesCount];
            List<int> visitedEdges = new List<int>(); // Tworzymy dodatkową listę, która przechowa szukany path.
            visited[terminalVertices[0]] = true;
            foreach (var edge in g.OutEdges(terminalVertices[0]))
                edgesList.Add(edge);

            Find2(g, terminalVertices, ref edgesList, visited, 0, 0, ref visitedEdges, left);

            // Zmieniamy znalezioną ścieżkę na tablicę.
            Edge[] result = new Edge[path.Count]; 
            for (int i = 0; i < path.Count; i++)
                result[i] = path[i];

            return (minCost, result);
        }

        void Find2(Graph g, int[] terminalVertices, ref List<Edge> edgesList, bool[] visited, double currentCost, int i, ref List<int> visitedEdges, int left)
        {
            if (left == 0)
            {
                if (minCost > currentCost)
                { 
                    // Zmieniamy path dla nowej najkrótszej ścieżki
                    path.Clear();
                    foreach (var edge in visitedEdges)
                        path.Add(edgesList[edge]);
                    minCost = currentCost;
                }               
                return;
            }

            if (edgesList.Count <= i || currentCost >= minCost)
                return;

            Find2(g, terminalVertices, ref edgesList, visited, currentCost, i + 1, ref visitedEdges, left);

            Edge currentEdge = edgesList[i];
            if (visited[currentEdge.To])
                return;
            foreach (var edge in g.OutEdges(currentEdge.To))
                if (edge != currentEdge)
                    edgesList.Add(edge);

            visited[currentEdge.To] = true;
            visitedEdges.Add(i);
            if (terminalVertices.Contains(currentEdge.To))
                left--;

            Find2(g, terminalVertices, ref edgesList, visited, currentCost + currentEdge.Weight, i + 1, ref visitedEdges, left);
            visitedEdges.Remove(i);
            visited[currentEdge.To] = false;

            foreach (var edge in g.OutEdges(currentEdge.To))
                if (edge != currentEdge)
                    edgesList.Remove(edge);
        }
    }
}