using System;
using System.Diagnostics.CodeAnalysis;

namespace MarkDocGen
{
   struct PageInfo : IEquatable<PageInfo>
   {
      public static readonly PageInfo NoPage = new PageInfo();

      public PageInfo(string extension, string fileNameOverride = null)
      {
         GeneratesPage = true;
         Extension = extension == null ? "" : (extension.StartsWith(".") ? extension : "." + extension);
         FileNameOverride = fileNameOverride;
      }

      public bool GeneratesPage { get; }
      public string Extension { get; }
      public string FileNameOverride { get; }

      public bool Equals([AllowNull] PageInfo other)
      {
         throw new NotImplementedException();
      }
   }
}
