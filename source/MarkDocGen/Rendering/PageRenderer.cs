using System;
using System.IO;
using DefaultDocumentation.Model;

namespace MarkDocGen
{

   abstract class PageRenderer<T> : IPageRenderer where T : DocItem
   {
      private readonly Func<T, string> m_fileName;
      private readonly Predicate<T> m_supports;
      private readonly Predicate<T> m_isLinkTarget;

      public PageRenderer(Func<T, string> fileName, Predicate<T> supports = null, Predicate<T> isLinkTarget = null)
      {
         m_isLinkTarget = isLinkTarget;
         m_fileName = fileName;
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
         return m_fileName((T)item);
      }

      void IPageRenderer.RenderPage(RenderingContext context, DocItem item, TextWriter writer) => RenderPage(context, (T)item, writer);

      public abstract void RenderPage(RenderingContext context, T item, TextWriter writer);
   }
}
