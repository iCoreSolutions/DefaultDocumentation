using System;
using System.Collections.Generic;

namespace Dummy.SubDummy.SomeNamespace
{
   /// <summary>
   /// This is a delegate
   /// </summary>
   /// <param name="a">parameter a</param>
   /// <param name="b">parameter b</param>
   public delegate void MyDelegate1(int a, int b);

   /// <summary>
   /// Factory for SubDummy
   /// </summary>
   /// <typeparam name="T">A type argument</typeparam>
   /// <param name="name">Some name</param>
   /// <returns>A new subdummy!?</returns>
   public delegate SubDummy<T> SubDummyFactory<T>(string name);

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
   /// <para>And the type parameter <typeparamref name="T"/> is also here.</para>
   /// </remarks>
   /// <seealso cref="SubDummy{T}.SubDummy(Tuple{int, string})"/>
   public class SubDummy<T> : DummyClass.DummyNested<T>
   {
      /// <summary>
      /// This is a delegate
      /// </summary>
      /// <param name="a">parameter a</param>
      /// <param name="b">parameter b</param>
      /// <remarks>
      /// <para>This is the remarks, with a bullet list:
      /// <list type="bullet">
      /// <item>
      ///   <description>This is item 1</description>
      /// </item>
      /// <item>
      ///   <description>This is item 2</description>
      /// </item>
      /// <item>
      ///   <description>This is item 3</description>
      /// </item>
      /// </list>
      /// </para>
      /// <para>
      /// And here we go witha  <c>Number</c> list:
      /// <list type="number">
      /// <item>
      ///   <description>This is item 1</description>
      /// </item>
      /// <item>
      ///   <description>This is item 2</description>
      /// </item>
      /// <item>
      ///   <description>This is item 3</description>
      /// </item>
      /// </list>
      /// </para>
      /// 
      /// <para>
      /// Here's a table:
      /// /// <list type="table">
      /// <listheader>
      /// <term>Action</term>
      /// <term>Description</term>
      /// <term>Power Consumption</term>
      /// </listheader>
      /// <item>
      /// <term>Forward</term>
      /// <term>Move forwards in a straight line.</term>
      /// <term>50W</term>
      /// </item>
      /// <item>
      /// <term>Backward</term>
      /// <term>Move backwards in a straight line.</term>
      /// <term>50W</term>
      /// </item>
      /// <item>
      /// <term>RotateLeft</term>
      /// <term>Rotate to the left.</term>
      /// <term>30W</term>
      /// </item>
      /// <item>
      /// <term>RotateRight</term>
      /// <term>Rotate to the right.</term>
      /// <term>30W</term>
      /// </item>
      /// <item>
      /// <term>Dig</term>
      /// <term>Tells the robot to dig and obtain a soil sample.</term>
      /// <term>800W</term>
      /// </item>
      /// </list>
      /// </para>      
      /// 
      /// <para>
      /// A definition list:
      /// <list type="number">
      /// <item>
      /// <term>Forward</term>
      /// <description>Move forwards in a straight line.</description>
      /// </item>
      /// <item>
      /// <term>Backward</term>
      /// <description>Move backwards in a straight line.</description>
      /// </item>
      /// <item>
      /// <term>RotateLeft</term>
      /// <description>Rotate to the left.</description>
      /// </item>
      /// <item>
      /// <term>RotateRight</term>
      /// <description>Rotate to the right.</description>
      /// </item>
      /// <item>
      /// <term>Dig</term>
      /// <description>Tells the robot to dig and obtain a soil sample.</description>
      /// </item>
      /// </list>
      /// </para>
      /// </remarks>
      public delegate void MyDelegate1(int a, int b);

      
      /// <summary>
      /// The default constructor.
      /// </summary>
      /// <remarks>
      /// We can have a type parameter <typeparamref name="T"/>.
      /// </remarks>
      public SubDummy()
      {
      }

      /// <summary>
      /// This is the normal tuple ctor.
      /// </summary>
      /// <param name="tupleProp"></param>
      public SubDummy(Tuple<int, string> tupleProp)
      {
         TupleProp = tupleProp;
      }

      /// <summary>
      /// This is a strange ctor.
      /// </summary>
      /// <param name="valueTupleProp">The value</param>
      /// <param name="tupleProp">Something</param>
      /// <remarks>
      /// We can have a type parameter <typeparamref name="T"/> and also a parameter <paramref name="tupleProp"/>.
      /// </remarks>
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
      /// <code>SubDummy&lt;int&gt; a = new SumDummy&lt;int&gt;();
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
