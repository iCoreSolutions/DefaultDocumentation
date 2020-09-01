using System.Collections.Generic;
using System.Xml.Linq;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   interface IDocItem
   {
      string AnchorId { get; }
      IEnumerable<DocItem> Children { get; }
      XElement Documentation { get; }
      string Id { get; }
      DocItemKind Kind { get; }
   }
}
