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
using Microsoft.Extensions.Logging;
using Serilog;

namespace MarkDocGen
{
   class DefaultLinkResolver : ILinkResolver
   {
      private IPageRenderer GetLinkTargetPageRenderer(RenderingContext context, DocItem item)
      {
         var supportingRenderers = context.Template.PageRenderers.Where(renderer => renderer.Supports(item) && renderer.IsLinkTarget(item));
         if (supportingRenderers.Any())
         {
            if (supportingRenderers.Skip(1).Any())
               throw new InvalidOperationException($"Multiple renderers reported item of type {item.Kind} as a page link target.");
            return supportingRenderers.First();
         }
         return null;
      }

      public ILinkModel ResolveLink(RenderingContext context, DocItem item, string text)
      {
         var renderer = GetLinkTargetPageRenderer(context, item);
         if (renderer != null)
         {
            var fileName = renderer.GetFileName(item);
            return new InternalLinkModel(text ?? context.Template.GetDisplayName(item), fileName, null);
         }
         else
         {
            var pageItem = FindParentPage(context, item);
            renderer = GetLinkTargetPageRenderer(context, pageItem);
            if (renderer == null)
            {               
               return new NoLinkModel(text ?? context.Template.GetDisplayName(item));
            }

            var fileName = "./" + renderer.GetFileName(pageItem);
            return new InternalLinkModel(text ?? context.Template.GetDisplayName(item), fileName, item.AnchorId);
         }
      }

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
               // TODO PP (2020-08-21): Sanity checks perhaps and... NoLinkModel does not equal an unresolved link, does it?
               var type = context.Compilation.FindType(new FullTypeName(cref.Substring(2)));
               if (type == null)
                  return new NoLinkModel($"Unresolved link {cref}");

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
               if (typeParameterDocItem != null)
                  return true;
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

      private DocItem FindParentPage(RenderingContext context, DocItem item)
      {
         var current = item;
         while (GetLinkTargetPageRenderer(context, current) == null && current.Parent != null)
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
}
