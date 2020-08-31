using System;
using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   class TextPageRenderer<T> : PageRenderer<T> where T : DocItem
   {
      private readonly Action<RenderingContext, T, TextWriter> m_renderFunction;

      public TextPageRenderer(Action<RenderingContext, T, TextWriter> renderFunction, Func<T, string> fileName, Predicate<T> supports = null, Predicate<T> isLinkTarget = null)
         : base(fileName, supports, isLinkTarget)
      {
         m_renderFunction = renderFunction;
      }

      public override void RenderPage(RenderingContext context, T item, TextWriter writer) => m_renderFunction(context, item, writer);
   }
}
