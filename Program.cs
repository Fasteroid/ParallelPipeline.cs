

using Common;
using System;


List<int> victims = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

var results = await ParallelPipeline<int, int>.Foreach(victims, (int victim) => { 
        RandomWaiter.Wait();
        Console.WriteLine($"{victim} did step 0...");
        return victim;
    }).Next( (int victim) => {
        RandomWaiter.Wait();
        Console.WriteLine($"{victim} did step 1...");
        return victim;
    }).Next( (int victim) => {
        RandomWaiter.Wait();
        Console.WriteLine($"{victim} did step 2...");
        return victim;
    }).Return();

Console.WriteLine(string.Join(", ", results)); // 9, 15, 21, 27, 33, 39, 45, 51, 57, 63


public static class RandomWaiter {
    public static void Wait() {
        Random random = new Random();
        Thread.Sleep(random.Next(1000, 2000));
    }
}