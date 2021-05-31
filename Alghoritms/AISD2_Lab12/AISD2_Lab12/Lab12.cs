using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public class Lab12 : System.MarshalByRefObject
    {
        // etap 1

        /// <summary>
        /// Verify if a given polygon is y-monotone
        /// </summary>
        /// <param name="points">An array of points defining polygon, given in clock-wise manner.</param>
        /// <returns>
        ///     true - a given polygon is y-monotone, otherwise false is returned
        /// </returns>

        private (int, bool) check(double y1, double y2, int count, bool isGrowing)
        {
            if (y1 < y2)
            {
                if (!isGrowing)
                {
                    isGrowing = true;
                    count++;
                }
            }
            else if (y1 > y2)
            {
                if (isGrowing)
                {
                    isGrowing = false;
                    count++;
                }
            }
            return (count, isGrowing);
        }

        public bool IsYMonotonePolygon((double, double)[] points)
        {
            int n = points.Length;
            int count = 0;
            bool isGrowing = true;

            int min = 0;
            for (int j = 1; j < n; j++)
                if (points[min].Item2 > points[j].Item2)
                    min = j;

            int i = min + 1;
            if (min == n - 1)
                i = 0;
            while (i != min)
            {
                if (i == n - 1)
                {
                    i = 0;
                    (count, isGrowing) = check(points[n - 1].Item2, points[0].Item2, count, isGrowing);
                }
                else
                {
                    (count, isGrowing) = check(points[i].Item2, points[i + 1].Item2, count, isGrowing);
                    i++;
                }
            }
            if (min == n - 1)
                (count, isGrowing) = check(points[n - 1].Item2, points[0].Item2, count, isGrowing);
            else
                (count, isGrowing) = check(points[min].Item2, points[min + 1].Item2, count, isGrowing);

            if (count > 2)
                return false;
            else
                return true;
        }

        // etap 2

        private double CrossLike((double, double) p1, (double, double) p2, (double, double) p3)
        {
            return (p2.Item2 - p1.Item2) * (p3.Item1 - p2.Item1) + (p1.Item1 - p2.Item1) * (p3.Item2 - p2.Item2);
        }

        /// <summary>
        /// Function triangulating a given edge-visible polygon.
        /// </summary>
        /// <param name="points">An array of points defining polygon, given in clock-wise manner.</param>
        /// <param name="edge_i">An index of point which is defining an edge from which a polygon is visible.</param>
        /// <param name="edge_j">An index of point which is defining an edge from which a polygon is visible.</param>
        /// <returns>
        ///     A list of 3-length arrays defining triangles. An order of points in triangle does not matter.
        /// </returns>

        private bool check2(List<(double, double)> points, int x, int y, int z)
        {
            if (CrossLike(points[x], points[y], points[z]) > 0)
                return true;
            else
                return false;
        }

        public List<(double, double)[]> TriangulationForEdgeVisiblePolygon((double, double)[] points, int edge_i, int edge_j)
        {
            // nalezy zalozyc, ze wierzcholki podane sa zgodnie z ruchem wskazowek zegara

            int n = points.Length;
            var result = new List<(double, double)[]>();
            // Lista przechowująca wierzchołki od początkowego do końcowego
            var listPoints = new List<(double, double)>();
            for (int i = edge_i; i < n; i++)
                listPoints.Add(points[i]);
            for (int i = 0; i < edge_i; i++)
                listPoints.Add(points[i]);

            int p = 0, q = 1, r = 2;
            while (true)
            {
                if (listPoints.Count < 3)
                    break;

                if (check2(listPoints, p, q, r))
                {
                    result.Add(new (double, double)[3] { listPoints[p], listPoints[q], listPoints[r] });
                    listPoints.RemoveAt(q);
                    if (r == 0)
                        break;
                    if (p != 0)
                    {
                        p--;
                        q--;
                        r--;
                    }
                }
                else
                {
                    if (r == 0)
                        break;
                    p++;
                    q++;
                    r++;
                    if (r >= listPoints.Count)
                        r = 0;
                    if (q >= listPoints.Count)
                        q = 0;
                    if (p >= listPoints.Count)
                        p = 0;
                }
            }

            return result;
        }

        // etap 3

        /// <summary>
        /// Function triangulating a given y-monotone polygon.
        /// </summary>
        /// <param name="points">An array of points defining polygon, given in clock-wise manner.</param>
        /// <returns>
        ///     A list of 3-length arrays defining triangles. An order of points in triangle does not matter.
        /// </returns>

        // Wyznaczamy 2 ścieżki będące lewym i prawym bokiem wielokąta. Łączą one minimalny i maksymalny wierzchołek
        public (List<(double, double)>, List<(double, double)>) FindCorrectLists((double, double)[] points, int min, int max)
        {
            int n = points.Length;
            var Right = new List<(double, double)>();
            var Left = new List<(double, double)>();

            int i = min;
            while (i != max)
            {
                Left.Add(points[i]);
                i++;
                if (i == n)
                    i = 0;
            }
            Left.Add(points[i]);

            i = max;
            while (i != min)
            {
                Right.Add(points[i]);
                i++;
                if (i == n)
                    i = 0;
            }
            Right.Add(points[i]);
            return (Left, Right);
        }

        // Łączymy znalezione wcześniej ścieżki, tak aby otrzymać ciąg wierzchołków posortowanych względem ich wysokości
        public List<(double, double)> FindConcatenatedList(List<(double, double)> Left, List<(double, double)> Right)
        {
            var concatenatedList = new List<(double, double)>();
            int p = 1;
            int q = Right.Count - 1;

            while (p < Left.Count || q > 0)
            {
                if (q == 0)
                {
                    concatenatedList.Add(Left[p]);
                    p++;
                }
                else if (p == Left.Count)
                {
                    concatenatedList.Add(Right[q]);
                    q--;
                }
                else if (Left[p].Item2 < Right[q].Item2)
                {
                    concatenatedList.Add(Left[p]);
                    p++;
                }
                else
                {
                    concatenatedList.Add(Right[q]);
                    q--;
                }
            }
            return concatenatedList;
        }

        // Po kolei znajdujemy diagonale i wywołujemy dla otrzymanych "małych" wielokątów rozwiązanie etapu 2
        public List<(double, double)[]> DivideIntoFigures(List<(double, double)> concatenatedList, List<(double, double)> Left, List<(double, double)> Right)
        {
            var temp = new List<(double, double)>(); // Przechowujem w liście tymczasowe wierzchołki, które będą składały się na "mały" wielokąt
            var result = new List<(double, double)[]>();
            var triangles = new List<(double, double)[]>(); // Pomocnicza lista trójkątów, które przepisujemy do result

            bool leftSide;
            if (Left.Contains(concatenatedList[1]))
                leftSide = true;
            else
                leftSide = false;

            temp.Add(concatenatedList[1]);
            var starting = concatenatedList[0];

            for (int i = 2; i < concatenatedList.Count - 1; i++)
            {
                if (leftSide)
                {
                    // Jeśli jesteśmy na lewej stronie to sprawdzamy czy obecny wierzchołek nie jest na prawej stronie.
                    if (Right.Contains(concatenatedList[i]))
                    {
                        leftSide = false;
                        temp.Add(concatenatedList[i]);
                        temp.Add(starting);

                        triangles = TriangulationForEdgeVisiblePolygon(temp.ToArray(), temp.Count - 1, temp.Count - 2);
                        for (int j = 0; j < triangles.Count; j++)
                            result.Add(triangles[j]);
                        temp.Clear();
                        starting = concatenatedList[i - 1];
                        temp.Add(concatenatedList[i]);
                    }
                    else
                    {
                        temp.Add(concatenatedList[i]);
                    }
                }
                else
                {
                    // Jeśli jesteśmy na prawej stronie to sprawdzamy czy obecny wierzchołek nie jest na lewej stronie.
                    if (Left.Contains(concatenatedList[i]))
                    {
                        // Funkcja odwraca tylko wierzchołki składające się na "mały" wielokąt - ma taką samą złożoność jak etap 2.
                        // Nie zwiększa złożoności całego algorytmu (O(n)).
                        temp.Reverse(); 
                        temp.Add(starting);
                        temp.Add(concatenatedList[i]);

                        triangles = TriangulationForEdgeVisiblePolygon(temp.ToArray(), temp.Count - 1, temp.Count - 2);
                        for (int j = 0; j < triangles.Count; j++)
                            result.Add(triangles[j]);
                        temp.Clear();

                        leftSide = true;
                        starting = concatenatedList[i - 1];
                        temp.Add(concatenatedList[i]);
                    }
                    else
                    {
                        temp.Add(concatenatedList[i]);
                    }
                }
            }

            // Wywołujemy funkcję dla ostatniego górnego wielokąta
            if (leftSide)
            {
                temp.Add(concatenatedList[concatenatedList.Count - 1]);
                temp.Add(starting);
                triangles = TriangulationForEdgeVisiblePolygon(temp.ToArray(), temp.Count - 1, temp.Count - 2);
            }
            else
            {
                temp.Reverse();
                temp.Add(starting);
                temp.Add(concatenatedList[concatenatedList.Count - 1]);
                triangles = TriangulationForEdgeVisiblePolygon(temp.ToArray(), temp.Count - 1, temp.Count - 2);
            }

            for (int j = 0; j < triangles.Count; j++)
                result.Add(triangles[j]);
            return result;
        }

        public List<(double, double)[]> TriangulationForYMonotonePolygon((double, double)[] points)
        {
            // nalezy zalozyc, ze wierzcholki podane sa zgodnie z ruchem wskazowek zegara
            int n = points.Length;
            int min = 0;
            int max = 0;
            for (int i = 1; i < n; i++)
            {
                if (points[i].Item2 < points[min].Item2)
                    min = i;
                if (points[i].Item2 > points[max].Item2)
                    max = i;
            }

            var Left = new List<(double, double)>();
            var Right = new List<(double, double)>();
            (Left, Right) = FindCorrectLists(points, min, max);
            var concatenatedList = FindConcatenatedList(Left, Right);
            var result = DivideIntoFigures(concatenatedList, Left, Right);

            return result;
        }
    }
}