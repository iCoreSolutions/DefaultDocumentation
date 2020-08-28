using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   // TODO PP (2020-08-25): Handle compiler generated members, and accessibility configuration (i.e. only export public members etc)
   interface ITemplate
   {
      PageInfo GetPageInfo(DocItem item);
      string GetDisplayName(DocItem item);
      string RenderInlineCode(RenderingContext context, string content);
      string RenderCodeBlock(RenderingContext context, string code);
      string RenderLink(RenderingContext context, ILinkModel link);
      void RenderPage(RenderingContext context, TextWriter writer);
      string RenderParagraph(RenderingContext context, string content);
      string RenderParamRef(RenderingContext context, InternalLinkModel link);
      string RenderText(RenderingContext context, string text);
   }
}
