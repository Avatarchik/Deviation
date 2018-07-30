﻿using Assets.Deviation.Exchange.Scripts;
using Assets.Deviation.MasterServer.Scripts;
using Barebones.Networking;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Deviation.MasterServer.Scripts
{
	public class ExchangeDataAccess
	{
		LiteDatabase db = new LiteDatabase(@"Exchange.db");
		LiteCollection<ExchangeResult> _exchangeResult;
		LiteCollection<ExchangeDataEntry> _exchangeData;
		string exchangeResultName = "ExchangeResult";
		string exchangeDataName = "ExchangeData";

		public ExchangeDataAccess()
		{
			BsonMapper.Global.RegisterType
			(
				serialize: (packet) => packet.ToBsonDocument(),
				deserialize: (bson) => new PlayerStatsPacket(bson.AsDocument)
			);

			BsonMapper.Global.RegisterType
			(
				serialize: (packet) => packet.ToBsonDocument(),
				deserialize: (bson) => new ActionModulePacket(bson.AsDocument)
			);

			BsonMapper.Global.RegisterType
			(
				serialize: (packet) => packet.ToBsonDocument(),
				deserialize: (bson) => new ExchangeResult(bson.AsDocument)
			);

			BsonMapper.Global.RegisterType
			(
				serialize: (packet) => packet.ToBsonDocument(),
				deserialize: (bson) => new ExchangeDataEntry(bson.AsDocument)
			);

			_exchangeResult = db.GetCollection<ExchangeResult>(exchangeResultName);
			_exchangeData = db.GetCollection<ExchangeDataEntry>(exchangeDataName);
		}

		//ExchangeResult Logic
		public List<ExchangeResult> GetExchangeResults(long exchangeDataId)
		{
			return _exchangeResult.Find(x => x.ExchangeId == exchangeDataId).ToList();
		}

		public List<ExchangeResult> GetExchangeResults(PlayerAccount player)
		{
			return _exchangeResult.Find(x => x.Player== player).ToList();
		}

		public ExchangeResult GetExchangeResult(long exchangeDataId, PlayerAccount player)
		{
			return _exchangeResult.FindOne(x => x.ExchangeId == exchangeDataId && x.Player == player);
		}

		public void CreateExchangeResult(ExchangeResult result)
		{
			_exchangeResult.Insert(result);
		}

		
		//ExchangeDataEntry Logic
		public List<ExchangeDataEntry> GetExchangeDataEntries(long exchangeDataId)
		{
			return _exchangeData.Find(x => x.ExchangeId == exchangeDataId).ToList();
		}

		public List<ExchangeDataEntry> GetExchangeDataEntries(PlayerAccount player)
		{
			return _exchangeData.Find(x => x.Player == player).ToList();
		}

		public ExchangeDataEntry GetExchangeDataEntry(long exchangeDataId, PlayerAccount player)
		{
			return _exchangeData.FindOne(x => x.ExchangeId == exchangeDataId && x.Player == player);
		}

		public void CreateExchangeData(ExchangeDataEntry entry)
		{
			_exchangeData.Insert(entry);
		}
	}
}
