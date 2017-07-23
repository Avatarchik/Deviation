﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
/// <summary>
/// Instead of editing this script, I would recommend to write your own
/// (or copy and change it). Otherwise, your changes will be overwriten when you
/// update project :)
/// </summary>
public class DeviationBuild
{
	/// <summary>
	/// Have in mind that if you change it, it might take "a while" 
	/// for the editor to pick up changes 
	/// </summary>
	public static string Client = "Assets/Deviation.Client/DeviationClient.unity";
	public static string MasterServer = "Assets/Deviation.MasterServer/MasterServer.unity";
	public static string Exchange1v1Scene = "Assets/Deviation.Exchange/Exchange.1v1/GameServer/Scenes/1v1Exchange.unity";
	public static string Exchange1v1SpawnerServer = "Assets/Deviation.Exchange/Exchange.1v1/GameServer/Scenes/1v1ExchangeSpawnerServer.unity";
	public static string Exchange1v1GameServer = "Assets/Deviation.Exchange/Exchange.1v1/GameServer/Scenes/1v1ExchangeGameServer.unity";

	public static BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

	private static Dictionary<string, string> _serverLocations = new Dictionary<string, string>();
	/// <summary>
	/// Build with "Development" flag, so that we can see the console if something 
	/// goes wrong
	/// </summary>
	public static BuildOptions BuildOptions = BuildOptions.Development;

	public static string PrevPath = null;

	[MenuItem("Tools/Deviation/Build All", false, 0)]
	public static void BuildGame()
	{
		var path = GetPath();
		if (string.IsNullOrEmpty(path))
			return;

		BuildMaster(path);
		BuildSpawner(path);
		BuildClient(path);
		BuildGameServer(path);
	}

	[MenuItem("Tools/Deviation/Start Deviation Servers", false, 0)]
	public static void StartServer()
	{
		StartMaster();
		StartSpawner();
		Start1v1ExchangeGameServer();
	}

	[MenuItem("Tools/Deviation/Start Master Server", false, 11)]
	public static void StartMaster()
	{
		foreach (var process in Process.GetProcessesByName("MasterServer"))
		{
			UnityEngine.Debug.Log("Shutting down: " + process.ProcessName);
			process.Kill();
		}
		var commandLineArgs = " -msfStartMaster -batchmode";
		var exePath = GetServerLocation("MasterServer");
		UnityEngine.Debug.Log(exePath + commandLineArgs);

		Process.Start(exePath, commandLineArgs);
	}

	[MenuItem("Tools/Deviation/Start Spawner Server", false, 11)]
	public static void StartSpawner()
	{
		foreach (var process in Process.GetProcessesByName("1v1ExchangeSpawnerServer"))
		{
			UnityEngine.Debug.Log("Shutting down: " + process.ProcessName);
			process.Kill();
		}
		var commandLineArgs = " -msfStartSpawner -batchmode";
		var exePath = GetServerLocation("1v1ExchangeSpawnerServer");
		UnityEngine.Debug.Log(exePath + commandLineArgs);

		Process.Start(exePath, commandLineArgs);
	}

	[MenuItem("Tools/Deviation/Start 1v1 Exchange Game Server", false, 11)]
	public static void Start1v1ExchangeGameServer()
	{
		foreach (var process in Process.GetProcessesByName("1v1ExchangeGameServer"))
		{
			UnityEngine.Debug.Log("Shutting down: " + process.ProcessName);
			process.Kill();
		}
		var exePath = GetServerLocation("1v1ExchangeGameServer");
		var commandLineArgs = " -msfLoadScene 1v1Exchange";
		UnityEngine.Debug.Log(exePath + commandLineArgs);
		Process.Start(exePath, commandLineArgs);
	}

	/// <summary>
	/// Creates a build for master
	/// </summary>
	/// <param name="path"></param>
	public static void BuildMaster(string path)
	{
		string exePath = path + "/MasterServer.exe";

		var masterScenes = new[]
		{
			MasterServer
		};
		AddServerLocationsToDict("MasterServer", exePath);

		BuildPipeline.BuildPlayer(masterScenes, exePath, TargetPlatform, BuildOptions);
	}

	/// <summary>
	/// Creates a build for spawner
	/// </summary>
	/// <param name="path"></param>
	public static void BuildSpawner(string path)
	{
		string exePath = path + "/1v1ExchangeSpawnerServer.exe";

		var spawnerScenes = new[]
		{
			Exchange1v1SpawnerServer
		};
		AddServerLocationsToDict("1v1ExchangeSpawnerServer", exePath);
		BuildPipeline.BuildPlayer(spawnerScenes, exePath, TargetPlatform, BuildOptions);
	}

	/// <summary>
	/// Creates a build for client
	/// </summary>
	/// <param name="path"></param>
	public static void BuildClient(string path)
	{
		string exePath = path + "/DeviationClient.exe";

		var clientScenes = new[]
		{
			Client,

			// Add all the game scenes
			Exchange1v1Scene,

		};
		AddServerLocationsToDict("DeviationClient", exePath);
		BuildPipeline.BuildPlayer(clientScenes, exePath, TargetPlatform, BuildOptions);
	}

	/// <summary>
	/// Creates a build for game server
	/// </summary>
	/// <param name="path"></param>
	public static void BuildGameServer(string path)
	{
		string exePath = path + "/1v1ExchangeGameServer.exe";
		var gameServerScenes = new[]
		{
			Exchange1v1GameServer,

			// Add all the game scenes
			Exchange1v1Scene,

		};
		AddServerLocationsToDict("1v1ExchangeGameServer", exePath);
		BuildPipeline.BuildPlayer(gameServerScenes, exePath, TargetPlatform, BuildOptions);
	}

	#region Editor Menu

	[MenuItem("Tools/Deviation/Build Master", false, 11)]
	public static void BuildMasterMenu()
	{
		var path = GetPath();
		if (!string.IsNullOrEmpty(path))
		{
			BuildMaster(path);
		}
	}

	[MenuItem("Tools/Deviation/Build Spawner", false, 11)]
	public static void BuildSpawnerMenu()
	{
		var path = GetPath();
		if (!string.IsNullOrEmpty(path))
		{
			BuildSpawner(path);
		}
	}

	[MenuItem("Tools/Deviation/Build Client", false, 11)]
	public static void BuildClientMenu()
	{
		var path = GetPath();
		if (!string.IsNullOrEmpty(path))
		{
			BuildClient(path);
		}
	}

	[MenuItem("Tools/Deviation/Build Game Server", false, 11)]
	public static void BuildGameServerMenu()
	{
		var path = GetPath();
		if (!string.IsNullOrEmpty(path))
		{
			BuildGameServer(path);
		}
	}


	#endregion

	public static string GetPath()
	{
		string prevPath = EditorPrefs.GetString("msf.buildPath", "");
		string path = "";
		if (prevPath == "")
		{
			path = EditorUtility.SaveFolderPanel("Choose Location for binaries", prevPath, "");
		}
		else
		{
			path = prevPath;
		}

		if (!string.IsNullOrEmpty(path))
		{
			EditorPrefs.SetString("msf.buildPath", path);
		}
		return path;
	}

	public static void AddServerLocationsToDict(string serverExe, string path)
	{
		if (_serverLocations.ContainsKey(serverExe))
		{
			_serverLocations[serverExe] = path;
		}
		else
		{
			_serverLocations.Add(serverExe, path);
		}
	}

	public static string GetServerLocation(string serverName)
	{
		if (_serverLocations.ContainsKey(serverName))
		{
			return _serverLocations[serverName];
		}
		else
		{
			var path = GetPath();
			if (string.IsNullOrEmpty(path))
				return "";
			var exePath = path + "/" + serverName + ".exe";
			AddServerLocationsToDict(serverName, exePath);
			return exePath;
		}
	}
}