using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.TypeSystem;

namespace MarkDocGen
{
   interface ILinkResolver
   {
      ILinkModel ResolveCrefLink(RenderingContext context, string cref, string text);
      ILinkModel ResolveLangWordLink(RenderingContext context, string langword);
      InternalLinkModel ResolveLink(RenderingContext context, DocItem item, string text);
      ILinkModel ResolveLink(RenderingContext context, IType type, string text);
   }
}
