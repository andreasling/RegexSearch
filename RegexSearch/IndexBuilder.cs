using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexSearch
{
    public interface IndexBuilder
    {
        Index BuildIndex(Action<int> reportProgress);
    }
}
