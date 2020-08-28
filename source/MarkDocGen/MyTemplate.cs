using System.Linq;
using DefaultDocumentation.Model;
using Newtonsoft.Json;

namespace MarkDocGen
{

   //static class PageRenderer
   //{
   //   public static IPageRenderer Create<T>(Action<RenderingContext, T, TextWriter> action, Func<DocItem, string> fs, Predicate) where T : DocItem
   //   {
   //      throw new NotImplementedException();
   //   }
   //}

   static class TemplateExtensions
   {
      public static bool GeneratesPage(this ITemplate template, DocItem item) => template.PageRenderers.Any(renderer => renderer.Supports(item));
   }
}
