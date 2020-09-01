using System;
using System.IO;
using System.Linq;
using DefaultDocumentation.Model;
using Newtonsoft.Json.Linq;

namespace MarkDocGen
{
   class DocusaurusSideBarTemplate : TemplateBase
   {
      public ITemplate MainTemplate { get; }
      public string DirectoryPrefix { get; }

      public DocusaurusSideBarTemplate(ITemplate mainTemplate, string directoryPrefix)
      {
         MainTemplate = mainTemplate;
         DirectoryPrefix = directoryPrefix;
         AddRenderer(new TextPageRenderer<HomeDocItem>(RenderPage, _ => "Sidebar.json", isLinkTarget: _ => false));
      }

      public void RenderPage(RenderingContext context, DocItem item, TextWriter writer)
      {         
         JObject o = new JObject();         
         foreach (var ns in item.Children.OfType<AssemblyDocItem>().SelectMany(asm => asm.Children.OfType<DocItem>()))
         {
            var nsSidebar = new JArray();
            nsSidebar.Add(GetRef(ns));
                        
            foreach (var member in ns.Children.OfType<TypeDocItem>().OrderBy(t => t.Name))
            {
               if (member.Kind != DocItemKind.Enum && member.Kind != DocItemKind.Delegate)
               {
                  var typeCategory = AddCategory(nsSidebar, MainTemplate.GetDisplayName(member));
                  typeCategory.Add(GetRef(member));

                  if (member.AllConstructors().Any())
                  {
                     typeCategory.Add(GetRef(member.ConstructorOverloads().Any() ? (DocItem)member.ConstructorOverloads().First() : member.AllConstructors().Single()));
                  }

                  if (member.Fields.Any())
                  {
                     var propertyCategory = AddCategory(typeCategory, "Fields");
                     foreach (var field in member.Fields.OrderBy(p => MainTemplate.GetDisplayName(p)))
                        propertyCategory.Add(GetRef(field));
                  }

                  if (member.Delegates.Any())
                  {
                     var propertyCategory = AddCategory(typeCategory, "Delegates");
                     foreach (var field in member.Delegates.OrderBy(p => MainTemplate.GetDisplayName(p)))
                        propertyCategory.Add(GetRef(field));
                  }

                  if (member.Events.Any())
                  {
                     var propertyCategory = AddCategory(typeCategory, "Events");
                     foreach (var field in member.Events.OrderBy(p => MainTemplate.GetDisplayName(p)))
                        propertyCategory.Add(GetRef(field));
                  }

                  if (member.AllProperties().Any())
                  {
                     var propertyCategory = AddCategory(typeCategory, "Properties");
                     foreach (var property in member.NonOverloadedProperties().Cast<DocItem>().Concat(member.PropertyOverloads()).OrderBy(p => MainTemplate.GetDisplayName(p)))
                        propertyCategory.Add(GetRef(property));
                  }

                  if (member.AllMethods().Any())
                  {
                     var methodCategory = AddCategory(typeCategory, "Methods");
                     foreach (var method in member.NonOverloadedMethods().Cast<DocItem>().Concat(member.MethodOverloads()).OrderBy(p => MainTemplate.GetDisplayName(p)))
                        methodCategory.Add(GetRef(method));
                  }

                  if (member.AllOperators().Any())
                  {
                     var methodCategory = AddCategory(typeCategory, "Operators");
                     foreach (var method in member.NonOverloadedOperators().Cast<DocItem>().Concat(member.OperatorOverloads()).OrderBy(p => MainTemplate.GetDisplayName(p)))
                        methodCategory.Add(GetRef(method));
                  }
               }
               else
               {
                  nsSidebar.Add(GetRef(member));

               }
            }
            o.Add(new JProperty(ns.AnchorId, nsSidebar));
         }
         writer.WriteLine(o.ToString());
      }

      private JContainer AddCategory(JContainer json, string label)
      {
         JArray items = new JArray();
         json.Add(new JObject(
            new JProperty("type", "category"),
            new JProperty("label", label),
            new JProperty("items", items)
         ));

         return items;
      }

      private string GetRef(DocItem item)
      {
         if (DirectoryPrefix != null)
            return $"{DirectoryPrefix}/{item.AnchorId}";
         else
            return item.AnchorId;
      }

      public override string GetDisplayName(DocItem item)
      {
         return item.Id;
      }

   }
}
