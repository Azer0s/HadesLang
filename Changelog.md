**<u>30/1/2018</u>**

* Fixed pipeline in method call

```vb
with 'fibrec' as a
$a->fib:[in:[] |> raw:[??]]
```



* Now, file extension doesnt have to be given when loading files

Used to be:

```vb
with 'fibrec.hades' as a
```

Now:

```vb
with 'fibrec' as a
```

<u>Note: Old version can still be used</u>



**<u>08/04/2018</u>**

* Changed extension from .hades to .hd
* Added `%optmize%` flag
  * Preemptively optmizes a script (runs it through the Interpreter and collects the lexed calls in the CallCache)

Usage:

```vb
%optimize%
%import fibrec.hd%

a as num = 0

while[$a smaller 10]
	out:[fib:[$a]]
	a++
end
```



* Moved some `SetOutput` calls to `finally`  (removed duplicate code)