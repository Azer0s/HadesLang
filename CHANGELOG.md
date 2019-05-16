## 5/16/19

### Introduced extension methods

This allows for cool things like behavior based programming where one could have a bunch of group classes ("interfaces"), extend functions based on group and import these extensions based on usage.

```swift
class IDomainObject
end

class Person < IDomainObject
    @public
        var string firstname
        var string lastname
		var object birthday
    end
end

[..]

func extends object::IDomainObject persist(object::IConnection connection)
	[..]
end

[..]

func extends object::IDomainObject toJson()
	put [..] //Default impl. of index on object returns all the members
end
```

So based on need, one could import `persist` or `toJson` as extensions. In the controller, for instance, `persist` is absolutely useless, so is `toJson` in a service.

## 5/5/19

### Fixed call on return #3dca537b60026e55dd33233da8ef11039da6980d

```js
{x => x}(19)
({x => x}(19))(1)
(({x => x}(19))(1))(19)

test()()
(test())()
((test())())()
```

## 5/4/19

### Added try-catch-else block #5d514e61ea4189601e8ca8bfbc99e8916394c9a9

```js
try
    connection->open()
    console->out("Connection open!")
    connection->close()
catch(object::SqlException e)
    console->out("SqlException was caught!")
catch(e)
    console->out("An unknown exception was caught!")
end
```

```js
try
    connection->open()
    console->out("Connection open!")
    connection->close()
catch(object::SqlException e)
    console->out("SqlException was caught!")
catch(e)
    console->out("An unknown exception was caught!")
else
	console->out("No exception thrown!")
end
```

```js
try
    connection->open()
    console->out("Connection open!")
    connection->close()
else //Exception is not handled; if there was no exception, else block is called
	console->out("No exception thrown!")
end
```

### Added if block, code beautification #8b3d4d6b35167b37a0efdb96e9ad2fc964719e1e

```js
if(a < 10)
    console->out("a is smaller than 10")
else if(a is 11)
    console->out("a is 11")
else if(a > 11 and a < 21)
    console->out("a is greater than 11 and smaller than 21")
else
    console->out("a is " + a)
end
```

```javascript
if(a < 10)
    console->out("a is smaller than 10")
else
    console->out("a is " + a)
end
```

```javascript
if(a < 10)
    console->out("a is smaller than 10")
end
```

```js
if(a < 10)
    console->out("a is smaller than 10")
else if(a is 11)
    console->out("a is 11")
end
```

## 5/3/19

### Added fixed prefix #5aca4880462c36dc87643e614bd7e38602f7e334

* Added the fixed prefix in front of func

### Added pipelines #f3129f23a2f7c7103a3dc8a89b19f846a84585a5

This: 

```js
fruits
|> map({x => x->toLower()}, ??)
|> filter({x => x->startsWith("a")}, ??)
|> forEach({x => console->out(x)}, ??)
```

is now a thing.

## 4/26/19

### Removed multidimensional array access #b2a09b8f55e2024277e7fbd0ad9331f20d5f013a

* We moved from `.` to `,` when accessing multidimensional array

  ```js
  var int[3,3] matrix = {{1,0,0},{0,1,0},{0,0,1}}
  var int?[2,2,2] 3dArray = {{{1,2},{3,null}},{{null,6},{7,8}}}
  ```

### Added specific object and proto validation to lambdas, function and variables #6623878e8ccec87a20c395cdd157931dea2b403b

* Variable instantiations, function arguments and lambda arguments can now name specific class-/proto names

  ```swift
  func doStuff(args object::IClient a)
  	a->stuff("Test")
  end
  ```

  ```swift
  var x = {args object::IClient a, int b =>
  	a->moreStuff(b)
  }
  ```

  ```js
  var proto::console c
  ```

## 4/23/19

### Added lambda parameter types #c244d04d419cb163ce7e5581990df1dd39c44190

* Lambdas can now have parameter types

  ```js
  var add = {x, y => x + y}
  ```

  ```js
  var mul = {int x, int y => x * y}
  ```

  ```js
  var sum = { args int vals => 
  	var result = 0
  
      for(var i in a)
          result += i
      end
  
      put result
  }
  ```

  