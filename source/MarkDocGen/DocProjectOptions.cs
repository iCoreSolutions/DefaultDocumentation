using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkDocGen
{
   class DocProjectOptions
   {
      public static readonly DocProjectOptions Default = new DocProjectOptions();

      public bool IncludeCompilerGeneratedTypes { get; set; }
   }
}
