using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    class MyClass
    {
        #region Public methods

        public string StringMethod()
        {
            return "abc";
        }

        public string StringMethod(string arg)
        {
            return "_" + arg;
        }

        public object Test_Method_2000(object val)
        {
            return 200;
        }

        public double Increment(double i)
        {
            return i + 50;
        }

        public void VoidMethod()
        {
        }

        #endregion

        #region Public fields

        public int myVar = 40;

        public int MyProp { get { return 50; } }

        #endregion

        #region Privates

        private int PrivateProp { get { return 50; } }

        private int PrivateVar = 40;

        private object PrivateMethod()
        {
            return null;
        }

        #endregion

        #region Static

        public static int MyStaticVar = 68;

        public static int MyStaticProperty { get { return 69; } }

        public static string MyStaticMethod(string arg)
        {
            return arg + arg;
        }

        #endregion
    }
}
