using UnityEditor;
using System;

public class Build
{
    [MenuItem("BuildTools/Client Windows Build")]
    static void ClientBuild()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/Client/Init.unity",
            "Assets/Scenes/Client/ClientLogin.unity",
            "Assets/Scenes/Client/ClientMenu.unity",
            "Assets/Scenes/Client/ClientLobby.unity",
            "Assets/Scenes/Client/ClientGameSearch.unity",
            "Assets/Scenes/Client/ClientMyCharacters.unity",
            "Assets/Scenes/Client/ClientAchievement.unity",
            "Assets/Scenes/Client/ClientCharacterSelect.unity",
            "Assets/Scenes/Client/ClientGame.unity",
            "Assets/Scenes/Client/ClientEndGame.unity",
        };
        build.locationPathName = "/github/workspace/build/Client/ubivius-client.exe";
        build.target = BuildTarget.StandaloneWindows64;
        build.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(build);
    }

    [MenuItem("BuildTools/Server Linux Build")]
    static void ServerBuild()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/Server/Server.unity"
        };
        build.locationPathName = "/github/workspace/build/Server/ubivius-server.x86_64";
        build.target = BuildTarget.StandaloneLinux64;
        build.options = BuildOptions.EnableHeadlessMode;
        BuildPipeline.BuildPlayer(build);
    }

    [MenuItem("BuildTools/Server Windows Build")]
    static void ServerBuild_WIN()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/Server/Server.unity"
        };
        build.locationPathName = "/github/workspace/build/Server/ubivius-server.exe";
        build.target = BuildTarget.StandaloneWindows64;
        build.options = BuildOptions.EnableHeadlessMode;
        BuildPipeline.BuildPlayer(build);
    }
}
