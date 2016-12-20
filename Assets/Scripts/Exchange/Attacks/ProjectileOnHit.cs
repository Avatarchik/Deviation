﻿using System;
using Assets.Scripts.Interface;
using UnityEngine;
using Assets.Scripts.Controllers;

namespace Assets.Scripts.Exchange
{
	public class ProjectileOnHit : MonoBehaviour, IExchangeAttack
	{
		private Attack Attack;
		private ExchangeController ec;

		void Awake()
		{
			//if the object is a portal rocket shoot rocket toward player
			if (gameObject.name.Equals("PortalRocket(Clone)"))
			{
				GetComponent<Rigidbody>().velocity = 2 * new Vector3(0, 0, -15);
			}

			//if the object is a normal rocket shoot rocket toward enemy
			if (gameObject.name.Equals("Rocket(Clone)"))
			{
				GetComponent<Rigidbody>().velocity = 2 * new Vector3(0, 0, 15);
			}

			if (ec == null)
			{
				var ecObject = GameObject.FindGameObjectWithTag("ExchangeController");
				ec = ecObject.GetComponent<ExchangeController>();
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			//if this objects hit a player attack it
			if (other.tag.Equals("Player") || other.tag.Equals("MainPlayer"))
			{
				Attack.SetDefender(other.GetComponent<Player>());
				Attack.InitiateDrain();
				ec.UpdateExchangeControlsDisplay();
				Destroy(gameObject);
			}
		}

		public void Update()
		{
			if (gameObject.transform.position.y <= -15.0)
			{
				Destroy(gameObject);
			}
		}

		public void SetAttack(Attack attack)
		{
			Attack = attack;
		}
	}
}
