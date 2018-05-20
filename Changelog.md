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

**<u>24/04/2018</u>**

* Added support for web

* Added webserver

* Added page written in Hades

  * index.hd

  * about.hd

  * contact.hd

  * config

    * Written like

      ```
      config: localhost
      port: 5678
      ```

    * More settings will follow

* Todo: std library for web

**<u>25/04/2018</u>**

* Added routing options for HadesWeb

  * Option in config for routing file

    ```
    routing: main.hd
    ```

  * If no routing file is given, HadesWeb will switch to autorouting

  * Special hades functions:

    * `route:[word route,word action]` - Adds route to HadesWebEngine

      * e.g.: 

        ```vb
        route:['/','index']
        route:['/about','about']
        route:['/contact','contact']

        //This is a folder
        route:['/folder/hello','folder/hello']
        ```

        ​

    * `forward:[word filetype]` - Adds forward for filetype to HadesWebEngine

      * e.g.:

        ```vb
        forward:['.ico']
        ```

* Cleaned up `Programm.cs` in HadesWeb

* Added viewengine

  * supports variables

    ```html
    <p>${text}</p>
    ```

  * supports loops

    ```html
    <for:word x in y>
    	<p>${title}</p>
    	<h2>${x}</h2>
    </for>
    ```

* Added support for folders in routing

  ```vb
  route:['/contact/*','*']
  ```

  ​

**<u>26/04/2018</u>**

* Added params method to get request parameter
* Added getMethod method to get HTTP method
* Restructured HadesWeb project
* Added multithread options to HadesWeb Server
  * Pass multiple config files
  * HadesWeb Server will load config and run server in own thread
* Removed multithread support again

**<u>20/05/2018</u>**

* Added conditions in hdtml

  ```html
  <if:$cond>
      <p>
          ${hello} world
      </p>
  </if>
  
  <unless:inv $cond>
      <p>
          ayy ${name}!
      </p>
  </unless>
  ```

  