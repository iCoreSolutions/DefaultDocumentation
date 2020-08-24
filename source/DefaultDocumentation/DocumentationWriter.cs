﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DefaultDocumentation.Helper;
using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;

namespace DefaultDocumentation
{
    internal sealed class DocumentationWriter : IDisposable
    {
        private static readonly ConcurrentQueue<StringBuilder> _builders = new ConcurrentQueue<StringBuilder>();

        private readonly StringBuilder _builder;
        private readonly FileNameMode _fileNameMode;
        private readonly IReadOnlyDictionary<string, DocItem> _items;
        private readonly IReadOnlyDictionary<string, string> _links;
        private readonly DocItem _mainItem;
        private readonly string _filePath;

        public IEnumerable<DocItem> KnownItems => _items.Values;

        public NestedTypeVisibility NestedTypeVisibility { get; }

        public DocumentationWriter(
            FileNameMode fileNameMode,
            NestedTypeVisibility nestedTypeVisibility,
            IReadOnlyDictionary<string, DocItem> items,
            IReadOnlyDictionary<string, string> links,
            string folderPath,
            DocItem item)
        {
            if (!_builders.TryDequeue(out _builder))
            {
                _builder = new StringBuilder(1024);
            }

            _fileNameMode = fileNameMode;
            NestedTypeVisibility = nestedTypeVisibility;
            _items = items;
            _links = links;
            _mainItem = item;
            _filePath = Path.Combine(folderPath, $"{item.GetLink(_fileNameMode)}.md");
        }

        private bool WriteChildrenLink<T>(DocItem parent, string title, bool includeInnerChildren)
        {
            bool hasTitle = title is null;
            foreach (DocItem child in KnownItems.Where(i => i.Parent == parent).OrderBy(i => i.Id))
            {
                if (child is T)
                {
                    if (!hasTitle)
                    {
                        hasTitle = true;
                        WriteLine($"### {title}");
                    }

                    WriteLine($"- {GetLink(child)}");
                }

                if (includeInnerChildren)
                {
                    hasTitle = WriteChildrenLink<T>(child, hasTitle ? null : title, true);
                }
            }

            return hasTitle;
        }

        public string GetLink(DocItem item, string displayedName = null) =>
            item.GeneratePage ? $"[{displayedName ?? item.Name}](./{item.GetLink(_fileNameMode)}.md '{item.FullName}')" : GetInnerLink(item, displayedName);

        public string GetInnerLink(DocItem item, string displayedName = null)
        {
            DocItem pagedDocItem = item.GetPagedDocItem();

            return $"[{displayedName ?? item.Name}]({(_mainItem == pagedDocItem ? string.Empty : $"./{pagedDocItem.GetLink(_fileNameMode)}.md")}#{item.GetLink(_fileNameMode)} '{item.FullName}')";
        }

        public string GetTypeLink(IType type)
        {
            string HandleParameterizedType(ParameterizedType genericType)
            {
                ITypeDefinition typeDefinition = genericType.GetDefinition();
                if (typeDefinition != null && _items.TryGetValue(typeDefinition.GetIdString(), out DocItem docItem) && docItem is TypeDocItem typeDocItem)
                {
                    return GetLink(docItem, typeDocItem.Type.FullName + "&lt;")
                        + string.Join(GetLink(docItem, ","), genericType.TypeArguments.Select(GetTypeLink))
                        + GetLink(docItem, "&gt;");
                }

                return genericType.GenericType.ReflectionName.AsDotNetApiLink(genericType.FullName + "&lt;")
                    + string.Join(genericType.GenericType.ReflectionName.AsDotNetApiLink(","), genericType.TypeArguments.Select(GetTypeLink))
                    + genericType.GenericType.ReflectionName.AsDotNetApiLink("&gt;");
            }

            string HandleTupleType(TupleType tupleType)
            {
                return tupleType.FullName.AsDotNetApiLink(tupleType.FullName + "&lt;")
                    + string.Join(tupleType.FullName.AsDotNetApiLink(","), tupleType.ElementTypes.Select(GetTypeLink))
                    + tupleType.FullName.AsDotNetApiLink("&gt;");
            }

            return type.Kind switch
            {
                TypeKind.Array when type is TypeWithElementType arrayType => GetTypeLink(arrayType.ElementType) + "System.Array".AsDotNetApiLink("[]"),
                TypeKind.Pointer when type is TypeWithElementType pointerType => GetTypeLink(pointerType.ElementType) + "*",
                TypeKind.ByReference when type is TypeWithElementType innerType => GetTypeLink(innerType.ElementType),
                TypeKind.TypeParameter => _mainItem.TryGetTypeParameterDocItem(type.Name, out TypeParameterDocItem typeParameter) ? GetInnerLink(typeParameter) : type.Name,
                TypeKind.Dynamic => "[dynamic](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic 'dynamic')",
                TypeKind.Tuple when type is TupleType tupleType => HandleTupleType(tupleType),
                TypeKind.Unknown => type.FullName.AsDotNetApiLink(),
                _ when type is ParameterizedType genericType => HandleParameterizedType(genericType),
                _ => GetLink(type.GetDefinition().GetIdString())
            };
        }

        public string GetLink(string id) =>
            _items.TryGetValue(id, out DocItem item) ? GetLink(item) : (_links.TryGetValue(id, out string link) ? link.AsLink(id.Substring(2)) : id.Substring(2).AsDotNetApiLink());

        public void WriteLine(string line) => _builder.AppendLine(line);

        public void Write(string line) => _builder.Append(line);

        public void Break() => _builder.AppendLine();

        public void WriteHeader()
        {
            HomeDocItem home = KnownItems.OfType<HomeDocItem>().Single();
            if (home.GeneratePage)
            {
                WriteLine($"#### {GetLink(home)}");
            }

            Stack<DocItem> parents = new Stack<DocItem>();
            for (DocItem parent = _mainItem?.Parent; parent != home && parent != null; parent = parent.Parent)
            {
                parents.Push(parent);
            }

            if (parents.Count > 0)
            {
                WriteLine($"### {string.Join(".", parents.Select(p => GetLink(p)))}");
            }
        }

        public void WritePageTitle(string name, string title) => WriteLine($"## {name} {title}");

        public void WriteChildrenLink<T>(string title) where T : DocItem => WriteChildrenLink<T>(_mainItem, title, true);

        public void WriteDirectChildrenLink<T>(string title) where T : DocItem => WriteChildrenLink<T>(_mainItem, title, false);

        public void WriteDocItems(IEnumerable<DocItem> items, string title)
        {
            bool hasTitle = false;
            foreach (DocItem item in items)
            {
                if (!hasTitle)
                {
                    hasTitle = true;
                    WriteLine(title);
                }

                item.WriteDocumentation(this);
                WriteLine("  ");
            }
        }

        public void WriteDocItems<T>(string title)
            where T : DocItem
            => WriteDocItems(KnownItems.OfType<T>().Where(i => i.Parent == _mainItem), title);

        public void WriteLinkTarget(DocItem item) => WriteLine($"<a name='{item.GetLink(_fileNameMode)}'></a>");

        public void Write(DocItem item, XElement element) => Write(null, element, item);

        public void Write(string title, XElement element, DocItem item)
        {
            string GetSeeLink(XElement element)
            {
                string see = element.GetReferenceName();
                if (see is null)
                {
                    see = element.GetLangWord();
                    if (see is null)
                    {
                        return string.Empty;
                    }

                    return $"https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/{see}";
                }

                return GetLink(see);
            }

            string WriteNodes(IEnumerable<XNode> nodes)
            {
                return string.Concat(nodes.Select(node => node switch
                {
                    XText text => string.Join("  \n", text.Value.Split('\n')),
                    XElement element => element.Name.ToString() switch
                    {
                        "see" => GetSeeLink(element),
                        "seealso" => GetLink(element.GetReferenceName()),
                        "typeparamref" => item.TryGetTypeParameterDocItem(element.GetName(), out TypeParameterDocItem typeParameter) ? GetInnerLink(typeParameter) : element.GetName(),
                        "paramref" => item.TryGetParameterDocItem(element.GetName(), out ParameterDocItem parameter) ? GetInnerLink(parameter) : element.GetName(),
                        "c" => $"`{element.Value}`",
                        "code" => $"```csharp\n{element.Value}\n```\n",
                        "para" => $"\n\n{WriteNodes(element.Nodes())}\n\n",
                        "list" => "",
                        _ => element.ToString()
                    },
                    _ => throw new Exception($"unhandled node type in summary {node.NodeType}")
                }));
            }

            if (element is null)
            {
                return;
            }

            if (title != null)
            {
                WriteLine(title);
            }

            string summary = WriteNodes(element.Nodes());

            string[] lines = summary.Split('\n');
            int startIndex = 0;
            int firstLine = 0;

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    startIndex = line.Length - line.TrimStart().Length;
                    break;
                }

                ++firstLine;
            }

            summary = string.Join(Environment.NewLine, lines.Skip(firstLine).Select(l => l.StartsWith(" ") ? l.Substring(Math.Min(l.Length, startIndex)) : l));
            while (summary.EndsWith(Environment.NewLine))
            {
                summary = summary.Substring(0, summary.Length - Environment.NewLine.Length);
            }

            WriteLine(summary.TrimEnd() + "  ");
        }

        public void WriteExceptions(DocItem item)
        {
            bool hasTitle = false;
            foreach (XElement exception in item.Documentation.GetExceptions())
            {
                if (!hasTitle)
                {
                    hasTitle = true;
                    WriteLine("#### Exceptions");
                }

                string typeName = exception.GetReferenceName();

                Write(
                    _items.TryGetValue(typeName, out DocItem type)
                    ? GetLink(type)
                    : typeName.Substring(2).AsDotNetApiLink());
                WriteLine("  ");
                Write(item, exception);
            }
        }

        public void Dispose()
        {
            File.WriteAllText(_filePath, _builder.ToString());

            _builder.Clear();
            _builders.Enqueue(_builder);
        }
    }
}
