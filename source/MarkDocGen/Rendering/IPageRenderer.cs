using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   interface IPageRenderer
   {
      string GetFileName(DocItem item);
      void RenderPage(RenderingContext context, DocItem item, TextWriter writer);
      bool Supports(DocItem item);
      bool IsLinkTarget(DocItem item);
   }
}
