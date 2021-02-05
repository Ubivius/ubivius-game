using UnityEditor;
using UnityEditor.Build.Reporting;
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
        build.locationPathName = "clientBuild";
        build.target = BuildTarget.StandaloneWindows64;
        build.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(build);
        BuildSummary summary = report.summary;

        Debug.Log("Build message: OutputPath - " + summary.outputPath + ", BuildSize - " + summary.totalSize.toString(summary.totalSize)));
    }

    static void ServerBuild()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
            "Assets/Scenes/ServerScene.unity",
            "Assets/Scenes/Prototypes/proto_move_server.unity"
        };
        build.locationPathName = "serverBuild";
        build.target = BuildTarget.StandaloneLinux64;
        build.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(build);
    }
}