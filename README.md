# ubivius-game

_lAST UPDATED ON APRIL 25, 2021_

**Which Unity version should I use ?**\n
Unity 2019.4.25f1

**Where is the build script ?**\n
In Assets/Editor/Build.cs. You can call functions ClientBuild() and ServerBuild() inside the Build static class with a command line.

**CI/CD**\n
The CI/CD specs are located in  .github\workflows\main.yml.

**Misc**\n
The main code location is situated in Assets/Scripts. There are 3 solutions : tests, assembly-ubv, and Assembly-CSharp-Editor, which respectively manage unit tests, game code and editor custom inspectors.
