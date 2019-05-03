## 5/3/19

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

  