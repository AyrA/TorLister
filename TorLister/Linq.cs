using System;
using System.Collections.Generic;
using System.Linq;

namespace TorLister
{
    /// <summary>
    /// Linq Extensions
    /// </summary>
    public static class LinqExt
    {
        /// <summary>
        /// RNG
        /// </summary>
        private static Random R = new Random();

        /// <summary>
        /// Returns a randomly selected Element from the Collection
        /// </summary>
        /// <typeparam name="T">Element Type</typeparam>
        /// <param name="Elements">Elements</param>
        /// <returns>Random Element</returns>
        public static T Random<T>(this IEnumerable<T> Elements)
        {
            //Handle null case
            if (Elements == null)
            {
                return default(T);
            }
            var Count = Elements.Count();
            //Handle 0 or 1 item
            if (Count < 2)
            {
                return Elements.FirstOrDefault();
            }
            //Chose randomly
            //Note: Don't use (Count-1) because otherwise the last element will never be selected
            return Elements.Skip(R.Next(Count)).First();
        }
    }
}
