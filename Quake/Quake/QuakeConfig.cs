using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake
{
    public enum QuakeSortOrder { Nearest = 0, ByMagnitude, ByTime };


    public class QuakeConfig
    {
        public QuakeConfig()
        {
            SortOrder = QuakeSortOrder.Nearest;         // Our default sort order.
        }

        public QuakeSortOrder SortOrder { get; set; }
    }
}
