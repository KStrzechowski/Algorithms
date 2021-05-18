
using System;
using ASD.Graphs;

namespace ASD
{

    public class Lab03GraphFunctions : System.MarshalByRefObject
    {

        // Część 1
        // Wyznaczanie odwrotności grafu
        //   0.5 pkt
        // Odwrotność grafu to graf skierowany o wszystkich krawędziach przeciwnie skierowanych niż w grafie pierwotnym
        // Parametry:
        //   g - graf wejściowy
        // Wynik:
        //   odwrotność grafu
        // Uwagi:
        //   1) Metoda uruchomiona dla grafu nieskierowanego powinna zgłaszać wyjątek Lab03Exception
        //   2) Graf wejściowy pozostaje niezmieniony
        //   3) Graf wynikowy musi być w takiej samej reprezentacji jak wejściowy
        public Graph Lab03Reverse(Graph g)
        {
            if (!g.Directed) { throw new Lab03Exception(); }
            Graph result = g.IsolatedVerticesGraph();
            int size = g.VerticesCount;
            for(int i = 0; i < size; i++)
            {
                foreach(var x in g.OutEdges(i))
                {
                    result.Add(new Edge(x.To, x.From, x.Weight));
                }
            }
            return result;
        }

        // Część 2
        // Badanie czy graf jest dwudzielny
        //   0.5 pkt
        // Graf dwudzielny to graf nieskierowany, którego wierzchołki można podzielić na dwa rozłączne zbiory
        // takie, że dla każdej krawędzi jej końce należą do róźnych zbiorów
        // Parametry:
        //   g - badany graf
        //   vert - tablica opisująca podział zbioru wierzchołków na podzbiory w następujący sposób
        //          vert[i] == 1 oznacza, że wierzchołek i należy do pierwszego podzbioru
        //          vert[i] == 2 oznacza, że wierzchołek i należy do drugiego podzbioru
        // Wynik:
        //   true jeśli graf jest dwudzielny, false jeśli graf nie jest dwudzielny (w tym przypadku parametr vert ma mieć wartość null)
        // Uwagi:
        //   1) Metoda uruchomiona dla grafu skierowanego powinna zgłaszać wyjątek Lab03Exception
        //   2) Graf wejściowy pozostaje niezmieniony
        //   3) Podział wierzchołków może nie być jednoznaczny - znaleźć dowolny
        //   4) Pamiętać, że każdy z wierzchołków musi być przyporządkowany do któregoś ze zbiorów
        //   5) Metoda ma mieć taki sam rząd złożoności jak zwykłe przeszukiwanie (za większą będą kary!)
        public bool Lab03IsBipartite(Graph g, out int[] vert)
        {
            if (g.Directed) { throw new Lab03Exception(); }
            int size = g.VerticesCount;
            vert = new int[size];
            for (int i = 0; i < size; i++)
            {
                foreach (var x in g.OutEdges(i))
                {
                    if (vert[x.To] == 0 && vert[i] != 0)
                        vert[x.To] = vert[i] == 1 ? 2 : 1;
                    else if (vert[x.To] != 0 && vert[i] == 0)
                        vert[i] = vert[x.To] == 1 ? 2 : 1;
                    else if ((vert[x.To] == 1 && vert[i] == 1) || (vert[x.To] == 2 && vert[i] == 2))
                    {
                        vert = null;
                        return false;
                    }
                }
                if (vert[i] == 0)
                    vert[i] = 1;
            }
            return true;
        }

        // Część 3
        // Wyznaczanie minimalnego drzewa rozpinającego algorytmem Kruskala
        //   1 pkt
        // Schemat algorytmu Kruskala
        //   1) wrzucić wszystkie krawędzie do "wspólnego worka"
        //   2) wyciągać z "worka" krawędzie w kolejności wzrastających wag
        //      - jeśli krawędź można dodać do drzewa to dodawać, jeśli nie można to ignorować
        //      - punkt 2 powtarzać aż do skonstruowania drzewa (lub wyczerpania krawędzi)
        // Parametry:
        //   g - graf wejściowy
        //   mstw - waga skonstruowanego drzewa (lasu)
        // Wynik:
        //   skonstruowane minimalne drzewo rozpinające (albo las)
        // Uwagi:
        //   1) Metoda uruchomiona dla grafu skierowanego powinna zgłaszać wyjątek Lab03Exception
        //   2) Graf wejściowy pozostaje niezmieniony
        //   3) Wykorzystać klasę UnionFind z biblioteki Graph
        //   4) Jeśli graf g jest niespójny to metoda wyznacza las rozpinający
        //   5) Graf wynikowy (drzewo) musi być w takiej samej reprezentacji jak wejściowy
        public Graph Lab03Kruskal(Graph g, out double mstw)
        {
            if (g.Directed) { throw new Lab03Exception(); }
            var minQ = new EdgesMinPriorityQueue();
            int size = g.VerticesCount;
            for (int i = 0; i < size; i++)
                foreach (var x in g.OutEdges(i))
                    if (x.From < x.To)
                        minQ.Put(x);

            var Union = new UnionFind(g.VerticesCount);
            Graph result = g.IsolatedVerticesGraph();

            mstw = 0;
            while (!minQ.Empty)
            {
                var minEdge = minQ.Get();

                if (Union.Union(minEdge.From, minEdge.To))
                {
                    mstw += minEdge.Weight;
                    result.Add(minEdge);
                }
            }

            return result;
        }

        // Część 4
        // Badanie czy graf nieskierowany jest acykliczny
        //   0.5 pkt
        // Parametry:
        //   g - badany graf
        // Wynik:
        //   true jeśli graf jest acykliczny, false jeśli graf nie jest acykliczny
        // Uwagi:
        //   1) Metoda uruchomiona dla grafu skierowanego powinna zgłaszać wyjątek Lab03Exception
        //   2) Graf wejściowy pozostaje niezmieniony
        //   3) Najpierw pomysleć jaki, prosty do sprawdzenia, warunek spełnia acykliczny graf nieskierowany
        //      Zakodowanie tego sprawdzenia nie powinno zająć więcej niż kilka linii!
        //      Zadanie jest bardzo łatwe (jeśli wydaje się trudne - poszukać prostszego sposobu, a nie walczyć z trudnym!)
        public bool Lab03IsUndirectedAcyclic(Graph g)
        {
            if (g.Directed) { throw new Lab03Exception(); }
            if (g.VerticesCount < g.EdgesCount - 1)
                return false;

            g.GeneralSearchAll<EdgesStack>(null, null, egde =>
            {
                return true;
            }, out int numberOfCoherence);

            return g.VerticesCount == numberOfCoherence + g.EdgesCount;
        }

    }

}
