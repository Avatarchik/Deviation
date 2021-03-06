﻿using Assets.Deviation.Client.Scripts;
using Assets.Deviation.MasterServer.Scripts;
using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Deviation.Exchange.Scripts.Client
{
	public enum ClientState
	{
		Default = -1,
		Login = 0,
		Client = 1,
		Match = 2,
		Results = 3,
	}

	public class ClientDataRepository : MonoBehaviour
	{
		public static ClientDataRepository Instance = null;
		private static UnityAction _onInstanceCreated;

		public ClientState _state = ClientState.Default;
		public ClientState State
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
				OnClientDataStateChange?.Invoke(value);
			}
		}
		public UnityAction<ClientState> OnClientDataStateChange;

		public PlayerAccount PlayerAccount;

		public MatchFoundPacket Exchange;

		public int RoomId = -1;
		
		public UnityAction PlayerAccountRecieved;
		public UnityAction<AccountInfoPacket, string> OnLogin;

		public bool LoggedIn;
		public bool HasPlayerAccount;
		public bool HasExchange;

		public void Awake()
		{
			InstanceExists();
			OnClientDataStateChange += ClientDataStateChange;
			Msf.Client.SetHandler((short)Exchange1v1MatchMakingOpCodes.RespondRoomId, HandleReceiveRoomId);
		}

		private void ClientDataStateChange(ClientState state)
		{
			switch (state)
			{
				case ClientState.Default:
					break;
				case ClientState.Login:
					break;
				case ClientState.Client:
					break;
				case ClientState.Match:
					break;
				case ClientState.Results:
					RoomId = -1;
					Exchange = null;
					HasExchange = false;
					break;
			}
		}

		public void InstanceExists()
		{
			if (Instance == null)
			{
				Instance = this;
				_onInstanceCreated?.Invoke();
				DontDestroyOnLoad(Instance);
			}
			else if (Instance != this)
			{
				Destroy(gameObject);
			}
		}

		public static void OnInstanceCreated(UnityAction onInstanceCreated)
		{
			if (Instance == null)
			{
				_onInstanceCreated = onInstanceCreated;
			}
			else
			{
				onInstanceCreated();
			}
		}

		public void HandleReceiveRoomId(IIncommingMessage message)
		{
			Instance.RoomId = message.AsInt();
		}

		public void LoginAsGuest()
		{
			if (Msf.Client.Auth.IsLoggedIn)
			{
				return;
			}
			
			if (Msf.Client.Connection.IsConnected)
			{
				Msf.Client.Auth.LogInAsGuest((successful, error) =>
				{
					OnLogin?.Invoke(successful, error);
					LoggedIn = true;
					UnityEngine.Debug.Log("Logged in successfully");
				});
			}
			else
			{
				Debug.LogError("Not Connected To Server. Failed to login as guest.");
			}
		}

		public void GetPlayerAccount()
		{
			if (HasPlayerAccount)
			{
				return;
			}

			if (Msf.Client.Connection.IsConnected)
			{
				if (Msf.Client.Auth.IsLoggedIn)
				{
					Msf.Client.Connection.SendMessage((short)ExchangePlayerOpCodes.GetPlayerAccount, Msf.Client.Auth.AccountInfo.Username, (status, response) =>
					{

						PlayerAccount = response.Deserialize(new PlayerAccount());
						PlayerAccountRecieved?.Invoke();
						Msf.Client.Connection.SendMessage((short)ExchangePlayerOpCodes.Login, PlayerAccount);
						HasPlayerAccount = true;
						Debug.Log("Player Account Successfully Recieved");

					});
				}
				else
				{
					Debug.LogError("Not Logged in. Failed to get player account.");
				}
			}
			else
			{
				Debug.LogError("Not Connected To Server. Failed to get player account.");
			}
		}
	}
}
