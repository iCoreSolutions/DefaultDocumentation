using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal sealed class EnumFieldDocItem : FieldDocItem
   {
      public EnumFieldDocItem(EnumDocItem parent, IField field, XElement documentation)
          : base(parent, field, documentation)
      {
      }

      public override DocItemKind Kind => DocItemKind.EnumField;

      public object FieldValue => Field.GetConstantValue();
   }
}
