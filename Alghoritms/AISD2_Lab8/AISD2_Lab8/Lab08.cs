using System.Linq;

namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab08 : System.MarshalByRefObject
    {
        // Etap I
        /// <summary>
        /// Wyznacza możliwych zwycięzców i końcowy stan tabeli
        /// </summary>
        /// <returns>
        /// Lista drużyn, które wciąż mają szansę na mistrzostwo (chociażby ex aequo) w porządku rosnącym
        /// </returns>
        /// <param name="points">points[i] - liczba punktów na koncie drużyny i</param>
        /// <param name="vs">vs[i, j] - liczba pozostałych spotkań między drużyną <em>i</em> oraz <em>j</em></param>
        /// <param name="finalResult">Jeśli i-ta drużyna ma szansę na wygranie ligi: finalResult[i] - przykładowy stan tabeli na koniec sezonu. <br></br> 
        /// W przeciwnym przypadku: finalResult[i] = null</param>
        public List<int> FindPossibleWinners(int[] points, int[,] vs, out int[][] finalResult)
        {
            int n = points.Length;
            int allGames = 0;
            var possibleWinners = new List<int>();
            finalResult = new int[n][];

            // Sprawdzamy ile gier zostało każdej drużynie oraz wszystkich łącznie
            var remainingGames = new int[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    remainingGames[i] += vs[i, j];
                    if (i <= j)
                        allGames += vs[i, j];
                }

            for (int i = 0; i < n; i++)
            {
                int maxWins;
                int maxScore = points[i] + remainingGames[i];
                bool isPossible = true;
                var newGraph = new AdjacencyListsGraph<AVLAdjacencyList>(true, n * (n + 1) + 2);
                // n + 1 node in every row is team node - i * (n + 1) + n
                // n * (n + 1) + 1 is source node - n * (n + 1)
                // n * (n + 1) + 2 is sink node - n * (n + 1) + 1
                // source node -> Vjk (wierzchołek oznaczający grę między j-tym oraz k-tym zespołem) -> Vj (wierzchołek oznaczający j-ty zespół) -> sink node
                // przepływ: sorce node -> Vij - liczba gier pozostała do rozegrania pomiędzy j-tym oraz k-tym zespołem
                // Vj -> sink node - maksymalna liczba gier, którą może wygrać zespół j, aby nie zostać liderem (samodzielnym)

                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        if (points[j] > maxScore)
                        {
                            isPossible = false;
                            break;
                        }

                        maxWins = maxScore - points[j];
                        if (maxWins > 0)
                        {
                            newGraph.AddEdge(j * (n + 1) + n, n * (n + 1) + 1, maxWins);
                        }

                    }
                }

                if (!isPossible)
                {
                    continue;
                }
                for (int j = 0; j < n; j++)
                {
                    for (int k = j + 1; k < n; k++)
                    {
                        if (i != j && i != k && vs[j, k] > 0)
                        {
                            newGraph.AddEdge(n * (n + 1), j * (n + 1) + k, vs[j, k]);
                            newGraph.AddEdge(j * (n + 1) + k, j * (n + 1) + n, double.MaxValue);
                            newGraph.AddEdge(j * (n + 1) + k, k * (n + 1) + n, double.MaxValue);
                        }
                    }
                }

                var (flowValue, f) = newGraph.PushRelabelMaxFlow(n * (n + 1), n * (n + 1) + 1, MaxFlowGraphExtender.BFMaxPath);

                // Sprawdzamy czy liczba wszystkich gier jest równa przepływowi jaki otrzymaliśmy
                if (flowValue + remainingGames[i] == allGames)
                {
                    possibleWinners.Add(i);
                    finalResult[i] = new int[n];
                    finalResult[i][i] = maxScore;
                    for (int j = 0; j < n; j++)
                        if (i != j)
                        {
                            finalResult[i][j] = points[j];
                            if ((int)f.GetEdgeWeight(j * (n + 1) + n, n * (n + 1) + 1) > 0)
                                finalResult[i][j] += (int)f.GetEdgeWeight(j * (n + 1) + n, n * (n + 1) + 1);
                        }
                }
            }

            return possibleWinners;
        }

        // Etap II
        /// <summary>
        /// Wyznacza maksymalny poziom satysfakcji
        /// </summary>
        /// <returns>
        /// Tablica maksymalnej satysfakcji. <br></br>
        /// Maksymalna satysfakcja kibiców zespołu o ile ten zespół może wygrać ligę. <br></br>
        /// W przeciwnym przypadku: -1.
        /// </returns>
        /// <param name="points">points[i] - liczba punktów na koncie drużyny i</param>
        /// <param name="vs">vs[i, j] - liczba pozostałych spotkań między drużyną <em>i</em> oraz <em>j</em></param>
        /// <param name="fondness">fondness[i, j] - sympatia jaką kibice drużyny i darzą drużynę j</param>
        public int[] FindMaxSatisfaction(int[] points, int[,] vs, int[,] fondness)
        {
            int n = points.Length;
            int allGames = 0;
            var result = new int[n];

            // Sprawdzamy ile gier zostało każdej drużynie oraz wszystkich łącznie
            var remainingGames = new int[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    remainingGames[i] += vs[i, j];
                    if (i <= j)
                        allGames += vs[i, j];
                }

            for (int i = 0; i < n; i++)
            {
                int maxWins;
                int maxScore = points[i] + remainingGames[i];
                bool isPossible = true;
                var costGraph = new AdjacencyListsGraph<AVLAdjacencyList>(true, n * (n + 1) + 2); // dodatkowy graf, w którym przechowuję satysfakcję kibiców z każdej gry
                var newGraph = new AdjacencyListsGraph<AVLAdjacencyList>(true, n * (n + 1) + 2);
                // n + 1 node in every row is team node - i * (n + 1) + n
                // n * (n + 1) + 1 is source node - n * (n + 1)
                // n * (n + 1) + 2 is sink node - n * (n + 1) + 1
                // source node -> Vjk (wierzchołek oznaczający grę między j-tym oraz k-tym zespołem) -> Vj (wierzchołek oznaczający j-ty zespół) -> sink node
                // przepływ: sorce node -> Vij - liczba gier pozostała do rozegrania pomiędzy j-tym oraz k-tym zespołem
                // Vj -> sink node - maksymalna liczba gier, którą może wygrać zespół j, aby nie zostać liderem (samodzielnym)

                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        if (points[j] > maxScore)
                        {
                            isPossible = false;
                            break;
                        }

                        maxWins = maxScore - points[j];
                        if (maxWins > 0)
                        {
                            newGraph.AddEdge(j * (n + 1) + n, n * (n + 1) + 1, maxWins);
                            costGraph.AddEdge(j * (n + 1) + n, n * (n + 1) + 1, 0);
                        }
                    }
                }

                if (!isPossible)
                {
                    result[i] = -1;
                    continue;
                }
                for (int j = 0; j < n; j++)
                {
                    for (int k = j + 1; k < n; k++)
                    {
                        if (i != j && i != k && vs[j, k] > 0)
                        {
                            newGraph.AddEdge(n * (n + 1), j * (n + 1) + k, vs[j, k]);
                            newGraph.AddEdge(j * (n + 1) + k, j * (n + 1) + n, double.MaxValue);
                            newGraph.AddEdge(j * (n + 1) + k, k * (n + 1) + n, double.MaxValue);
                            costGraph.AddEdge(n * (n + 1), j * (n + 1) + k, 0);
                            if (fondness[i, j] > fondness[i, k])
                            {
                                costGraph.AddEdge(j * (n + 1) + k, j * (n + 1) + n, fondness[i, k] - fondness[i, j]);
                                costGraph.AddEdge(j * (n + 1) + k, k * (n + 1) + n, 0);
                            }
                            else
                            {
                                costGraph.AddEdge(j * (n + 1) + k, j * (n + 1) + n, 0);
                                costGraph.AddEdge(j * (n + 1) + k, k * (n + 1) + n, fondness[i, j] - fondness[i, k]);
                            }
                        }
                    }
                }

                var (flowValue, fond, f) = MinCostFlowGraphExtender.MinCostFlow(newGraph, costGraph, n * (n + 1), n * (n + 1) + 1, false, MaxFlowGraphExtender.PushRelabelMaxFlow, MaxFlowGraphExtender.BFMaxPath);
                // Sprawdzamy czy liczba wszystkich gier jest równa przepływowi jaki otrzymaliśmy
                if (flowValue + remainingGames[i] == allGames)
                {
                    result[i] = -(int)fond;
                }
                else
                {
                    result[i] = -1;
                }
            }

            return result;
        }
    }
}