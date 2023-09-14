// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices.ComTypes;

class OfflineStrike
{
    private static int dayCount;
    
    private static int[] maxSeats;
    private static int[] departureCosts;
    private static int[] hotelCosts;
        
    private static int theoreticalMin = Int32.MaxValue;
    private static int globalMin = Int32.MaxValue;

    private static int seatsLeft;


    public static void Main()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        string file = Path.Combine(currentDir, @"..\..\..\test.txt");
        List<string> text = File.ReadLines(file).ToList();
        
        dayCount = Int32.Parse(text[1]);
        
        maxSeats = new int[dayCount];
        departureCosts = new int[dayCount];
        hotelCosts = new int[dayCount];
        
        theoreticalMin = Int32.MaxValue;
        globalMin = Int32.MaxValue;
        
        int[][] decTree = new int[dayCount][];
        for (int i = 0; i < decTree.Length; i++)
        {
            decTree[i] = new int[2];
        }

        for (int i = 2; i < text.Count; i++)
        {
            string[] textLine = text[i].Split(", ");
            maxSeats[i - 2] = Int32.Parse(textLine[0]);
            departureCosts[i - 2] = Int32.Parse(textLine[1]);
            hotelCosts[i - 2] = Int32.Parse(textLine[2]);
        }

        (int, int[][]) result = DFS(0, 0, maxSeats.Sum(), Int32.Parse(text[0]), decTree);
        
        foreach (int[] ints in result.Item2)
        {
            Console.WriteLine($"{ints[0]}, {ints[1]}");
        }
        
        Console.WriteLine("Minimum cost: " + result.Item1);
    }

    private static (int, int[][]) DFS(int index, int currentCost, int seatsLeft, int peopleRemaining, int[][] decTree)
    {
        //Console.WriteLine("day: " + index + " | people left: " + peopleRemaining + " | currentCost: " + currentCost);
        
        int localMin = Int32.MaxValue;
        
        if (seatsLeft == 0 || peopleRemaining == 0) return (currentCost, decTree);
        
        int startIndex = seatsLeft - maxSeats[index] < peopleRemaining ? peopleRemaining - (seatsLeft - maxSeats[index]) : 0;
        
        for (int i = startIndex; i <= maxSeats[index] && i <= peopleRemaining; i++)
        {
            (int, int[][]) result = DFS(index + 1, currentCost + departureCosts[index] * i + hotelCosts[index] * (peopleRemaining - i),
                seatsLeft - maxSeats[index], peopleRemaining - i, decTree);
            
            if (result.Item1 < localMin)
            {
                if (result.Item1 <= globalMin)
                {
                    result.Item2[index][0] = i;
                    result.Item2[index][1] = peopleRemaining;
                    globalMin = result.Item1;
                }
                
                localMin = result.Item1;
            }
        }
        
        return (localMin, decTree);
    }
}

