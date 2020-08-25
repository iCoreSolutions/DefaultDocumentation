using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;

namespace DefaultDocumentation.Model
{
   internal abstract class MethodBaseDocItem : EntityDocItem, IParameterizedDocItem
   {
      public MethodBaseDocItem(DocItem parent, IMethod method, XElement documentation)
         : base(parent, method, documentation)
      {
         Method = method;
         Parameters = method.Parameters.Select(p => new ParameterDocItem(this, p, documentation)).ToArray();
      }

      public string Name => Method.Name;

      public IMethod Method { get; }
      public ParameterDocItem[] Parameters { get; }
   }
}
