﻿using UnityEngine;
using Assets.Scripts.Enum;
using Assets.Scripts.Controllers;
using Assets.Scripts.Library;
using Assets.Scripts.Utilities;
using Assets.Scripts.Interface;
using Assets.Scripts.Interface.DTO;
using Assets.Scripts.Interface.Exchange;
using Assets.Scripts.Exchange.NPC;
using Assets.Scripts.DTO.Exchange;

namespace Assets.Scripts.Exchange
{
	public class Player : MonoBehaviour, IExchangeObject, IPlayer
	{
		public bool IsMainPlayer { get; set; }
		public Transform Transform { get { return transform; } }
		public Battlefield Battlefield { get; set; }
		public IKit EquipedKit { get; set; }
		public int Health { get; set; }
		public int MinHealth { get; set; }
		public int MaxHealth { get; set; }
		public int Energy { get; set; }
		public int MinEnergy { get; set; }
		public int MaxEnergy { get; set; }
		public float EnergyRate { get; set; }
		public IPlayer[] Enemies { get; set; }
		public IBattlefieldController BattlefieldController { get; set; }
		public IExchangeController ExchangeController { get; set; }
		public ITimerManager TimerManager { get; set; }
		public INPCController NPCController { get; set; }
		public int CurrentColumn { get; set; }
		public int CurrentRow { get; set; }
		public IModule CurrentModule { get { return EquipedKit.GetCurrentModule(); } set { CurrentModule = value; } }
		public IExchangeAction CurrentAction { get { return EquipedKit.GetCurrentModule().GetCurrentAction(); } set { CurrentAction = value; } }

		private MovingDetails _movingDetails;
		private float _restoreEnergy;

		public void SetPlayer(bool isMainPlayer, Battlefield startField, IKit kit, float energyRate, int maxHealth, int maxEnergy, int minHealth, int minEnergy)
		{
			IsMainPlayer = isMainPlayer;
			Battlefield = startField;
			EquipedKit = kit;
			kit.Player = this;
			EnergyRate = energyRate;
			MinHealth = minHealth;
			MinEnergy = minEnergy;
			MaxHealth = maxHealth;
			MaxEnergy = maxEnergy;

			_restoreEnergy = 0;
			ResetHealth();
			ResetEnergy();
			UpdateLocation(0, 0);
			CreateTimersForKitActions();
		}

		public void Awake()
		{
			BattlefieldController = FindObjectOfType<BattlefieldController>();
			ExchangeController = FindObjectOfType<ExchangeController>();
			TimerManager = FindObjectOfType<TimerManager>();
			NPCController = FindObjectOfType<NPCController>();

			BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), true);
		}

		public void Update()
		{
			CheckMovingDetails();
			RestoreEnergy();
		}

		//restores energy
		public void RestoreEnergy()
		{
			_restoreEnergy += EnergyRate;
			if (_restoreEnergy > 1)
			{
				AddEnergy(10);
				_restoreEnergy = 0.0f;
				ExchangeController.UpdateExchangeControlsDisplay();
			}
		}

		//reset health to max
		public void ResetHealth()
		{
			Health = MaxHealth;
		}

		//reset energy to max
		public void ResetEnergy()
		{
			Energy = MaxEnergy;
		}

		//sets health
		public void SetHealth(int set)
		{
			Health = AddStatWithinBoundary(set, 0, MinHealth, MaxHealth);
		}

		//sets energy
		public void SetEnergy(int set)
		{
			Energy = AddStatWithinBoundary(set, 0, MinEnergy, MaxEnergy);
		}

		//adds specified health
		public void AddHealth(int add)
		{
			Health = AddStatWithinBoundary(Health, add, MinHealth, MaxHealth);
			if (add > 0 && CheckIsNPC(Enemies[0]))
			{
				AddDecision(Enemies[0], Decision.Action, 5);
			}
		}

		//adds specified energy
		public void AddEnergy(int add)
		{
			Energy = AddStatWithinBoundary(Energy, add, MinEnergy, MaxEnergy);
			if (add > 0 && CheckIsNPC(Enemies[0]))
			{
				AddDecision(Enemies[0], Decision.Action, 5);
			}
		}


		//moves the player over time
		public bool MoveObject(Direction direction, int distance, bool force = false)
		{
			if (_movingDetails != null)
			{
				return false;
			}
			int destPoint;

			bool success = false;
			switch (direction)
			{
				case Direction.Right:
					if (force || !BattlefieldController.GetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn + 1)))
					{
						destPoint = CurrentColumn + 1;
						if (destPoint <= 2)
						{
							_movingDetails = new MovingDetails(new Vector3(destPoint, 0, CurrentRow), direction);
							success = true;
						}
					}
					break;
				case Direction.Left:
					if (force || !BattlefieldController.GetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn - 1)))
					{
						destPoint = CurrentColumn - 1;
						if (destPoint >= -2)
						{
							_movingDetails = new MovingDetails(new Vector3(destPoint, 0, CurrentRow), direction);
							success = true;
						}
					}
					break;
				case Direction.Up:
					if (force || !BattlefieldController.GetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow + 1), ConvertToArrayNumber(CurrentColumn)))
					{
						destPoint = CurrentRow + 1;
						if (destPoint <= 2)
						{
							_movingDetails = new MovingDetails(new Vector3(CurrentColumn, 0, destPoint), direction);
							success = true;
						}
					}
					break;
				case Direction.Down:
					if (force || !BattlefieldController.GetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow - 1), ConvertToArrayNumber(CurrentColumn)))
					{
						destPoint = CurrentRow - 1;
						if (destPoint >= -2)
						{
							_movingDetails = new MovingDetails(new Vector3(CurrentColumn, 0, destPoint), direction);
							success = true;
						}
					}
					break;
			}

			if (CheckIsNPC(Enemies[0]))
			{
				AddDecision(Enemies[0], Decision.Move, 35);
				AddDecision(Enemies[0], Decision.Action, 5);
			}
			return success;
		}

		//moves the player instantly
		public void MoveObject_Instant(int row, int column)
		{
			UpdateLocation(row, column);
			UpdateTransform(row, column);
		}

		//uses the current action
		public bool PrimaryAction()
		{
			bool success = false;
			IExchangeAction currentAction = EquipedKit.GetCurrentModule().GetCurrentAction();

			int attackCost = (int)(currentAction.Attack.EnergyRecoilModifier * currentAction.Attack.BaseDamage);
			int potentialEnergy = Energy + attackCost;
			if (TimerManager.TimerUp(currentAction.Name) && potentialEnergy >= MinEnergy)
			{
				currentAction.InitiateAttack(BattlefieldController);

				TimerManager.StartTimer(currentAction.Name);
				success = true;
			}

			if (CheckIsNPC(Enemies[0]))
			{
				AddDecision(Enemies[0], Decision.Move, 25);
				AddDecision(Enemies[0], Decision.Action, 5);
			}
			return success;
		}

		//uses the current module ultimate
		public bool PrimaryModule()
		{
			bool success = false;
			success = true;

			if (CheckIsNPC(Enemies[0]))
			{
				AddDecision(Enemies[0], Decision.Move, 25);
				AddDecision(Enemies[0], Decision.Action, 5);
			}
			return success;
		}

		//Cycle Action Left
		public bool CycleActionLeft()
		{
			EquipedKit.GetCurrentModule().CycleActionLeft();
			return true;
		}

		//Cycle Action Right
		public bool CycleActionRight()
		{
			EquipedKit.GetCurrentModule().CycleActionRight();
			return true;
		}

		//Cycle Module Left
		public bool CycleModuleLeft()
		{
			EquipedKit.CycleModuleLeft();
			return true;
		}

		//Cycle Module Right
		public bool CycleModuleRight()
		{
			EquipedKit.CycleModuleRight();
			return true;
		}


		//Cycles the Battlefield counter clockwise
		public bool CycleBattlefieldCC()
		{
			return true;
		}

		//cycles the battlefiled clockwise
		public bool CycleBattlefieldCW()
		{
			return true;
		}









		private bool CheckIsNPC(IPlayer target)
		{
			foreach (IPlayer player in NPCController.NPCPlayers)
			{
				if (target.Equals(player))
					return true;
			}

			return false;
		}

		private void AddDecision(IPlayer target, Decision decision, int add)
		{
			NPCController.State.DecisionAdd(decision, add);
		}

		private int AddStatWithinBoundary(int current, int add, int statMin, int statMax)
		{
			int retVal = current + add;

			if (retVal > statMax)
			{
				retVal = statMax;

			}
			else if (retVal < statMin)
			{
				retVal = statMin;
			}

			return retVal;
		}


		//create a timer for each action in each module
		private void CreateTimersForKitActions()
		{
			IKit kit = EquipedKit;
			IModule currentModule = EquipedKit.GetCurrentModule();

			for (int i = 0; i < kit.ModuleCount; i++)
			{
				IExchangeAction currentAction = currentModule.GetCurrentAction();
				for (int j = 0; j < currentModule.ActionCount; j++)
				{
					TimerManager.AddAttackTimer(currentAction.Name, currentAction.Cooldown);
					currentAction = currentModule.GetRightAction();
					currentModule.CycleActionRight();
				}

				currentModule = kit.GetRightModule();
				kit.CycleModuleRight();
			}
		}

		//Utilities move this!!!!!!
		private int ConvertToArrayNumber(int input)
		{
			return input + 2;
		}

		private int ConvertFromArrayNumber(int input)
		{
			return input - 2;
		}

		//update current location of the player
		private void UpdateLocation(int row, int column)
		{
			if (!BattlefieldController.GetBattlefieldState(Battlefield, ConvertToArrayNumber(column), ConvertToArrayNumber(row)))
			{
				CurrentColumn = column;
				CurrentRow = row;
			}
		}

		//moves the player over time
		private void UpdateTransform(float row, float column)
		{
			transform.localPosition = new Vector3(column, 0, row);
			transform.Translate(0, 0, 0);
		}

		//update moving details
		private void CheckMovingDetails()
		{
			if (_movingDetails != null)
			{
				switch (_movingDetails.MovingDirection)
				{
					case Direction.Up:
						if (transform.localPosition.z >= _movingDetails.Destination.z)
						{
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), false);
							CurrentRow += 1;
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), true);
							UpdateTransform(CurrentRow, CurrentColumn);
							_movingDetails = null;
						}
						else
						{
							UpdateTransform(transform.localPosition.z + 0.25f, transform.localPosition.x);
						}
						break;
					case Direction.Right:
						if (transform.localPosition.x >= _movingDetails.Destination.x)
						{
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), false);
							CurrentColumn += 1;
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), true);
							UpdateTransform(CurrentRow, CurrentColumn);
							_movingDetails = null;
						}
						else
						{
							UpdateTransform(transform.localPosition.z, transform.localPosition.x + 0.25f);
						}
						break;
					case Direction.Left:
						if (transform.localPosition.x <= _movingDetails.Destination.x)
						{
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), false);
							CurrentColumn -= 1;
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), true);
							UpdateTransform(CurrentRow, CurrentColumn);
							_movingDetails = null;
						}
						else
						{
							UpdateTransform(transform.localPosition.z, transform.localPosition.x - 0.25f);
						}
						break;
					case Direction.Down:
						if (transform.localPosition.z <= _movingDetails.Destination.z)
						{
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), false);
							CurrentRow -= 1;
							BattlefieldController.SetBattlefieldState(Battlefield, ConvertToArrayNumber(CurrentRow), ConvertToArrayNumber(CurrentColumn), true);
							UpdateTransform(CurrentRow, CurrentColumn);
							_movingDetails = null;
						}
						else
						{
							UpdateTransform(transform.localPosition.z - 0.25f, transform.localPosition.x);
						}
						break;
				}
			}
		}
	}
}
