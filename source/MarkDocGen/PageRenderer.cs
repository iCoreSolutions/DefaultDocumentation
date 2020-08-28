using System;
using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   class PageRenderer<T> : IPageRenderer where T : DocItem
   {
      private readonly Action<RenderingContext, T, TextWriter> m_renderFunction;
      private readonly Func<T, string> fileName;
      private readonly Predicate<T> m_supports;
      private readonly Predicate<T> m_isLinkTarget;

      public PageRenderer(Action<RenderingContext, T, TextWriter> renderFunction, Func<T, string> fileName, Predicate<T> supports = null, Predicate<T> isLinkTarget = null)
      {
         m_isLinkTarget = isLinkTarget;
         m_renderFunction = renderFunction;
         this.fileName = fileName;
         m_supports = supports;
      }

      bool IPageRenderer.Supports(DocItem item)
      {
         return item is T typedItem && Supports(typedItem);
      }

      bool IPageRenderer.IsLinkTarget(DocItem item) => item is T typedItem && IsLinkTarget(typedItem);

      public bool Supports(T item) => m_supports?.Invoke(item) ?? true;

      public bool IsLinkTarget(T item) => m_isLinkTarget?.Invoke(item) ?? true;

      public string GetFileName(DocItem item)
      {
         // TODO PP (2020-08-28): erro check
         return fileName((T)item);
      }

      public void RenderPage(RenderingContext context, DocItem item, TextWriter writer) => m_renderFunction(context, (T)item, writer); 
   }
}
