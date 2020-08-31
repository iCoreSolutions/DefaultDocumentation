using DefaultDocumentation.Model;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MarkDocGen
{
   // TODO PP (2020-08-25): Fix anchors
   class DocProject
   {
      private class DocItemCollection : KeyedCollection<string, DocItem>
      {
         protected override string GetKeyForItem(DocItem item)
         {
            return item.Id;
         }
      }

      private readonly DocItemCollection _docItems = new DocItemCollection();
      private readonly HomeDocItem _homeItem;
      public DocProject(ILogger logger)
      {
         Log = logger ?? NullLogger.Instance;
         _homeItem = new HomeDocItem(this, "Home", "Home", null);
         _docItems.Add(_homeItem);
      }

      public ILogger Log { get; }

      public IEnumerable<AssemblyDocItem> Assemblies => _docItems.OfType<AssemblyDocItem>();

      public KeyedCollection<string, DocItem> Items => _docItems;

      public void Add(string assemblyFilePath, string documentationFilePath)
      {
         Log.LogInformation("Loading assembly {FilePath}", assemblyFilePath);
         var decompiler = new CSharpDecompiler(assemblyFilePath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
         var documentationProvider = new XmlDocumentationProvider(documentationFilePath);
         CollectDocItems(decompiler, documentationProvider);
      }

      private void CollectDocItems(CSharpDecompiler _decompiler, XmlDocumentationProvider documentationProvider)
      {
         static XElement ConvertToDocumentation(string documentationString) => documentationString is null ? null : XElement.Parse($"<doc>{documentationString}</doc>");

         bool TryGetDocumentation(IEntity entity, out XElement documentation)
         {
            documentation = ConvertToDocumentation(documentationProvider.GetDocumentation(entity));

            return documentation != null;
         }

         AssemblyDocItem assemblyDocItem = new AssemblyDocItem(this, _homeItem, _decompiler.TypeSystem.MainModule, ConvertToDocumentation(documentationProvider.GetDocumentation($"T:{_decompiler.TypeSystem.MainModule.AssemblyName}.AssemblyDoc")));
         _docItems.Add(assemblyDocItem);

         foreach (ITypeDefinition type in _decompiler.TypeSystem.MainModule.TypeDefinitions.Where(t => t.Name != "NamespaceDoc" && t.Name != "AssemblyDoc"))
         {
            bool showType = TryGetDocumentation(type, out XElement documentation);
            bool newNamespace = false;

            string namespaceId = $"N:{type.Namespace}";
            if (!_docItems.TryGetValue(type.DeclaringType?.GetDefinition().GetIdString() ?? namespaceId, out DocItem parentDocItem))
            {
               newNamespace = true;

               parentDocItem = new NamespaceDocItem(
                   assemblyDocItem,
                   type.Namespace,
                   ConvertToDocumentation(documentationProvider.GetDocumentation(namespaceId) ?? documentationProvider.GetDocumentation($"T:{type.Namespace}.NamespaceDoc")));
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

            foreach (var group in type.Methods.Cast<IEntity>().Concat(type.Properties).Where(m => m.Accessibility == Accessibility.Public).GroupBy(m => m.Name))
            {
               if (group.Count() == 1)
               {
                  var entity = group.First();
                  // TODO PP (2020-08-25): warn if doc missing for visible member, and add filter
                  TryGetDocumentation(entity, out var doc);
                  var methodItem = CreateItem(doc, typeDocItem, entity);
                  showType = true;
                  _docItems.Add(methodItem);
               }
               else
               {
                  OverloadGroupDocItem groupDocItem;

                  // TODO PP (2020-08-25): Add parsing of overload documentation.

                  IEntity firstItem = group.First();
                  if (firstItem is IMethod method)
                  {
                     if (method.IsConstructor)
                        groupDocItem = new ConstructorOverloadGroupDocItem(typeDocItem, method.FullName, group.First().Name, XElement.Parse("<doc />"));
                     else if (method.IsOperator)
                        groupDocItem = new OperatorOverloadGroupDocItem(typeDocItem, method.FullName, group.First().Name, XElement.Parse("<doc />"));
                     else
                        groupDocItem = new MethodOverloadGroupDocItem(typeDocItem, method.FullName, group.First().Name, XElement.Parse("<doc />"));
                  }
                  else if (firstItem is IProperty property)
                  {
                     groupDocItem = new PropertyOverloadGroupDocItem(typeDocItem, property.FullName, firstItem.Name, XElement.Parse("<doc />"));
                  }
                  else
                  {
                     throw new InvalidOperationException($"Internal error; Unsupported overloaded member of type {firstItem.GetType().Name}");
                  }

                  List <DocItem> members = new List<DocItem>();
                  foreach (var entity in group)
                  {
                     // TODO PP (2020-08-25): warn if doc missing for visible member, and add filter
                     TryGetDocumentation(entity, out documentation);
                     showType = true;
                     var item = CreateItem(documentation, groupDocItem, entity);

                     members.Add(item);
                  }

                  _docItems.Add(groupDocItem);                  
                  foreach (var member in members)
                     _docItems.Add(member);
               }
            }

            foreach (IEntity entity in Enumerable.Empty<IEntity>().Concat(type.Fields).Concat(type.Events).Where(m => m.Accessibility == Accessibility.Public))
            {
               if (TryGetDocumentation(entity, out documentation))
               {
                  showType = true;

                  DocItem item = entity switch
                  {
                     IField field when typeDocItem is EnumDocItem enumDocItem => new EnumFieldDocItem(enumDocItem, field, documentation),
                     IField field => new FieldDocItem(typeDocItem, field, documentation),
                     IEvent @event => new EventDocItem(typeDocItem, @event, documentation),
                     _ => throw new NotSupportedException()
                  };

                  _docItems.Add(item);
               }
            }

            if (showType)
            {
               if (newNamespace)
               {
                  _docItems.Add(parentDocItem);
               }

               _docItems.Add(typeDocItem);
            }
         }

         static DocItem CreateItem(XElement documentation, DocItem parent, IEntity entity)
         {
            return entity switch
            {
               IMethod method when method.IsConstructor => new ConstructorDocItem(parent, method, documentation),
               IMethod method when method.IsOperator => new OperatorDocItem(parent, method, documentation),
               IMethod method => new MethodDocItem(parent, method, documentation),
               IProperty property => new PropertyDocItem(parent, property, documentation),
               _ => throw new NotSupportedException()
            };
         }
      }

      internal IEnumerable<DocItem> GetChildren(DocItem docItem)
      {
         return _docItems.Where(item => item.Parent == docItem);
      }
   }
}
