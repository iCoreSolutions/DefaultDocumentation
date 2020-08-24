using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Xml.Linq;
using DefaultDocumentation.Model;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;

namespace DefaultDocumentation
{
   // Idea 1:
   // Move out GeneratesPage to some formatter.
   // Move the actual writing of a page out to same formatter or other class.
   // Link generation in some service?

   class DocWriter
   {
      Template m_template;

      public void Method(DocumentationGenerator generator)
      {
         var root = generator.DocItems.OfType<HomeDocItem>().Single();

         Stack<DocItem> stack = new Stack<DocItem>();
         stack.Push(root);

         while (stack.Count > 0)
         {
            var current = stack.Pop();

            if (m_template.GeneratesFile(current))
            {

               // Create a file.
            }

            foreach (var child in generator.GetChildren(current))
            {
               stack.Push(child);
            }
         }
      }
   }

   internal class HeaderTemplate
   {
      public void Write(DocItem item, TextWriter writer)
      {
         writer.WriteLine($"# {item.Name}");
      }
   }

   internal class FileTemplate
   {
      public bool Supports(DocItem item)
      {
         return item is NamespaceDocItem;
      }

      public void Write(DocItem item, TextWriter writer)
      {
         new HeaderTemplate().Write(item, writer);
      }
   }

   internal class FileNameStrategy
   {
      public string GetFileName(DocItem item)
      {
         return null;
      }
   }

   internal class Template
   {
      public bool GeneratesFile(DocItem item)
      {
         return item is TypeDocItem;
      }

      public void Write(NamespaceDocItem item)
      {

      }
   }

   internal sealed class DocumentationGenerator
   {
      private readonly CSharpDecompiler _decompiler;
      private readonly XmlDocumentationProvider _documentationProvider;
      private readonly FileNameMode _fileNameMode;
      private readonly NestedTypeVisibility _nestedTypeVisibility;
      private readonly Dictionary<string, DocItem> _docItems;
      private readonly Dictionary<string, string> _links;

      public DocumentationGenerator(
          string assemblyFilePath,
          string documentationFilePath,
          string homePageName,
          FileNameMode fileNameMode,
          NestedTypeVisibility nestedTypeVisibility,
          string linksFiles)
      {
         _decompiler = new CSharpDecompiler(assemblyFilePath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
         _documentationProvider = new XmlDocumentationProvider(documentationFilePath);
         _fileNameMode = fileNameMode;
         _nestedTypeVisibility = nestedTypeVisibility;

         _docItems = new Dictionary<string, DocItem>();
         foreach (DocItem item in GetDocItems(homePageName))
         {
            _docItems.Add(item.Id, item);
         }

         _links = new Dictionary<string, string>();
         foreach ((string id, string link) in GetExternalLinks(linksFiles))
         {
            _links[id] = link;
         }
      }

      public IEnumerable<DocItem> DocItems => _docItems.Values;

      public IEnumerable<DocItem> GetChildren(DocItem parent)
      {
         return _docItems.Values.Where(i => i.Parent == parent);
      }

      public IEnumerable<T> GetChildren<T>(DocItem parent) where T : DocItem
      {
         return _docItems.Values.Where(i => i.Parent == parent).OfType<T>();
      }

      private IEnumerable<DocItem> GetDocItems(string homePageName)
      {
         static XElement ConvertToDocumentation(string documentationString) => documentationString is null ? null : XElement.Parse($"<doc>{documentationString}</doc>");

         bool TryGetDocumentation(IEntity entity, out XElement documentation)
         {
            documentation = ConvertToDocumentation(_documentationProvider.GetDocumentation(entity));

            return documentation != null;
         }

         HomeDocItem homeDocItem = new HomeDocItem(
             homePageName,
             _decompiler.TypeSystem.MainModule.AssemblyName,
             ConvertToDocumentation(_documentationProvider.GetDocumentation($"T:{_decompiler.TypeSystem.MainModule.AssemblyName}.AssemblyDoc")));
         yield return homeDocItem;

         foreach (ITypeDefinition type in _decompiler.TypeSystem.MainModule.TypeDefinitions.Where(t => t.Name != "NamespaceDoc" && t.Name != "AssemblyDoc"))
         {
            bool showType = TryGetDocumentation(type, out XElement documentation);
            bool newNamespace = false;

            string namespaceId = $"N:{type.Namespace}";
            if (!_docItems.TryGetValue(type.DeclaringType?.GetDefinition().GetIdString() ?? namespaceId, out DocItem parentDocItem))
            {
               newNamespace = true;

               parentDocItem = new NamespaceDocItem(
                   homeDocItem,
                   type.Namespace,
                   ConvertToDocumentation(_documentationProvider.GetDocumentation(namespaceId) ?? _documentationProvider.GetDocumentation($"T:{type.Namespace}.NamespaceDoc")));
            }

            TypeDocItem typeDocItem = type.Kind switch
            {
               TypeKind.Class => new ClassDocItem(parentDocItem, type, documentation),
               TypeKind.Struct => new StructDocItem(parentDocItem, type, documentation),
               TypeKind.Interface => new InterfaceDocItem(parentDocItem, type, documentation),
               TypeKind.Enum => new EnumDocItem(parentDocItem, type, documentation),
               TypeKind.Delegate => new DelegateDocItem(parentDocItem, type, documentation),
               _ => throw new NotSupportedException()
            };

      CSharpAmbience EntityNameAmbience = new CSharpAmbience
                  {
                     ConversionFlags =
              ConversionFlags.ShowParameterList
              | ConversionFlags.ShowTypeParameterList
              | ConversionFlags.UseFullyQualifiedTypeNames
                  };

            
      var t = _decompiler.TypeSystem.FindType(new FullTypeName("System.Linq.Enumerable"));
      var mem = t.GetMembers().Where(m => m.Name == "LastOrDefault").Last();



            foreach (IEntity entity in Enumerable.Empty<IEntity>().Concat(type.Fields).Concat(type.Properties).Concat(type.Methods).Concat(type.Events))
            {
               var yt = entity.Compilation.FindType(new FullTypeName("System.Linq.Enumerable"));
               if (TryGetDocumentation(entity, out documentation))
               {
                  showType = true;

                  yield return entity switch
                  {
                     IField field when typeDocItem is EnumDocItem enumDocItem => new EnumFieldDocItem(enumDocItem, field, documentation),
                     IField field => new FieldDocItem(typeDocItem, field, documentation),
                     IProperty property => new PropertyDocItem(typeDocItem, property, documentation),
                     IMethod method when method.IsConstructor => new ConstructorDocItem(typeDocItem, method, documentation),
                     IMethod method when method.IsOperator => new OperatorDocItem(typeDocItem, method, documentation),
                     IMethod method => new MethodDocItem(typeDocItem, method, documentation),
                     IEvent @event => new EventDocItem(typeDocItem, @event, documentation),
                     _ => throw new NotSupportedException()
                  };
}
            }

            if (showType)
{
   if (newNamespace)
   {
      yield return parentDocItem;
   }

   yield return typeDocItem;
}
         }

         homeDocItem.HasMultipleNamespaces = _docItems.Values.OfType<NamespaceDocItem>().Count() > 1;
      }

      private IEnumerable<(string, string)> GetExternalLinks(string linksFiles)
{
   foreach (string linksFile in (linksFiles ?? string.Empty).Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)))
   {
      using StreamReader reader = File.OpenText(linksFile);

      string baseLink = string.Empty;
      while (!reader.EndOfStream)
      {
         string[] items = reader.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

         switch (items.Length)
         {
            case 0:
               baseLink = string.Empty;
               break;

            case 1:
               baseLink = items[0];
               if (!baseLink.EndsWith("/"))
               {
                  baseLink += "/";
               }
               break;

            case 2:
               yield return (items[0], baseLink + items[1]);
               break;
         }
      }
   }
}

public void WriteDocumentation(string outputFolderPath)
{
   _docItems.Values.Where(i => i.GeneratePage).AsParallel().ForAll(i =>
   {
      try
      {
         using DocumentationWriter writer = new DocumentationWriter(_fileNameMode, _nestedTypeVisibility, _docItems, _links, outputFolderPath, i);

         i.WriteDocumentation(writer);
      }
      catch (Exception exception)
      {
         throw new Exception($"Error while writing documentation for {i.FullName}", exception);
      }
   });
}

public void WriteLinks(string baseLinkPath, string linksFilePath)
{
   using StreamWriter writer = File.CreateText(linksFilePath);

   if (!string.IsNullOrEmpty(baseLinkPath))
   {
      writer.WriteLine(baseLinkPath);
   }

   foreach (DocItem item in _docItems.Values)
   {
      switch (item)
      {
         case HomeDocItem _:
            break;

         case EnumFieldDocItem _:
            writer.WriteLine($"{item.Id} {item.Parent.GetLink(_fileNameMode)}.md#{item.GetLink(_fileNameMode)}");
            break;

         default:
            writer.WriteLine($"{item.Id} {item.GetLink(_fileNameMode)}.md");
            break;
      }
   }
}
   }
}
