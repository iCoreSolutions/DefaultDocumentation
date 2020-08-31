using System.Collections.Generic;
using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   // TODO PP (2020-08-25): Handle compiler generated members, and accessibility configuration (i.e. only export public members etc)
   interface ITemplate
   {
      IReadOnlyList<IPageRenderer> PageRenderers { get; }
      string GetDisplayName(DocItem item);
      //string 
      //string RenderInlineCode(RenderingContext context, string content);
      //string RenderCodeBlock(RenderingContext context, string code);
      //string RenderLink(RenderingContext context, ILinkModel link);
      ////void RenderPage(RenderingContext context, TextWriter writer);
      //string RenderParagraph(RenderingContext context, string content);
      //string RenderParamRef(RenderingContext context, ILinkModel link);
      //string RenderText(RenderingContext context, string text);
   }
}
