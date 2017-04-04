﻿using Assets.Scripts.Interface;

namespace Assets.Scripts.Client
{
	public class PlayerAccount : IPlayerAccount
	{
		public string AccountName { get; set; }
		public string AccountAlias { get; set; }
		public IResourceBag ResourceBag { get; set; }

		public PlayerAccount(string accountName, string accountAlias, IResourceBag resourceBag)
		{
			AccountName = accountName;
			AccountAlias = accountAlias;
			ResourceBag = resourceBag;
		}
	}
}
