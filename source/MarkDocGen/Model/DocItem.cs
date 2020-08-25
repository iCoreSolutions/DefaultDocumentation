using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{

   internal abstract class DocItem
   {
      // TODO PP (2020-08-25): Remove commented code.
      //private static readonly CSharpAmbience FullNameAmbience = new CSharpAmbience
      //{
      //   ConversionFlags =
      //        ConversionFlags.ShowParameterList
      //        | ConversionFlags.ShowTypeParameterList
      //        | ConversionFlags.UseFullyQualifiedTypeNames
      //        | ConversionFlags.ShowDeclaringType
      //        | ConversionFlags.UseFullyQualifiedEntityNames
      //};

      //private static readonly CSharpAmbience NameAmbience = new CSharpAmbience
      //{
      //   ConversionFlags =
      //        ConversionFlags.ShowParameterList
      //        | ConversionFlags.ShowTypeParameterList
      //};

      //private static readonly CSharpAmbience TypeNameAmbience = new CSharpAmbience
      //{
      //   ConversionFlags =
      //        ConversionFlags.ShowParameterList
      //        | ConversionFlags.ShowTypeParameterList
      //        | ConversionFlags.ShowDeclaringType
      //        | ConversionFlags.UseFullyQualifiedTypeNames
      //};

      //private static readonly CSharpAmbience EntityNameAmbience = new CSharpAmbience
      //{
      //   ConversionFlags =
      //        ConversionFlags.ShowParameterList
      //        | ConversionFlags.ShowTypeParameterList
      //        | ConversionFlags.UseFullyQualifiedTypeNames
      //};

      public IEnumerable<DocItem> Children => Project.GetChildren(this);
      
      public abstract DocItemKind Kind { get; }

      public DocItem Parent { get; }
      public string Id { get; }
      public XElement Documentation { get; }
      public DocProject Project { get; }
      public virtual string AnchorId => Id == null ? null : Regex.Replace(Id, @"[`,\(\)\{\}\.\:]", "_");
      
      public enum DisplayNameFormat
      {
         CSharpCodeDeclaration,
         Name,
         FullName,
      }

      protected DocItem(DocProject project, DocItem parent, string id, XElement documentation)
      {
         // TODO PP (2020-08-20): assert parameters.
         Project = project;
         Parent = parent;
         Id = id;
         if (id.Length > 1 && id[1] == ':')
            
         Documentation = documentation;

         // TODO PP (2020-08-20): Don't like these replaces
         //FullName = fullName.Replace("<", "&lt;").Replace(">", "&gt;").Replace("this ", string.Empty);
         //Name = name.Replace("<", "&lt;").Replace(">", "&gt;").Replace("this ", string.Empty);
      }

      // TODO PP (2020-08-25): Remove commented code.
      //private static string GetName(IEntity entity, IAmbience ambience)
      //{
      //   string fullName = ambience.ConvertSymbol(entity);

      //   if (entity.SymbolKind == SymbolKind.Operator)
      //   {
      //      int offset = 17;
      //      int index = fullName.IndexOf("implicit operator ");
      //      if (index < 0)
      //      {
      //         index = fullName.IndexOf("explicit operator ");

      //         if (index < 0)
      //         {
      //            index = fullName.IndexOf("operator ");
      //            offset = fullName.IndexOf('(') - index;
      //         }
      //      }

      //      if (index >= 0)
      //      {
      //         fullName = fullName.Substring(0, index) + entity.Name + fullName.Substring(index + offset);
      //      }
      //   }

      //   return fullName;
      //}

      // TODO PP (2020-08-20): Remove commented code.
      //public abstract void WriteDocumentation(DocumentationWriter writer);

      //public virtual string GetLink(FileNameMode fileNameMode) => (fileNameMode switch
      //{
      //    FileNameMode.Md5 => Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(FullName))),
      //    FileNameMode.Name => _entity is null ? FullName : string.Join(".", GetHierarchy().Reverse()),
      //    _ => FullName
      //}).Clean();

   }

   internal abstract class SymbolDocItem : DocItem
   {
      protected SymbolDocItem(DocItem parent, ISymbol symbol, string id, XElement documentation)
      : base(parent.Project, parent, id, documentation)
      {
         Symbol = symbol;
      }

      public ISymbol Symbol { get; }

   }
}
