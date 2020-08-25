using System;

namespace MarkDocGen
{
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
}
