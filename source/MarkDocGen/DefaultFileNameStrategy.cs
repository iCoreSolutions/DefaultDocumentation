using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultDocumentation.Model;

namespace MarkDocGen
{
   interface IFileNameStrategy
   {
      string GetFileName(DocItem item, string extension);

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
          ["("] = "_",
          [")"] = "_",
          ["<"] = "_",
          [">"] = "_",
          ["#"] = "_",
          ["`"] = "_",
          ["+"] = "_",
       };

      public string GetFileName(DocItem item, string extension)
      {
         // TODO PP (2020-08-31): This is actually pretty specific to the Docusaurus template.. We should probably just move this into the template and remove this I think... maybe... hmm... 
         string id = item.Id;
         if (item is MethodBaseDocItem mdi)
            return Clean("M:" + mdi.Method.ReflectionName) + extension;         

         return Clean(item.Id) + extension;
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
