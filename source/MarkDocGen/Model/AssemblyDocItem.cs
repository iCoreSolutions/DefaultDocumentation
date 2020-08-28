using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   internal class AssemblyDocItem : DocItem
   {
      public AssemblyDocItem(DocProject project, DocItem parent, IModule module, XElement documentation) 
         : base(project, parent, "A:" + module.AssemblyName, documentation)
      {
         Module = module;
      }

      public override DocItemKind Kind => DocItemKind.Assembly;

      public IModule Module { get; }
   }
}
