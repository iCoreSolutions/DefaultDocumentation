using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using MarkDocGen;

namespace DefaultDocumentation.Model
{

   /*
    * Introducing an AssemblyDocItem might be a bit problematic.  GetIdString() in documentation does *not* contain any information about the assembly in which the item is found I think. This means we cannot resolve the correct
    * type by this Id, *if* a type with the same FullName exists in two assemblies.  However... this is likely never going to happen, because it would just be stupid to be honest. So for now, we ignore assembly name in ID generation, and just 
    * keep the generated ID.
    * 
    * */
   /*
    * Several parts to this:
    * 1. Determine which files to generate
    * 2. Render the content of a page or a DocItem, this will need to reference parts of the XmlDocumentation.
    * 3. Render the content of the XmlDocumentation elements, such as <c>, <see cref=""> etc.
    * 
    * 
    * 
    * 
    * 
    */
   internal abstract class DocItem
   {
      private static readonly CSharpAmbience FullNameAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowParameterList
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.UseFullyQualifiedTypeNames
              | ConversionFlags.ShowDeclaringType
              | ConversionFlags.UseFullyQualifiedEntityNames
      };

      private static readonly CSharpAmbience NameAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowParameterList
              | ConversionFlags.ShowTypeParameterList
      };

      private static readonly CSharpAmbience TypeNameAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowParameterList
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.ShowDeclaringType
              | ConversionFlags.UseFullyQualifiedTypeNames
      };

      private static readonly CSharpAmbience EntityNameAmbience = new CSharpAmbience
      {
         ConversionFlags =
              ConversionFlags.ShowParameterList
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.UseFullyQualifiedTypeNames
      };

      private readonly IEntity _entity;

      public IEnumerable<DocItem> Children => Project.GetChildren(this);
      public abstract DocItemKind Kind { get; }

      public DocItem Parent { get; }
      public string Id { get; }
      public XElement Documentation { get; }
      public string FullName { get; }
      public string Name { get; }
      public DocProject Project { get; }

      public string SimpleName => Entity == null ? null :  GetName(Entity, NameAmbience);

      protected DocItem(DocProject project, DocItem parent, string id, string fullName, string name, XElement documentation)
      {
         // TODO PP (2020-08-20): assert parameters.
         Project = project;
         Parent = parent;
         Id = id;
         Documentation = documentation;

         // TODO PP (2020-08-20): Don't like these replaces
         FullName = fullName.Replace("<", "&lt;").Replace(">", "&gt;").Replace("this ", string.Empty);
         Name = name.Replace("<", "&lt;").Replace(">", "&gt;").Replace("this ", string.Empty);
      }

      protected DocItem(DocItem parent, IEntity entity, XElement documentation)
          : this(parent.Project, parent, entity.GetIdString(), GetName(entity, FullNameAmbience), (entity is ITypeDefinition ? TypeNameAmbience : EntityNameAmbience).ConvertSymbol(entity), documentation)
      {
         _entity = entity;
      }

      private static string GetName(IEntity entity, IAmbience ambience)
      {
         string fullName = ambience.ConvertSymbol(entity);

         if (entity.SymbolKind == SymbolKind.Operator)
         {
            int offset = 17;
            int index = fullName.IndexOf("implicit operator ");
            if (index < 0)
            {
               index = fullName.IndexOf("explicit operator ");

               if (index < 0)
               {
                  index = fullName.IndexOf("operator ");
                  offset = fullName.IndexOf('(') - index;
               }
            }

            if (index >= 0)
            {
               fullName = fullName.Substring(0, index) + entity.Name + fullName.Substring(index + offset);
            }
         }

         return fullName;
      }

      // TODO PP (2020-08-20): Remove commented code.
      //public abstract void WriteDocumentation(DocumentationWriter writer);

      //public virtual string GetLink(FileNameMode fileNameMode) => (fileNameMode switch
      //{
      //    FileNameMode.Md5 => Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(FullName))),
      //    FileNameMode.Name => _entity is null ? FullName : string.Join(".", GetHierarchy().Reverse()),
      //    _ => FullName
      //}).Clean();

      internal IEntity Entity => _entity;
   }
}
