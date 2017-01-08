# Hades - Breathing new life into scripting
*Tired of seeing the same old scripting language over and over again?*
<br>
*Hades is a scripting language built from scratch!*
<br>
<br>
Here are a few examples of the language in action:

## Hello world
```
out:'Hello world'
```

## Fibonacci Numbers
```
a as num closed = 1
b as num closed = 1

runala[true]
out:a
out:b

a = {a} + {b}
b = {a} + {b}
endala
```

## Overarching variables
### foo.hades
```
a as num reachable_all = 18
load:'bar.hades' as b
b->printA:void
```
### bar.hades
```
func printA[]
out:a
endfunc
```

## Conditions
```
case[Sqrt(9) smallerIs 3]
out:'The squareroot of 9 is smaller/equals 3'
endcase
```

## Unloading
```
a as num reachable = 18
uload:a
```
