<img src="https://raw.githubusercontent.com/Azer0s/HadesLang/master/HadesLang/IconLong.png" /> 

***

[![Build Status](https://travis-ci.org/Azer0s/HadesLang.svg?branch=master)](https://travis-ci.org/Azer0s/HadesLang)
[![License](https://img.shields.io/badge/license-MIT-brightgreen.svg)](https://github.com/Azer0s/HadesLang/blob/master/LICENSE)


*Hades is a embedded scripting language built from scratch!*
<br>
Here are a few examples of the language in action:

## Register a custom function in C#
```csharp
var interpreter = new Interpreter(new ConsoleOutput(),new ConsoleOutput());
interpreter.RegisterFunction(new Function("print", a =>
{
    a.ToList().ForEach(Console.WriteLine);
    return string.Empty;
}));
```

## Register an alias in C#
```csharp
Evaluator.AliasManager.Add("clear","cls");
```

## Hello world
```vb
out:'Hello world'
```

## Fibonacci Numbers
### fibonacci.hades
```vb
func fib[num n]
    if[($n is 0) or ($n is 1)]
        put n
    end

    put fib:[$n-1] + fib:[$n-2]
end
```
### main.hades
```vb
with 'fibonacci.hades' as a
b as num reachable = $a->fib:[7]
out:b
```
## Force value through interpreter 

```vb
a as word = 'out:b'
b as word = 'Hello world'
#a
```
Output will be Hello world

## Overarching variables
### foo.hades
```vb
a as num global = 18
with 'bar.hades' as b
$b->printA:[]
```
### bar.hades
```vb
func printA[]
    out:a
end
```

## Method guards
```vb
func t1[num a] requires $a smaller 10
  out:'a is smaller 10'
end

func t1[num a] requires $a is 11
  out:'a is 11'
end
```

| Call        | Output          |
| ------------- |:-------------:|
| `t1:[11]`      | a is 11 |
| `t1:[9]`      | a is smaller 10      |
| `t1:[100]` | *No method will be called*      |

## Reflection
```vb
foo as word = 'Hello world'
bar as bit = false
foobar as num = 22

arr as word[*] = getfields:[]
out:#arr[0]
```

## Pipeline

*Syntactic sugar for nested method calls*
```vb
with 'fibrec.hades' as a
$a->fib:[9] |> out:[??]
```
## Conditions
```vb
if[sqrt(9) smallerIs 3]
    out:'The squareroot of 9 is smaller/equals 3'
end
```

## Unloading
```vb
a as num reachable = 18
uload:a
```

## Credits
[mXparser](https://github.com/mariuszgromada/MathParser.org-mXparser) by Mariusz Gromada
