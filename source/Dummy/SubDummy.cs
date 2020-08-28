using System;
using System.Collections.Generic;

namespace Dummy.SubDummy.SomeNamespace
{
   /// <summary>
   /// This is my sub enum. Real nice.
   /// </summary>
   /// <remarks>
   /// Some remarks about this enum.
   /// </remarks>
   public enum MySubEnum
   {
      /// <summary>
      /// The numeber one field
      /// </summary>
      One = 1,
      /// <summary>
      /// The numeber twelve field
      /// </summary>
      Twelve = 12,

      /// <summary>
      /// The number fourteen field.
      /// </summary>
      Fourteen = 14
   }

   /// <summary>
   /// SumDummy is a class deriving from a nested class.
   /// </summary>
   /// <remarks>
   /// <para>This is the remarks for the <see cref="SubDummy{T}"/> class.</para>
   /// <para>It contains some paragraphs, with a small <c>code</c> tag.</para>
   /// <para>And finally one last paragraph, with a <see cref="IEnumerable{T}"/> (see) and also a <see langword="false"/>.</para>
   /// </remarks>
   /// <seealso cref="SubDummy{T}.SubDummy(Tuple{int, string})"/>
   class SubDummy<T> : DummyClass.DummyNested<T>
   {
      public SubDummy()
      {
      }

      public SubDummy(Tuple<int, string> tupleProp)
      {
         TupleProp = tupleProp;
      }

      public SubDummy((int Apa, string Bepa) valueTupleProp, Tuple<int, string> tupleProp)
      {
         ValueTupleProp = valueTupleProp;
         TupleProp = tupleProp;
      }
      /// <summary>
      /// This returns a value tuple.
      /// </summary>
      /// <value>The tuple value of Apa and Bepa... a bit weird.</value>
      public (int Apa, string Bepa) ValueTupleProp { get; }

      /// <summary>
      /// This returns a normal tuple.
      /// </summary>
      public Tuple<int, string> TupleProp { get; }


      /// <summary>
      /// This is a method with an example.
      /// </summary>
      /// <example>      
      /// This example illustrates accessing this method.
      /// <code>
      /// SubDummy&lt;int&gt; a = new SumDummy&lt;int&gt;();
      /// a.Method1(3);
      /// </code>
      /// </example>
      /// <remarks>
      /// <para>This is the remarks for the <see cref="SubDummy{T}"/> class.</para>
      /// <para>It contains some paragraphs, with a small <c>code</c> tag.</para>
      /// <para>And finally one last paragraph, with a <see cref="IEnumerable{T}"/> (see) and also a <see langword="false"/>.</para>
      /// </remarks>
      /// <exception cref="ArgumentException">Thrown when an argument is out of whack</exception>
      /// <exception cref="ArgumentNullException">Thrown when an argument is out of whack</exception>
      /// <seealso cref="DummyClass.DummyNested{T}"/>
      /// <seealso cref="StringComparer.GetHashCode(string)"/>
      /// <param name="a">This is the value to pass in to the function. It is called <c>a</c>.</param>
      /// <returns>This method actually always returns <see langword="null"/>.</returns>
      public string Method1(int a)
      {
         return null;
      }

      /// <summary>
      /// some getter stuff
      /// </summary>
      /// <param name="i">the parameter i</param>
      /// <returns>a value of something</returns>
      public string this[int i] => null;

      /// <summary>
      /// some setter stuff.
      /// </summary>
      /// <param name="s">the s parameter</param>
      /// <value>The value of this property.</value>
      public string this[string s] { get => null; set { } }

   }
}
