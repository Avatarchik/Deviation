﻿using Assets.Scripts.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Client;
using Assets.Scripts.Interface.DTO;
using Assets.Scripts.External;

namespace Assets.Scripts.Controllers
{
    public class MultiplayerController : MonoBehaviour, IMultiplayerController
    {
		private static int NUMBER_OF_PLAYERS;
		private static int CURRENT_ROUND;
		private static int NUMBER_OF_ROUNDS;
		private static int[] WINNERS;

		public int NumberOfPlayers { get { return NUMBER_OF_PLAYERS; } set { NUMBER_OF_PLAYERS = value; } }
		public int CurrentRound { get { return CURRENT_ROUND; } set { CURRENT_ROUND = value; } }
		public int NumberOfRounds { get { return NUMBER_OF_ROUNDS; } set { NUMBER_OF_ROUNDS = value; } }
		public int[] Winners { get { return WINNERS; } set { WINNERS = value; } }

		private IDeviationClient dc;
		private ILootPool _pool;

		public void Awake()
		{
			CURRENT_ROUND = 0;
			NUMBER_OF_ROUNDS = 3;
			WINNERS = new int[NUMBER_OF_ROUNDS];
			WINNERS.Initialize();
			dc = FindObjectOfType<DeviationClient>();
			_pool = new LootPool();
		}

		//instantiates a new multiplayer instance
		public void StartMultiplayerExchangeInstance()
        {
			if (CURRENT_ROUND < NUMBER_OF_ROUNDS)
			{
				CURRENT_ROUND++;
				SceneManager.LoadScene("MultiplayerExchange");
			}
			else
			{
				for (int i = 1; i <= WINNERS.Length; i++)
				{
					Debug.Log("Round " + i + " Winner: " + WINNERS[i-1]);
				}

				AllocateResources();
				Debug.Log("That's it Folks!");
				
				SceneManager.LoadScene("MultiplayerMenu");
				Destroy(gameObject);
			}
		}

		public void GetResource()
		{
			AllocateResources();
		}

		private void AllocateResources()
		{
			var loot = _pool.GetLoot();
			Debug.Log("Adding \"" + loot.Name + "\" to Resource Bag");
			dc.currentPlayer.ResourceBag.AddResource(loot);
		}

		public void OutputResourceBag()
		{
			foreach (string str in dc.currentPlayer.ResourceBag.ToStringArray())
			{
				Debug.Log(str);
			}
		}
    }
}
