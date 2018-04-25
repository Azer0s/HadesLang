- [ ] Add documentation for HadesWeb
- [ ] Add std library for Web
- [ ] Rework comments in FileInterpreter
- [ ] Add viewengine for HadesWeb
  * Supports for loop and variables
  ```html
  <html>
    <body>
      <h1>${a}</h1>

      <for:word x in list>
        <ul>${x}</ul>
      <endfor>

    </body>
  </html>
  ```
