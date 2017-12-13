﻿using UnityEngine;
using System.Collections;
using Barebones.MasterServer;
using System.Collections.Generic;
using System;
using Assets.Deviation.Exchange.Scripts;
using Assets.Deviation.Exchange.Scripts.Client;

public class StandaloneController : MonoBehaviour
{
	protected SpawnRequestController Request;
	private ClientDataController cdc;

	public void Awake()
	{
		cdc = FindObjectOfType<ClientDataController>();
		JoinServer();
	}

	public void JoinServer()
	{
		Msf.Client.Rooms.GetAccess(cdc.RoomId, OnPassReceived);
	}

	protected void OnPassReceived(RoomAccessPacket packet, string errorMessage)
	{
		var loadingPromise = Msf.Events.FireWithPromise(Msf.EventNames.ShowLoading, "Joining lobby");

		if (packet == null)
		{
			Msf.Events.Fire(Msf.EventNames.ShowDialogBox, DialogBoxData.CreateError(errorMessage));
			Logs.Error(errorMessage);
			return;
		}

		Logs.Info("Connecting to Game");

		FindObjectOfType<ExchangeRoomConnector>().ConnectToGame(packet);
	}
}
