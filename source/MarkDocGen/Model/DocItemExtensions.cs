using System.Collections.Generic;
using System.Linq;

namespace DefaultDocumentation.Model
{
   static class DocItemExtensions
   {
      public static IEnumerable<ConstructorOverloadGroupDocItem> ConstructorOverloads(this TypeDocItem item) => item.Children.OfType<ConstructorOverloadGroupDocItem>();
      public static IEnumerable<ConstructorDocItem> AllConstructors(this TypeDocItem item) => item.ConstructorOverloads().SelectMany(group => group.Constructors).Concat(item.NonOverloadedConstructors());
      public static IEnumerable<ConstructorDocItem> NonOverloadedConstructors(this TypeDocItem item) => item.Children.OfType<ConstructorDocItem>();

      public static IEnumerable<PropertyOverloadGroupDocItem> PropertyOverloads(this TypeDocItem item) => item.Children.OfType<PropertyOverloadGroupDocItem>();
      public static IEnumerable<PropertyDocItem> AllProperties(this TypeDocItem item) => item.PropertyOverloads().SelectMany(group => group.Properties).Concat(item.NonOverloadedProperties());
      public static IEnumerable<PropertyDocItem> NonOverloadedProperties(this TypeDocItem item) => item.Children.OfType<PropertyDocItem>();

      public static IEnumerable<MethodOverloadGroupDocItem> MethodOverloads(this TypeDocItem item) => item.Children.OfType<MethodOverloadGroupDocItem>();
      public static IEnumerable<MethodDocItem> AllMethods(this TypeDocItem item) => item.Children.OfType<MethodOverloadGroupDocItem>().SelectMany(group => group.Methods.OfType<MethodDocItem>()).Concat(item.Children.OfType<MethodDocItem>()).OrderBy(method => method.Name).ThenBy(method => method.Parameters.Length);
      public static IEnumerable<MethodDocItem> NonOverloadedMethods(this TypeDocItem item) => item.Children.OfType<MethodDocItem>();

      public static IEnumerable<OperatorOverloadGroupDocItem> OperatorOverloads(this TypeDocItem item) => item.Children.OfType<OperatorOverloadGroupDocItem>();
      public static IEnumerable<OperatorDocItem> AllOperators(this TypeDocItem item) => item.Children.OfType<OperatorOverloadGroupDocItem>().SelectMany(group => group.Operators).Concat(item.Children.OfType<OperatorDocItem>()).OrderBy(method => method.Name).ThenBy(method => method.Parameters.Length);
      public static IEnumerable<OperatorDocItem> NonOverloadedOperators(this TypeDocItem item) => item.Children.OfType<OperatorDocItem>();

      public static IEnumerable<ClassDocItem> Classes(this NamespaceDocItem item) => item.Children.OfType<ClassDocItem>();
      public static IEnumerable<StructDocItem> Structs(this NamespaceDocItem item) => item.Children.OfType<StructDocItem>();
      public static IEnumerable<InterfaceDocItem> Interfaces(this NamespaceDocItem item) => item.Children.OfType<InterfaceDocItem>();
      public static IEnumerable<EnumDocItem> Enums(this NamespaceDocItem item) => item.Children.OfType<EnumDocItem>();
      public static IEnumerable<DelegateDocItem> Delegates(this NamespaceDocItem item) => item.Children.OfType<DelegateDocItem>();

      public static IEnumerable<DocItem> ParentChain(this DocItem item)
      {
         if (item == null)
            yield break;

         var current = item.Parent;
         while (current != null)
         {
            yield return current;
            current = current.Parent;
         }
      }

      public static NamespaceDocItem ContainingNamespace(this DocItem item) => item.ParentChain().OfType<NamespaceDocItem>().FirstOrDefault();
      public static AssemblyDocItem ContainingAssembly(this DocItem item) => item.ParentChain().OfType<AssemblyDocItem>().FirstOrDefault();
      public static IEnumerable<NamespaceDocItem> AllNamespaces(this HomeDocItem item) => item.Children.OfType<AssemblyDocItem>().SelectMany(assembly => assembly.Namespaces());
      public static IEnumerable<NamespaceDocItem> Namespaces(this AssemblyDocItem item) => item.Children.OfType<NamespaceDocItem>();
   }
}
