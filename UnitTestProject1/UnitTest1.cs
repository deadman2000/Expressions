using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Expressions;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCall()
        {
            MyClass mc = new MyClass();

            Assert.AreEqual(40, Expression.GetProperty(mc, "myVar")); // Variable
            Assert.AreEqual(50, Expression.GetProperty(mc, "MyProp")); // Property

            Assert.AreEqual("abc", Expression.Call(mc, "StringMethod")); // Method
            Assert.AreEqual(null, Expression.Call(mc, "VoidMethod")); // Void method
            Assert.AreEqual("_xyz", Expression.Call(mc, "StringMethod", "xyz")); // Method with argument
            Assert.AreEqual(110d, Expression.Call(mc, "Increment", 60)); // Method with argument

            Assert.AreEqual(69, Expression.GetProperty(typeof(MyClass), "MyStaticProperty")); // Static propery
            Assert.AreEqual("abcabc", Expression.Call(typeof(MyClass), "MyStaticMethod", "abc")); // Static method

            // Privates test
            try
            {
                Expression.GetProperty(mc, "PrivateProp"); // Private property
                Assert.Fail();
            }
            catch { }

            try
            {
                Expression.GetProperty(mc, "PrivateVar"); // Private property
                Assert.Fail();
            }
            catch { }

            try
            {
                Expression.Call(mc, "PrivateMethod"); // Private method
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void TestExpressions()
        {
            Dictionary<string, object> scope = new Dictionary<string, object>();
            scope.Add("my", new MyClass());
            scope.Add("MyClass", typeof(MyClass));

            Dictionary<string, object> scope2 = new Dictionary<string, object>();
            scope.Add("test", new MyClass());

            Assert.AreEqual(8, Expression.Eval("'abcdefgh'.Length", scope)); // String property call
            Assert.AreEqual("abc", Expression.Eval("my.StringMethod()", scope)); // Method execution
            Assert.AreEqual("def'", Expression.Eval(@"'def\''", scope)); //  String escaping '
            Assert.AreEqual("def\"", Expression.Eval("\"def\\\"\"", scope)); //  String escaping "
            Assert.AreEqual("_def'", Expression.Eval(@"my.StringMethod('def\'')", scope)); //  Method with string argument
            Assert.AreEqual(4, Expression.Eval("my.StringMethod('def').Length", scope)); // Propery of method result
            Assert.AreEqual(200, Expression.Eval("my.Test_Method_2000(null)", scope)); // Method with _ and digits + null keyword
            Assert.AreEqual(100.5, Expression.Eval("test.Increment (50.5)", scope)); // Double argument + spacing

            Assert.AreEqual(69, Expression.Eval("MyClass.MyStaticProperty", scope)); // Static property
            Assert.AreEqual("abcabc", Expression.Eval("MyClass.MyStaticMethod('abc')", scope)); // Static method call
        }

        [TestMethod]
        public void TestExpressionExceptions()
        {
            Dictionary<string, object> scope = new Dictionary<string, object>();
            scope.Add("my", new MyClass());

            try
            {
                Expression.Eval("asd", scope); // Not exists variable test
                Assert.Fail();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                Expression.Eval("my.StringMethod())", scope); // Invalid symbol
                Assert.Fail();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                Expression.Eval("my.StringMethod(", scope); // No closing bracer
                Assert.Fail();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}