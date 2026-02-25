# Todo
- i thing getcompilationErros saves to db after compiling
- make sure references are safe, more for future
- add a constructor to the customerScript class maybe add 2, 1 empty one so stuff dont break
- make sure when calling executebyid that you dont have to pass the required context in the useful method its super stupid rn
- add namespaces without using

# Converting to a class library:
### In sandbox.csproj make sure to uncomment:
`<OutputType>Library</OutputType>`

and comment the line above.

### Remove MainProgram.cs
move it to seperate test/console project that references the library.
