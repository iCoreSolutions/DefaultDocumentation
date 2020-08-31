using System;
using System.Diagnostics;

namespace DefaultDocumentation
{
   [DebuggerDisplay("{Alignment} {Width} IsWhiteSpace = {IsWhiteSpace}")]
   public readonly struct TableColumnInfo : IEquatable<TableColumnInfo>
   {
      public TableColumnInfo(HorizontalAlignment alignment, int width, bool isWhiteSpace)
      {
         Alignment = alignment;
         Width = width;
         IsWhiteSpace = isWhiteSpace;
      }

      internal static TableColumnInfo Default { get; } = new TableColumnInfo(HorizontalAlignment.Left, 0, isWhiteSpace: true);

      public HorizontalAlignment Alignment { get; }

      public bool IsWhiteSpace { get; }

      public int Width { get; }

      internal TableColumnInfo UpdateWidthIfGreater(int newWidth)
      {
         if (newWidth > Width)
         {
            return new TableColumnInfo(Alignment, newWidth, IsWhiteSpace);
         }
         else
         {
            return this;
         }
      }

      public TableColumnInfo WithAlignment(HorizontalAlignment alignment)
      {
         return new TableColumnInfo(
             alignment: alignment,
             width: Width,
             isWhiteSpace: IsWhiteSpace);
      }

      public TableColumnInfo WithWidth(int width)
      {
         return new TableColumnInfo(
             alignment: Alignment,
             width: width,
             isWhiteSpace: IsWhiteSpace);
      }

      public TableColumnInfo WithIsWhiteSpace(bool isWhiteSpace)
      {
         return new TableColumnInfo(
             alignment: Alignment,
             width: Width,
             isWhiteSpace: isWhiteSpace);
      }

      public override bool Equals(object obj)
      {
         return (obj is TableColumnInfo other)
             && Equals(other);
      }

      public bool Equals(TableColumnInfo other)
      {
         return Alignment == other.Alignment
                && IsWhiteSpace == other.IsWhiteSpace
                && Width == other.Width;
      }

      public override int GetHashCode()
      {
         return HashCode.Combine((int)Alignment, IsWhiteSpace, Width);
      }

      public static bool operator ==(in TableColumnInfo info1, in TableColumnInfo info2)
      {
         return info1.Equals(info2);
      }

      public static bool operator !=(in TableColumnInfo info1, in TableColumnInfo info2)
      {
         return !(info1 == info2);
      }
   }
}
