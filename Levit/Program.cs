namespace Levit
{
    /// <summary>
    /// Представляет ориентированный взвешенный граф и алгоритм поиска кратчайших путей.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Представляет дугу графа с начальной вершиной, конечной вершиной и весом.
        /// </summary>
        private class Arc
        {
            /// <summary>
            /// Начальная вершина дуги.
            /// </summary>
            public int From { get; set; }

            /// <summary>
            /// Конечная вершина дуги.
            /// </summary>
            public int To { get; set; }

            /// <summary>
            /// Вес дуги.
            /// </summary>
            public int Weight { get; set; }

            /// <summary>
            /// Инициализирует новую дугу графа.
            /// </summary>
            /// <param name="from">Начальная вершина.</param>
            /// <param name="to">Конечная вершина.</param>
            /// <param name="weight">Вес дуги.</param>
            public Arc(int from, int to, int weight)
            {
                From = from;
                To = to;
                Weight = weight;
            }
        }

        private readonly int _verticesCount;

        /// <summary>
        /// Количество вершин в графе.
        /// </summary>
        public int VerticesCount => _verticesCount;

        /// <summary>
        /// Список смежности для хранения дуг графа.
        /// </summary>
        private List<Arc>[] AdjacencyList { get; }

        /// <summary>
        /// Инициализирует новый граф с указанным количеством вершин.
        /// </summary>
        /// <param name="verticesCount">Количество вершин в графе.</param>
        public Graph(int verticesCount)
        {
            _verticesCount = verticesCount;
            AdjacencyList = new List<Arc>[verticesCount];
            for (int i = 0; i < verticesCount; i++) AdjacencyList[i] = [];
        }

        /// <summary>
        /// Добавляет новую дугу в граф.
        /// </summary>
        /// <param name="from">Начальная вершина дуги.</param>
        /// <param name="to">Конечная вершина дуги.</param>
        /// <param name="weight">Вес дуги.</param>
        public void AddArc(int from, int to, int weight) => AdjacencyList[from].Add(new Arc(from, to, weight));

        /// <summary>
        /// Загружает граф из файла с матрицей смежности.
        /// </summary>
        /// <param name="filePath">Путь к файлу с матрицей смежности.</param>
        /// <param name="noArc">Символ, обозначающий отсутствие дуги (по умолчанию "-").</param>
        /// <returns>Загруженный граф.</returns>
        /// <exception cref="Exception">Выбрасывается при ошибках чтения файла или неверном формате данных.</exception>
        public static Graph LoadFromMatrixFile(string filePath, string noArc = "-")
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) throw new Exception("Файл пуст");

            int verticesCount = lines.Length;
            Graph graph = new(verticesCount);

            for (int i = 0; i < verticesCount; i++)
            {
                string[] parts = lines[i].Split([' ', '\t', ','], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != verticesCount)
                    throw new Exception($"Некорректная строка {i + 1}: ожидается {verticesCount} значений");

                for (int j = 0; j < verticesCount; j++)
                {
                    if (parts[j] == noArc) continue;

                    int weight = int.Parse(parts[j]);
                    if (i != j) graph.AddArc(i, j, weight);
                }
            }
            return graph;
        }

        /// <summary>
        /// Выполняет поиск кратчайших путей из заданной вершины с использованием алгоритма Левита.
        /// </summary>
        /// <param name="start">Начальная вершина для поиска путей.</param>
        /// <returns>Словарь, где ключ - номер конечной вершины, значение - длина кратчайшего пути из начальной вершины до текущей.</returns>
        public Dictionary<int, int> LevitsAlgorythm(int start)
        {
            Dictionary<int, int> dist = Enumerable.Range(0, VerticesCount)
                         .ToDictionary(i => i, _ => int.MaxValue);

            HashSet<int> processed = [];                //M0
            Queue<int> mainQueue = [];                  //M1'
            Queue<int> urgentQueue = [];                //M1''
            HashSet<int> unprocessed = new(dist.Keys);  //M2

            unprocessed.Remove(start);
            dist[start] = 0;

            mainQueue.Enqueue(start);

            while (mainQueue.Count > 0 || urgentQueue.Count > 0)
            {
                int current;

                if (urgentQueue.Count > 0) current = urgentQueue.Dequeue();
                else current = mainQueue.Dequeue();

                foreach (Arc arc in AdjacencyList[current])
                {
                    int neighbour = arc.To;
                    int newDist = dist[current] + arc.Weight;

                    if (newDist < dist[neighbour])
                    {
                        dist[neighbour] = newDist;

                        if (processed.Contains(neighbour))
                        {
                            processed.Remove(neighbour);
                            urgentQueue.Enqueue(neighbour);
                        }

                        if (unprocessed.Contains(neighbour))
                        {
                            unprocessed.Remove(neighbour);
                            mainQueue.Enqueue(neighbour);
                        }
                    }
                }
                processed.Add(current);
            }

            return dist;
        }
    }

    public class Program
    {
        public static void Main()
        {
            try
            {
                string filePath = @"matrix.txt";
                Graph graph = Graph.LoadFromMatrixFile(filePath);

                Console.WriteLine($"Введите стартовую вершину (от 1 до {graph.VerticesCount}):");
                int start = int.Parse(Console.ReadLine());

                Dictionary<int, int> distances = graph.LevitsAlgorythm(start - 1);

                Console.WriteLine($"\nКратчайшие расстояния от вершины {start}:");
                foreach (int vertex in distances.Keys)
                    Console.WriteLine($"До {vertex + 1}: " + (distances[vertex] == int.MaxValue ? "нет пути" : distances[vertex]));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}