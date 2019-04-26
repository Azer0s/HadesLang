## 4/26/19

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

  