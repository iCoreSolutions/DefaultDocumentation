using ICSharpCode.Decompiler.TypeSystem;

namespace DefaultDocumentation.Model
{
   internal interface IReturnTypeDocItem : IDocItem
   {
      IType ReturnType { get; }
   }
}
