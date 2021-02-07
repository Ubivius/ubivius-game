﻿using UnityEditor;
using System;

public class Build
{
    static void ClientBuild()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/ClientScene.unity",
            "Assets/Scenes/Prototypes/proto_move_client.unity"
        };
        build.locationPathName = "/github/workspace/build/Client/ubivius-client.exe";
        build.target = BuildTarget.StandaloneWindows64;
        build.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(build);
    }

    static void ServerBuild()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/ServerScene.unity",
            "Assets/Scenes/Prototypes/proto_move_server.unity"
        };
        build.locationPathName = "/github/workspace/build/Server/ubivius-server.exe";
        build.target = BuildTarget.StandaloneWindows64;
        build.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(build);
    }
}
