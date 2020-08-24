using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;

namespace MarkDocGen
{
   static class RenderingContextExtensions
   {
      public static ILinkModel ResolveCrefLink(this RenderingContext context, string cref, string text = null)
      {
         return context.LinkResolver.ResolveCrefLink(context, cref, text);
      }

      public static ILinkModel ResolveTypeLink(this RenderingContext context, IType type, string text = null)
      {
         return context.LinkResolver.ResolveLink(context, type, text);
      }

      public static InternalLinkModel ResolveLink(this RenderingContext context, DocItem item, string text = null)
      {         
         return context.LinkResolver.ResolveLink(context, item, text);
      }

      public static ILinkModel ResolveLangWordLink(this RenderingContext context, string langword)
      {
         return context.LinkResolver.ResolveLangWordLink(context, langword);
      }
   }

   interface ILinkResolver
   {
      ILinkModel ResolveCrefLink(RenderingContext context, string cref, string text);
      ILinkModel ResolveLangWordLink(RenderingContext context, string langword);
      InternalLinkModel ResolveLink(RenderingContext context, DocItem item, string text);
      ILinkModel ResolveLink(RenderingContext context, IType type, string text);
   }

   class LinkResolver : ILinkResolver
   {
      private IFileNameStrategy m_fileNameStrategy;

      public LinkResolver(IFileNameStrategy fileNameStrategy)
      {
         m_fileNameStrategy = fileNameStrategy;
      }
      public InternalLinkModel ResolveLink(RenderingContext context, DocItem item, string text)
      {
         if (context.Template.GeneratesPage(item))
         {
            var fileName = m_fileNameStrategy.GetFileName(item);
            return new InternalLinkModel(text ?? item.Name, fileName, null);
         }
         else
         {
            var pageItem = FindParentPage(context.Template, item);
            var fileName = "./" + m_fileNameStrategy.GetFileName(pageItem);
            // TODO PP (2020-08-21): Should have an anchor property or sanitized ID!.
            return new InternalLinkModel(text ?? item.Name, fileName, IdStringToUrlAnchor(item.Entity?.GetIdString() ?? "BUG"));
         }
      }

      // TODO PP (2020-08-21): Should return a "multi-link" (i.e. link to type arguments as well), and handle tuples pointers arrays etc.
      public ILinkModel ResolveLink(RenderingContext context, IType type, string text = null)
      {
         ILinkModel HandleGenericType(ParameterizedType genericType)
         {
            ITypeDefinition typeDefinition = genericType.GetDefinition();
            ILinkModel baseLink;
            if (typeDefinition != null && context.Project.Items.TryGetValue(typeDefinition.GetIdString(), out DocItem docItem) && docItem is TypeDocItem typeDocItem)
            {
               baseLink = ResolveLink(context, docItem, typeDocItem.Type.FullName);
            }
            else
            {
               // TODO PP (2020-08-24): Note: This is duplicated in ResolveCrefLink
               baseLink = new ExternalLinkModel(GetDotNetApiUrl(typeDefinition.ReflectionName.Replace("`", "-").ToLower()), String.IsNullOrEmpty(text) ? type.FullName : text);
            }

            var typeArguments = genericType.TypeArguments.Select(arg => ResolveLink(context, arg)).ToArray();

            return new TypeLinkModel(baseLink, typeArguments, null);
         }

         ILinkModel HandleTupleType(TupleType tupleType)
         {
            ITypeDefinition typeDefinition = tupleType.GetDefinition();
            ILinkModel baseLink;
            if (typeDefinition != null && context.Project.Items.TryGetValue(typeDefinition.GetIdString(), out DocItem docItem) && docItem is TypeDocItem typeDocItem)
            {
               baseLink = ResolveLink(context, docItem, typeDocItem.Type.FullName);
            }
            else
            {
               // TODO PP (2020-08-24): Note: This is duplicated in ResolveCrefLink
               baseLink = new ExternalLinkModel(GetDotNetApiUrl(typeDefinition.ReflectionName.Replace("`", "-").ToLower()), String.IsNullOrEmpty(text) ? type.FullName : text);
            }

            // TODO PP (2020-08-24): We may want to handle value tuples here. New LinkModel to render as eg. (string Foo, int Bar) instead of ValueTuple<string, int>
            List<ILinkModel> typeArguments = new List<ILinkModel>();
            for (int i = 0; i < tupleType.ElementTypes.Length; i++)
            {               
               typeArguments.Add(ResolveLink(context, tupleType.ElementTypes[i]));
            }

            return new TypeLinkModel(baseLink, typeArguments, null);
         }

         // TODO PP (2020-08-21): add support for external link definitions perhaps?
         //if (context.Project.Items.TryGetValue(type.GetDefinition().GetIdString(), out var item))
         //{
         //   return context.ResolveLink(item, text);
         //}
         //else
         {
            return type.Kind switch
            {
               TypeKind.Array when type is TypeWithElementType arrayType => new TypeLinkModel(context.ResolveTypeLink(arrayType.ElementType), null, ResolveCrefLink(context, "T:System.Array", "[]")),
               TypeKind.Pointer when type is TypeWithElementType pointerType => new TypeLinkModel(context.ResolveTypeLink(pointerType.ElementType), null, new NoLinkModel("*")),
               TypeKind.ByReference when type is TypeWithElementType elementType => ResolveLink(context, elementType, text),
               TypeKind.TypeParameter => TryGetTypeParameterDocItem(context.CurrentItem, type.Name, out var typeParameter) ? (ILinkModel)ResolveLink(context, typeParameter, type.Name) : new NoLinkModel(type.Name),
               TypeKind.Dynamic => new ExternalLinkModel("https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic", "dynamic"),
               TypeKind.Tuple when type is TupleType tupleType => HandleTupleType(tupleType),
               _ when type is ParameterizedType genericType => HandleGenericType(genericType),
               _ => context.ResolveCrefLink(type.GetDefinition().GetIdString(), text)
            };
            switch (type.Kind)
            {
               case TypeKind.Array:
                  
                  break;
            }
            // External item.
            throw new NotImplementedException();
         }
      }

      public ILinkModel ResolveCrefLink(RenderingContext context, string cref, string text)
      {
         if (context.Project.Items.TryGetValue(cref, out var docItem))
         {
            return ResolveLink(context, docItem, text);
         }
         else
         {
            if (cref.StartsWith("T:"))
            {
               // TODO PP (2020-08-21): Sanity checks
               var type = context.Compilation.FindType(new FullTypeName(cref.Substring(2)));
               if (type == null)
                  throw new Exception();

               return new ExternalLinkModel(GetDotNetApiUrl(type.ReflectionName.Replace("`", "-").ToLower()), String.IsNullOrEmpty(text) ? TypeNameAmbience.ConvertType(type) : text);
            }
            else
            {
               int firstParen = cref.IndexOf('(');
               int typeNameEnd;
               if (firstParen != -1)
               {
                  typeNameEnd = cref.Substring(0, firstParen).LastIndexOf('.');
               }
               else
               {
                  typeNameEnd = cref.LastIndexOf('.');
               }

               var typeName = cref.Substring(2, typeNameEnd - 2);
               var declaringType = context.Compilation.FindType(new FullTypeName(typeName));
               if (declaringType == null)
                  throw new Exception();

               var member = declaringType.GetMembers(m => m.GetIdString() == cref).FirstOrDefault();

               if (member == null)
                  throw new Exception();

               return new ExternalLinkModel(GetDotNetApiUrl(member.ReflectionName.Replace("`", "-").ToLower() + "#" + IdStringToUrlAnchor(member.GetIdString())), String.IsNullOrEmpty(text) ? EntityNameAmbience.ConvertSymbol(member) : text);
            }
         }
      }

      private bool TryGetTypeParameterDocItem(DocItem item, string name, out TypeParameterDocItem typeParameterDocItem)
      {
         typeParameterDocItem = null;
         while (item != null && typeParameterDocItem == null)
         {
            if (item is ITypeParameterizedDocItem typeParameters)
            {
               typeParameterDocItem = Array.Find(typeParameters.TypeParameters, i => i.TypeParameter.Name == name);
            }

            item = item.Parent;
         }

         return typeParameterDocItem != null;
      }

      private string GetDotNetApiUrl(string path)
      {
         return "https://docs.microsoft.com/en-us/dotnet/api/" + path;
      }

      private static string IdStringToUrlAnchor(string idString)
      {
         return Regex.Replace(idString.Substring(2), @"[`,\(\)\{\}\.]", "_");
      }

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
         | ConversionFlags.UseNullableSpecifierForValueTypes
      };

      private DocItem FindParentPage(ITemplate template, DocItem item)
      {
         var current = item;
         while (!template.GeneratesPage(current) && current.Parent != null)
         {
            current = current.Parent;
         }

         return current;
      }

      public ILinkModel ResolveLangWordLink(RenderingContext context, string langword)
      {
         // TODO PP (2020-08-24): warn if non-existing.
         return new ExternalLinkModel($"https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/{langword}", langword);
      }
   }


   public enum LinkType
   {
      Internal,
      External,
      TypeLink,
      NoLink
   }

   interface ILinkModel
   {
      LinkType LinkType { get; }
      string Text { get; }

      ILinkModel WithText(string text);
//      void Render(MyTemplate template, TextWriter writer);
   }

   class ExternalLinkModel : ILinkModel
   {
      public ExternalLinkModel(string url, string text)
      {
         Url = url;
         Text = text;
      }

      public string Url { get; }
      public string Text { get; }

      public virtual LinkType LinkType => LinkType.External;

      public virtual ILinkModel WithText(string text)
      {
         return new ExternalLinkModel(Url, text);
      }
   }

   class InternalLinkModel : ILinkModel
   {
      public InternalLinkModel(string text, string fileName, string anchor)
      {
         Text = text;
         FileName = fileName;
         Anchor = anchor;
      }

      public string Text { get; }
      public string FileName { get; }
      public string Anchor { get; }
      public bool HasAnchor => !String.IsNullOrEmpty(Anchor);

      public LinkType LinkType => LinkType.Internal;

      public ILinkModel WithText(string text)
      {
         return new InternalLinkModel(text, FileName, Anchor);
      }
   }

   class NoLinkModel : ILinkModel
   {
      public NoLinkModel(string text)
      {
         Text = text;
      }

      public LinkType LinkType => LinkType.NoLink;

      public string Text { get; }

      public ILinkModel WithText(string text)
      {
         return new NoLinkModel(text);
      }
   }
      
   class TypeLinkModel : ILinkModel
   {
      public TypeLinkModel(ILinkModel typeLink, IReadOnlyList<ILinkModel> typeArguments, ILinkModel suffix)
      {
         TypeLink = typeLink;
         TypeArguments = typeArguments ?? Array.Empty<ILinkModel>();
         Suffix = suffix;
      }

      public LinkType LinkType => LinkType.TypeLink;

      public string Text => TypeLink.Text;

      public ILinkModel TypeLink { get; }
      public IReadOnlyList<ILinkModel> TypeArguments { get; }
      public ILinkModel Suffix { get; }

      public ILinkModel WithText(string text)
      {
         throw new NotImplementedException();
      }
   }
}
