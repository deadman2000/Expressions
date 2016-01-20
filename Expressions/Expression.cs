using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Expressions
{
    public class Expression
    {
        private string expr;
        private Dictionary<string, object>[] _scopes;

        private int _offset = 0;
        private int partStart = 0;

        private static bool DEBUG = true;

        public Expression(string expression, params Dictionary<string, object>[] scopes)
        {
            expr = expression.Trim();
            _scopes = scopes;
        }

        private bool IsAvailable
        {
            get { return _offset < expr.Length; }
        }

        public object Process()
        {
            if (DEBUG) Console.WriteLine();
            if (DEBUG) Console.WriteLine("Process expression: '{0}'", expr);

            object val = null;
            while (IsAvailable)
            {
                val = ProcessPart(val);
            }

            if (DEBUG) Console.WriteLine("Expression result: " + val);
            if (DEBUG) Console.WriteLine();
            return val;
        }

        public object ProcessPart(object val)
        {
            if (DEBUG) Console.WriteLine("Process part");

            if (val != null)
            {
                if (expr[_offset] != '.')
                    throw new FormatException("Invalid symbol: " + expr[_offset]);
                _offset++;
            }

            var c = expr[_offset];
            if (DEBUG) Console.WriteLine(_offset + ": " + c);

            if (c == '\'' || c == '"') // String expression
            {
                return ReadString();
            }

            if (Char.IsDigit(c))
            {
                return ReadDigit();
            }

            if (Char.IsLetter(c)) // Variable
            {
                string word = ReadWord();
                if (DEBUG) Console.WriteLine("Word: " + word);

                if (Search('(')) // Method calling
                {
                    if (DEBUG) Console.WriteLine("Method: " + val + "." + word);

                    string argStr = ReadBracers().Trim();
                    if (argStr.Length > 0)
                    {
                        string[] argParts = argStr.Split(',');
                        object[] args = new object[argParts.Length];
                        for (int i = 0; i < argParts.Length; i++)
                        {
                            object arg = new Expression(argParts[i], _scopes).Process();
                            args[i] = arg;
                        }

                        if (DEBUG) Console.WriteLine("Call: " + word + "(" + String.Join(", ", args) + ")");
                        return Call(val, word, args);
                    }
                    else
                    {
                        if (DEBUG) Console.WriteLine("Call: " + word + "()");
                        return Call(val, word);
                    }
                }
                else // Variable or property
                {
                    if (val == null) // TODO Check on first word, because it can be property or method result
                    {
                        if (DEBUG) Console.WriteLine("Variable: " + word);
                        return GetVariable(word);
                    }
                    else
                    {
                        if (DEBUG) Console.WriteLine("Property: " + word + " of " + val);
                        return GetProperty(val, word);
                    }
                }
            }

            if (c == '(')
            {
                string argStr = ReadBracers();
                object arg = new Expression(argStr, _scopes).Process();
                return arg;
            }

            throw new FormatException(String.Format("Wrong symbol '{0}' at {1}", c, _offset));
        }

        private string ReadString()
        {
            char q = expr[_offset];

            _offset++;
            int strStart = _offset;
            for (; _offset < expr.Length; _offset++) // Searching closing quote
            {
                if (expr[_offset] == q)
                {
                    if (expr[_offset - 1] == '\\')
                    {
                        if (DEBUG) Console.WriteLine("Escaping");
                        continue; // Escaping
                    }
                    break;
                }
            }

            string str = expr.Substring(strStart, _offset - strStart).Replace("\\" + q, q.ToString());
            _offset++;
            return str;
        }

        private double ReadDigit()
        {
            int wordStart = _offset;
            while (_offset < expr.Length && (Char.IsDigit(expr[_offset]) || expr[_offset] == '.')) _offset++;
            string str = expr.Substring(wordStart, _offset - wordStart);
            return double.Parse(str, CultureInfo.InvariantCulture);
        }

        private string ReadWord()
        {
            int wordStart = _offset;
            while (_offset < expr.Length && (Char.IsLetterOrDigit(expr[_offset]) || expr[_offset] == '_')) _offset++;
            return expr.Substring(wordStart, _offset - wordStart);
        }

        private bool Search(char p) // Check next non whitespace symbol and if equals move offset to him
        {
            if (!IsAvailable)
                return false;

            for (int i = _offset; i < expr.Length; i++)
            {
                char c = expr[i];
                if (p == c)
                {
                    _offset = i;
                    return true;
                }

                if (!Char.IsWhiteSpace(c))
                    return false;
            }
            return false;
        }

        private string ReadBracers()
        {
            int start = _offset + 1;

            int lvl = 0;
            for (; _offset < expr.Length; _offset++) // Searching closing bracer
            {
                switch (expr[_offset])
                {
                    case '\'':
                    case '"':
                        ReadString();
                        _offset--;
                        break;
                    case '(':
                        lvl++;
                        break;

                    case ')':
                        lvl--;
                        if (lvl == 0)
                        {
                            string value = expr.Substring(start, _offset - start);
                            _offset++;
                            return value;
                        }
                        break;
                }
            }

            throw new Exception("Closing bracer not found");
        }

        private object GetVariable(string name)
        {
            if (name.Equals("null"))
                return null;

            if (name.Equals("false"))
                return false;

            if (name.Equals("true"))
                return true;

            object val;
            for (int i = 0; i < _scopes.Length; i++)
            {
                if (_scopes[i].TryGetValue(name, out val))
                    return val;
            }
            throw new Exception("Variable '" + name + "' is not exists");
        }


        #region Static

        public static object Eval(string expression, params Dictionary<string, object>[] scopes)
        {
            return new Expression(expression, scopes).Process();
        }

        // TODO if result is null, return type-handler and use it for method searching
        public static object GetProperty(object obj, string name)
        {
            Type t;
            if (obj is Type) // TODO implement Type-property
            {
                t = (Type)obj;
                var p = t.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                if (p != null)
                    return p.GetValue(null);

                var fi = t.GetField(name, BindingFlags.Static | BindingFlags.Public);
                if (fi != null)
                    return fi.GetValue(null);
            }
            else
            {
                t = obj.GetType();
                var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (p != null)
                    return p.GetValue(obj);

                var fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
                if (fi != null)
                    return fi.GetValue(obj);
            }

            throw new InvalidOperationException(String.Format("Property '{0}' not exists for type {1}", name, t.Name));
        }

        private static object[] ConvertTypes(object[] values, ParameterInfo[] types)
        {
            if (values.Length != types.Length) throw new ArgumentException("Arrays length is not equal");

            object[] result = new object[values.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Convert.ChangeType(values[i], types[i].ParameterType);
            return result;
        }

        public static object Call(object val, string methodName, params object[] args)
        {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                    types[i] = args[i].GetType();
                else
                    types[i] = typeof(object); // TODO check
            }

            if (val is Type)
            {
                var t = (Type)val;
                var mi = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, types, null);
                if (mi == null)
                    throw new Exception(String.Format("Method '{0}({1})' not exists", methodName, String.Join(", ", (object[])types)));

                args = ConvertTypes(args, mi.GetParameters());
                return mi.Invoke(null, args);
            }
            else
            {
                var t = val.GetType();

                var mi = t.GetMethod(methodName, types);
                if (mi == null)
                    throw new Exception(String.Format("Method '{0}({1})' not exists", methodName, String.Join(", ", (object[])types)));


                args = ConvertTypes(args, mi.GetParameters());
                return mi.Invoke(val, args);
            }
        }

        #endregion
    }
}
