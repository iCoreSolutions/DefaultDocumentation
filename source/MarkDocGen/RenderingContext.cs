using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace MarkDocGen
{
   class RenderingContext
   {
      public RenderingContext(DocGen generator, ITemplate template, DocItem currentItem, IFileNameStrategy fileNameStrategy, ILinkResolver linkResolver, ILogger log)
      {
         Generator = generator;
         Template = template;
         CurrentItem = currentItem;
         FileNameStrategy = fileNameStrategy;
         LinkResolver = linkResolver;
         Log = log ?? NullLogger.Instance;
      }

      public ILogger Log { get; }
      public IFileNameStrategy FileNameStrategy { get; }
      public ILinkResolver LinkResolver { get; }
      public DocGen Generator { get; }
      public DocProject Project => CurrentItem?.Project;

      public ICompilation Compilation
      {
         get
         {
            var current = CurrentItem;

            while (!(current is EntityDocItem || current is AssemblyDocItem) && current.Parent != null)
               current = current.Parent;

            return (current as EntityDocItem)?.Entity.Compilation ?? ((current as AssemblyDocItem)?.Module)?.Compilation;
         }
      }
      
      public ITemplate Template { get; }

      // TODO PP (2020-08-28): Don't linke CurrentItem here... But we need this to resovle type parameters it seems... Maybe enough with current page? (no)
      public DocItem CurrentItem { get; }
      
      public RenderingContext WithItem(DocItem item)
      {
         return new RenderingContext(Generator, Template, item, FileNameStrategy, LinkResolver, Log);
      }
   }

   static class RenderingContextExtensions
   {
      public static ILinkModel ResolveCrefLink(this RenderingContext context, string cref, string text = null)
      {
         return context.LinkResolver.ResolveCrefLink(context, cref, text);
      }

      public static ILinkModel ResolveTypeLink(this RenderingContext context, IType type, string text = null)
      {
         return context.LinkResolver.ResolveLink(context, type, text);
      }

      public static ILinkModel ResolveLink(this RenderingContext context, DocItem item, string text = null)
      {
         return context.LinkResolver.ResolveLink(context, item, text);
      }

      public static ILinkModel ResolveLangWordLink(this RenderingContext context, string langword)
      {
         return context.LinkResolver.ResolveLangWordLink(context, langword);
      }
   }
}
