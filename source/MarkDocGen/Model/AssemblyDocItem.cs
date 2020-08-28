using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{
   /*
    * Type Name: 
    * Declaration
    * Without generic arguments(?)
    * With generic arguments
    * Declaration
    * 
    * With/Without namespace
    * 
    * Member Name:
    * + With or without parameter list
    * 
    */
   internal class AssemblyDocItem : DocItem
   {
      public AssemblyDocItem(DocProject project, DocItem parent, IModule module, XElement documentation) 
         : base(project, parent, "A:" + module.AssemblyName, documentation)
      {
         Module = module;
      }

      public override DocItemKind Kind => DocItemKind.Assembly;

      public IModule Module { get; }

      // TODO PP (2020-08-20): Remove commented code.
      //public override void WriteDocumentation(DocumentationWriter writer)
      //{
      //   throw new NotImplementedException();
      //}
   }
}
