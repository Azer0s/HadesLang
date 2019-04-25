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

  