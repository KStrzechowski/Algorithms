namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab04 : System.MarshalByRefObject
    {
        /// <summary>
        /// Część I: wyznaczenie zbioru potencjalnie zarazonych obywateli.
        /// </summary>
        /// <param name="Z">Zbiór osób zarażonych na początku epidemii, uporządkowany rosnąco</param>
        /// <param name="G">Informacja o spotkaniach; wagi krawędzi są nieujemne i oznaczają czas spotkania</param>
        /// <returns>Lista potencjalnie zarażonych obywateli, uporządkowana rosnąco</returns>
        public List<int> QuarantineTargets(List<int> Z, Graph G)
        {
            // Przedstawione złożoności pojedyńczych operacji wynikają z dokumentacji Microsoft.
            // Tworzymy słownik, w którym dla każdej potencjalnie zarażonej osoby przechowujemy minimalny czas jej zarażenia.
            var P = new Dictionary<int, double>();
            var minQ = new EdgesMinPriorityQueue();
            foreach (var person in Z)
            {
                P.Add(person, 0);
            }

            // Wstawiamy wszystkie krawędzie do kolejki priorytetowej, gdzie najmniejsza waga ma największy priorytet.
            int size = G.VerticesCount;
            for (int i = 0; i < size; i++)
                foreach (var x in G.OutEdges(i))
                    if (x.From < x.To)
                        minQ.Put(x);

            // Po kolei wyciągamy każdą krawędź.
            while (!minQ.Empty)
            {
                Edge x = minQ.Get();

                // Sprawdzamy czy któraś ze spotykających się osób jest potencjalnie zarażona.
                if (P.ContainsKey(x.From)) // Oczekiwana złożoność operacji O(1) https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.containskey?view=net-5.0.
                {
                    if (!P.ContainsKey(x.To))
                    {
                        // Sprawdzamy czy osoba mogła zarazić się wcześniej czy w tym samym momencie co czas spotkania (jeśli w tym samym, to nie przekazuje wirusa dalej).
                        P.TryGetValue(x.From, out double time); // Oczekiwana złożoność operacji O(1) https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.trygetvalue?view=net-5.0.
                        if (time < x.Weight)
                        {
                            {
                                Z.Add(x.To);
                                P.Add(x.To, x.Weight);
                            }
                        }
                    }
                }
                else if (P.ContainsKey(x.To))
                {
                    P.TryGetValue(x.To, out double time);
                    if (time < x.Weight)
                    {
                        {
                            Z.Add(x.From);
                            P.Add(x.From, x.Weight);
                        }
                    }
                }
            }
            Z.Sort(); // Złożoność operacji O(n log(n)) https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=net-5.0.

            return Z;
        }

        /// <summary>
        /// Część II: wyznaczenie zbioru potencjalnych pacjentów zero.
        /// </summary>
        /// <param name="S">Zbiór osób zakażonych przez potencjalnego pacjenta zero</param>
        /// <param name="G">Informacja o spotkaniach; wagi krawędzi są nieujemne i oznaczają czas spotkania</param>
        /// <returns>Lista potencjalnych pacjentów zero, uporządkowana rosnąco</returns>

        public List<int> PotentialPatientsZero(List<int> S, Graph G)
        {
            int size = G.VerticesCount;
            var Result = new List<int>();
            var maxQ = new EdgesMaxPriorityQueue();
            var potentialPatientsZero = new bool[size, S.Count]; // Dla każdej osoby w grafie zapisujemy czy mogła zarazić osobę z potwierdzonych już przypadków.

            // Wstawiamy wszystkie krawędzie do kolejki priorytetowej, gdzie największa waga ma największy priorytet.
            for (int i = 0; i < size; i++)
                foreach (var x in G.OutEdges(i))
                    if (x.From < x.To)
                        maxQ.Put(x);


            double weight;
            double prevWeight = -1;
            var Temp = new HashSet<(int, int)>(); // zapisujemy, które osoby mogły zarażać w czasie t.

            while (!maxQ.Empty)
            {
                int i;
                Edge x = maxQ.Get();
                weight = x.Weight;

                if (weight != prevWeight) // jeśli waga krawędzi jest inna, to czyścimy zbiór, ponieważ w następnym kroku będziemy rozważać zachorowania w czasie t - 1.
                    Temp.Clear(); // Złożoność operacji wynosi O(n), ale każdą z krawędzi możemy dodać i usunąć tylko raz, stąd łączna złożoność wyciągania krawędzi z kolejki, wstawianie jej
                                  // do zbioru Temp i czyszczenie tego zbioru wynosi też O(n).  https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1.clear?view=net-5.0.

                // Jeśli dana osoba mogła zarazić pewną osobę ze zbioru S w czasie > t, to sama mogła zarazić się od osoby którą spotkała w czasie = t.
                if ((i = S.BinarySearch(x.From)) >= 0) // Złożoność operacji wynosi O(1) dla |S| <= 30.
                {
                    potentialPatientsZero[x.From, i] = true;
                }
                if ((i = S.BinarySearch(x.To)) >= 0)
                {
                    potentialPatientsZero[x.To, i] = true;
                }
                for (i = 0; i < S.Count; i++)
                {
                    if (potentialPatientsZero[x.From, i] && !Temp.Contains((i, x.From))) // Złożoność operacji O(1) https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1.contains?view=net-5.0.
                    {
                        if (!potentialPatientsZero[x.To, i])
                        {
                            Temp.Add((i, x.To));
                            potentialPatientsZero[x.To, i] = true;
                        }
                    }
                    else if (potentialPatientsZero[x.To, i] && !Temp.Contains((i, x.To)))
                    {
                        Temp.Add((i, x.From));
                        potentialPatientsZero[x.From, i] = true;
                    }
                }
                prevWeight = weight;
            }

            // Sprawdzamy czy wszystkie z zarażonych osób j zawierajacych się w zbiorze S mogły zostać zarażone przez osobę i.
            bool check;
            for (int i = 0; i < size; i++)
            {
                check = true;
                for (int j = 0; j < S.Count; j++)
                    if (!potentialPatientsZero[i, j])
                    {
                        check = false;
                        break;
                    }
                if (check)
                    Result.Add(i);
            }

            return Result;
        }
    }

}
