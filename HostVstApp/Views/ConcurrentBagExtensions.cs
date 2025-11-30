using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HostVstApp;

public static class ConcurrentBagExtensions
{
   public static bool Remove<T>(this ConcurrentBag<T> bag, T item)
   {
      var temp = new List<T>();
      bool removed = false;
      T element;

      while (bag.TryTake(out element!))
      {
         // Skip the first matching item
         if (!removed && EqualityComparer<T>.Default.Equals(element, item))
         {
            removed = true;
            continue;
         }
         temp.Add(element);
      }

      // Put back others
      foreach (var e in temp)
         bag.Add(e);

      return removed;
   }
}
