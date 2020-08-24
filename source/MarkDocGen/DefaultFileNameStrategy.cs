using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   interface IFileNameStrategy
   {
      string GetFileName(DocItem item);

   }
   class DefaultFileNameStrategy : IFileNameStrategy
   {
      public static readonly DefaultFileNameStrategy Instance = new DefaultFileNameStrategy();
      private static readonly IReadOnlyDictionary<string, string> _invalidStrings =
       new Dictionary<string, string>(Path.GetInvalidFileNameChars().ToDictionary(c => $"{c}", _ => "-"))
       {
          ["="] = string.Empty,
          [" "] = string.Empty,
          [","] = "_",
          ["."] = "-",
          ["["] = "-",
          ["]"] = "-",
          ["&lt;"] = "-",
          ["&gt;"] = "-",
       };

      public string GetFileName(DocItem item)
      {
         return Clean(item.Id) + Extension;
         //string baseName = item.Entity is null ? item.FullName : string.Join(".", GetHierarchy(item).Reverse());
         //return Clean(baseName + Extension);
      }

      public string Extension => ".md";

      private IEnumerable<string> GetHierarchy(DocItem item)
      {
         // TODO PP (2020-08-20): verify this... GetName does some stuff with operators
         yield return item.SimpleName;// GetName(_entity, NameAmbience);

         DocItem parent = item.Parent;
         while (parent is TypeDocItem)
         {
            yield return parent.SimpleName; // GetName(parent._entity, NameAmbience);

            parent = parent.Parent;
         }
      }

      private static string Clean(string value)
      {
         foreach (KeyValuePair<string, string> pair in _invalidStrings)
         {
            value = value.Replace(pair.Key, pair.Value);
         }

         return value;
      }
   }
}
