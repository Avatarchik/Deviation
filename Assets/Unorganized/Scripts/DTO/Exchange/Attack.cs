﻿using Assets.Scripts.Interface.DTO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DTO.Exchange
{
	public struct Attack : IAttack
	{
		//this is the base damage of the attack
		public int BaseDamage { get; set; }

		//these modifiers apply to the basedamage and only affect the attacker
		public float HealthRecoilModifier { get; set; }
		public float EnergyRecoilModifier { get; set; }

		//these modifiers apply to the basedamage and only affect the defender
		public float HealthDrainModifier { get; set; }
		public float EnergyDrainModifier { get; set; }

		public Attack( int baseDamage = 0, float healthDrainModifier = -1.0f, float energyDrainModifier = 0.0f, float healthRecoilModifier = 0.0f, float energyRecoilModifier = -1.0f)
		{
			BaseDamage = baseDamage;
			HealthDrainModifier = healthDrainModifier;
			EnergyDrainModifier = energyDrainModifier;
			HealthRecoilModifier = healthRecoilModifier;
			EnergyRecoilModifier = energyRecoilModifier;
		}

		public void InitiateAttack(List<IExchangePlayer> allies, List<IExchangePlayer> enemies)
		{
			InitiateAttack(allies, AttackAlignment.Allies);
			InitiateAttack(enemies, AttackAlignment.Enemies);
		}

		public void ApplyEffect(List<IExchangePlayer> targets, StatusEffect effect, float timeout, float rate = 0f)
		{
			foreach (IExchangePlayer player in targets)
			{
				player.Status.ApplyEffect(effect, timeout, rate);
			}
		}

		public void InitiateAttack(List<IExchangePlayer> targets, AttackAlignment alignment)
		{
			Debug.LogError("Attack - InitiateAttack. Targeting: " + alignment);
			switch (alignment)
			{
				case AttackAlignment.Allies:
					DeliverDamage(targets, EnergyRecoilModifier, HealthRecoilModifier);
					break;
				case AttackAlignment.Enemies:
					DeliverDamage(targets, EnergyDrainModifier, HealthDrainModifier);
					break;
			}
		}

		public int GetHealthCost(AttackAlignment alignment)
		{
			switch (alignment)
			{
				case AttackAlignment.Allies:
					return GetDamage(HealthRecoilModifier);
				case AttackAlignment.Enemies:
					return GetDamage(HealthDrainModifier);
				default:
					return 0;
			}
		}

		public int GetEnergyCost(AttackAlignment alignment)
		{
			switch (alignment)
			{
				case AttackAlignment.Allies:
					return GetDamage(EnergyRecoilModifier);
				case AttackAlignment.Enemies:
					return GetDamage(EnergyDrainModifier);
				default:
					return 0;
			}
		}

		private void DeliverDamage(List<IExchangePlayer> targets, float energyModifier, float healthModifier)
		{
			foreach (IExchangePlayer player in targets)
			{
				player.Energy.Add(GetDamage(energyModifier));
				player.Health.Add(GetDamage(healthModifier));
			}
		}

		private int GetDamage(float modifier)
		{
			return (int)(modifier * BaseDamage);
		}
	}
}
