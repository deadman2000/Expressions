# C# Expressions

Simple expression evaluator on C#

### Usage

```csharp
Dictionary<string, object> scope = new Dictionary<string, object>();
scope.Add("my", new MyClass());
string result = (string)Expression.Eval("my.StringMethod()", scope);
```

### Features

Getting values of public properties and variables
```csharp
Expression.Eval("my.myVar", scope);
Expression.Eval("my.MyProp", scope);
```

Method calling
```csharp
Expression.Eval("my.StringMethod()", scope);
```

Chaining
```csharp
Expression.Eval("my.StringMethod('def').Length", scope);
```

Static properties call
```csharp
Dictionary<string, object> scope = new Dictionary<string, object>();
scope.Add("MyClass", typeof(MyClass));
Expression.Eval("MyClass.MyStaticProperty", scope);
Expression.Eval("MyClass.MyStaticMethod('abc')", scope);
```

Keyword `null` support
```csharp
Expression.Eval("my.Test_Method_2000(null)", scope);
```

### TODO
* arithmetic and boolean opertors
* boolean keywords (true, false)
* add system classes (Math, DateTime) by default
