using System.Collections.Generic;
using System.Linq;

namespace DefaultDocumentation.Model
{
   static class DocItemExtensions
   {
      // TODO PP (2020-08-25): Change sorting to include parameters maybe?
      public static IEnumerable<MethodDocItem> AllMethods(this TypeDocItem item) => item.Children.OfType<MethodOverloadGroupDocItem>().SelectMany(group => group.Methods.OfType<MethodDocItem>()).Concat(item.Children.OfType<MethodDocItem>()).OrderBy(method => method.Name).ThenBy(method => method.Parameters.Length);
      public static IEnumerable<OperatorDocItem> AllOperators(this TypeDocItem item) => item.Children.OfType<OperatorOverloadGroupDocItem>().SelectMany(group => group.Operators).Concat(item.Children.OfType<OperatorDocItem>()).OrderBy(method => method.Name).ThenBy(method => method.Parameters.Length);

      public static IEnumerable<ConstructorOverloadGroupDocItem> ConstructorOverloads(this TypeDocItem item) => item.Children.OfType<ConstructorOverloadGroupDocItem>();
      public static IEnumerable<ConstructorDocItem> AllConstructors(this TypeDocItem item) => item.ConstructorOverloads().SelectMany(group => group.Constructors).Concat(item.NonOverloadedConstructors());
      public static IEnumerable<ConstructorDocItem> NonOverloadedConstructors(this TypeDocItem item) => item.Children.OfType<ConstructorDocItem>();

      public static IEnumerable<PropertyOverloadGroupDocItem> PropertyOverloads(this TypeDocItem item) => item.Children.OfType<PropertyOverloadGroupDocItem>();
      public static IEnumerable<PropertyDocItem> AllProperties(this TypeDocItem item) => item.PropertyOverloads().SelectMany(group => group.Properties).Concat(item.NonOverloadedProperties());
      public static IEnumerable<PropertyDocItem> NonOverloadedProperties(this TypeDocItem item) => item.Children.OfType<PropertyDocItem>();
   }
}
