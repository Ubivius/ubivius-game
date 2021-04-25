# ubivius-game

_lAST UPDATED ON APRIL 25, 2021_\\

**Which Unity version should I use ?**\
Unity 2019.4.25f1

**Where is the build script ?**\
In Assets/Editor/Build.cs. You can call functions ClientBuild() and ServerBuild() inside the Build static class with a command line.

**CI/CD**\
The CI/CD specs are located in  .github\workflows\main.yml.

**Misc**\
The main code location is situated in Assets/Scripts. There are 3 solutions : tests, assembly-ubv, and Assembly-CSharp-Editor, which respectively manage unit tests, game code and editor custom inspectors.
