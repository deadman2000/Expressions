# C# Expressions

Simple expression parser on C#

### Usage

```csharp
Dictionary<string, object> scope = new Dictionary<string, object>();
scope.Add("my", new MyClass());
string result = Expression.Eval("my.StringMethod()", scope);
```
