using System;
using System.IO;
using System.Linq;
using DefaultDocumentation.Model;
using Newtonsoft.Json.Linq;

namespace MarkDocGen
{
   class SideBarTemplate : ITemplate
   {
      public ITemplate MainTemplate { get; }
      public string DirectoryPrefix { get; }

      public SideBarTemplate(ITemplate mainTemplate, string directoryPrefix)
      {
         MainTemplate = mainTemplate;
         DirectoryPrefix = directoryPrefix;
      }

      public PageInfo GetPageInfo(DocItem item)
      {
         if (item is HomeDocItem)
            return new PageInfo(".json", "Sidebar");
         else
            return PageInfo.NoPage;
      }

      public void RenderPage(RenderingContext context, TextWriter writer)
      {
         var item = (HomeDocItem)context.CurrentItem;

         JObject o = new JObject();         
         foreach (var ns in item.Children.OfType<AssemblyDocItem>().SelectMany(asm => asm.Children.OfType<DocItem>()))
         {
            var nsSidebar = new JArray();
            nsSidebar.Add(GetRef(ns));

            // TODO PP (2020-08-28): Ordering
            foreach (var member in ns.Children.OfType<TypeDocItem>())
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
               //foreach (var 
               //nsSidebar.Add(GetRef(member));
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
         return $"{DirectoryPrefix}/{item.AnchorId}";
      }

      public string RenderCodeBlock(RenderingContext context, string code)
      {
         throw new NotImplementedException();
      }

      public string RenderInlineCode(RenderingContext context, string content)
      {
         throw new NotImplementedException();
      }

      public string RenderLink(RenderingContext context, ILinkModel link)
      {
         throw new NotImplementedException();
      }

     public string RenderParagraph(RenderingContext context, string content)
      {
         throw new NotImplementedException();
      }

      public string RenderParamRef(RenderingContext context, InternalLinkModel link)
      {
         throw new NotImplementedException();
      }

      public string RenderText(RenderingContext context, string text)
      {
         throw new NotImplementedException();
      }

      public string GetDisplayName(DocItem item)
      {
         return item.Id;
      }
   }
}
