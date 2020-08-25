using System;
using System.Collections.Generic;

namespace MarkDocGen
{
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
