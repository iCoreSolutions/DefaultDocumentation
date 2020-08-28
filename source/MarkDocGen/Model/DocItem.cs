using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using MarkDocGen;

namespace DefaultDocumentation.Model
{

   internal abstract class DocItem
   {
      public IEnumerable<DocItem> Children => Project.GetChildren(this);
      
      public abstract DocItemKind Kind { get; }

      public DocItem Parent { get; }
      public string Id { get; }
      public XElement Documentation { get; }
      public DocProject Project { get; }
      public virtual string AnchorId => Id == null ? null : Regex.Replace(Id, @"[`,\(\)\{\}\.\:<>\*\#]", "_");
      
      public enum DisplayNameFormat
      {
         CSharpCodeDeclaration,
         Name,
         FullName,
      }

      protected DocItem(DocProject project, DocItem parent, string id, XElement documentation)
      {
         Project = project;
         Parent = parent;
         Id = id;
         if (id.Length > 1 && id[1] == ':')
            
         Documentation = documentation;
      }
   }
}
