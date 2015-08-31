using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RandomImage
{
    public static class ListExtensions
    {
        public static List<T> Shuffle<T>(this List<T> list)
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available Bytes");
            double seed = Math.Abs(DateTime.Now.Millisecond - Math.Round(ramCounter.NextValue()));
            if (seed >= Int32.MaxValue)
                seed -= Math.Round(seed / Int32.MaxValue) * Int32.MaxValue;

            Random rng = new Random(Convert.ToInt32(seed));
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(list.Count);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}