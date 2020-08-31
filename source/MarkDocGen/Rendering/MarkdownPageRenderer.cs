using System;
using System.IO;
using DefaultDocumentation;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   class MarkdownPageRenderer<T> : PageRenderer<T> where T : DocItem
   {
      private readonly Action<RenderingContext, T, MarkdownWriter> m_renderFunc;

      public MarkdownPageRenderer(Action<RenderingContext, T, MarkdownWriter> renderFunc, Func<T, string> fileName, Predicate<T> supports = null, Predicate<T> isLinkTarget = null) 
         : base(fileName, supports, isLinkTarget)
      {
         m_renderFunc = renderFunc;
      }

      public override void RenderPage(RenderingContext context, T item, TextWriter writer)
      {
         using (MarkdownWriter mdWriter = new MarkdownWriter(writer))
         {
            m_renderFunc(context, item, mdWriter);
         }         
      }
   }
}
