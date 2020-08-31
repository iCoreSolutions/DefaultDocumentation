using System.Collections.Generic;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   abstract class TemplateBase : ITemplate
   {
      private readonly List<IPageRenderer> m_pageRenderers = new List<IPageRenderer>();

      public TemplateBase()
      {
      }

      public IReadOnlyList<IPageRenderer> PageRenderers => m_pageRenderers;

      protected void AddRenderer(IPageRenderer renderer)
      {
         m_pageRenderers.Add(renderer);
      }

      public abstract string GetDisplayName(DocItem item);      
      //public abstract string RenderCodeBlock(RenderingContext context, string code);
      //public abstract string RenderInlineCode(RenderingContext context, string content);
      //public abstract string RenderLink(RenderingContext context, ILinkModel link);
      //public abstract string RenderParagraph(RenderingContext context, string content);
      //public abstract string RenderParamRef(RenderingContext context, ILinkModel link);
      //public abstract string RenderText(RenderingContext context, string text);
   }
}
