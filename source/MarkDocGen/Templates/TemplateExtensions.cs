using System.Linq;
using DefaultDocumentation.Model;
using Newtonsoft.Json;

namespace MarkDocGen
{
   static class TemplateExtensions
   {
      public static bool GeneratesPage(this ITemplate template, DocItem item) => template.PageRenderers.Any(renderer => renderer.Supports(item));
   }
}
