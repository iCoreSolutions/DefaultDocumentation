using System.Collections.Generic;
using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   interface ITemplate
   {
      IReadOnlyList<IPageRenderer> PageRenderers { get; }
      string GetDisplayName(DocItem item);    
   }
}
