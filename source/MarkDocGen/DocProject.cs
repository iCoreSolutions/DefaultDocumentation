using DefaultDocumentation.Model;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Scriban;
using Scriban.Runtime;
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

      public DocProject(ILogger logger)
      {
         Log = logger ?? NullLogger.Instance;
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

         AssemblyDocItem assemblyDocItem = new AssemblyDocItem(this, null, _decompiler.TypeSystem.MainModule, ConvertToDocumentation(documentationProvider.GetDocumentation($"T:{_decompiler.TypeSystem.MainModule.AssemblyName}.AssemblyDoc")));
         // TODO PP (2020-08-20): Remove commented code.
         //HomeDocItem homeDocItem = new HomeDocItem(
         //    homePageName,
         //    _decompiler.TypeSystem.MainModule.AssemblyName,
         //    ConvertToDocumentation(documentationProvider.GetDocumentation($"T:{_decompiler.TypeSystem.MainModule.AssemblyName}.AssemblyDoc")));
         //yield return homeDocItem;
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

            foreach (var group in type.Methods.GroupBy(m => m.Name))
            {
               List<MethodBaseDocItem> methods = new List<MethodBaseDocItem>();
               MethodOverloadGroupDocItem groupDocItem = new MethodOverloadGroupDocItem(typeDocItem, group.First().FullName, group.First().Name, XElement.Parse("<doc />"));

               foreach (var entity in group)
               {
                  if (TryGetDocumentation(entity, out documentation))
                  {
                     showType = true;

                     MethodBaseDocItem item = entity switch
                     {
                        IMethod method when method.IsConstructor => new ConstructorDocItem(groupDocItem, method, documentation),
                        IMethod method when method.IsOperator => new OperatorDocItem(groupDocItem, method, documentation),
                        IMethod method => new MethodDocItem(groupDocItem, method, documentation),
                        _ => throw new NotSupportedException()
                     };

                     methods.Add(item);
                  }
               }

               if (methods.Count > 0)
               {
                  _docItems.Add(groupDocItem);
                  foreach (var method in methods)
                     _docItems.Add(method);
               }
            }

            foreach (IEntity entity in Enumerable.Empty<IEntity>().Concat(type.Fields).Concat(type.Properties).Concat(type.Events))
            {
               if (TryGetDocumentation(entity, out documentation))
               {
                  showType = true;

                  DocItem item = entity switch
                  {
                     IField field when typeDocItem is EnumDocItem enumDocItem => new EnumFieldDocItem(enumDocItem, field, documentation),
                     IField field => new FieldDocItem(typeDocItem, field, documentation),
                     IProperty property => new PropertyDocItem(typeDocItem, property, documentation),
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
      }

      internal IEnumerable<DocItem> GetChildren(DocItem docItem)
      {
         return _docItems.Where(item => item.Parent == docItem);
      }
   }
}
