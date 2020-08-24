﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace DefaultDocumentation.Helper
{
   internal static class XElementExtension
   {
      public static XElement GetSummary(this XElement element) => element?.Element("summary");

      public static IEnumerable<XElement> GetTypeParameters(this XElement element) => element?.Elements("typeparam");

      public static IEnumerable<XElement> GetParameters(this XElement element) => element?.Elements("param");

      public static IEnumerable<XElement> GetExceptions(this XElement element) => element?.Elements("exception");

      public static XElement GetReturns(this XElement element) => element?.Element("returns");

      public static XElement GetRemarks(this XElement element) => element?.Element("remarks");

      public static XElement GetExample(this XElement element) => element?.Element("example");

      public static XElement GetValue(this XElement element) => element?.Element("value");

      public static string GetName(this XElement element) => element.Attribute("name")?.Value;

      public static string GetReferenceName(this XElement element) => element.Attribute("cref")?.Value;

      public static IEnumerable<XElement> GetSeeAlsos(this XElement element) => element?.Elements("seealso");

        public static string GetLangWord(this XElement element) => element.Attribute("langword")?.Value;
   }
}
