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