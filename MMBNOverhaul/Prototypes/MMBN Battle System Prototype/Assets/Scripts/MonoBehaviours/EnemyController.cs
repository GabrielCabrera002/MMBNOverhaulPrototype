﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyController : LivingEntityController
{
	#region Fields
	[SerializeField] int enemyInfoID;
	[SerializeField] EnemyInfo _enemyInfo;
	EnemyAI ai;

	EnemyDatabase enemyDatabase;
	#endregion

	#region Properties
	#region For backing fields

	public EnemyInfo enemyInfo 
	{
		get { return _enemyInfo; }
		protected set { _enemyInfo = value; }
	}

	public override LivingEntityInfo entityInfo 
	{
		get { return _enemyInfo; }
		protected set { _enemyInfo = value as EnemyInfo; }
	}

	public override LivingEntityStats stats 
	{
		get { return _enemyInfo.stats; }

	}

	#endregion
	#endregion

	#region Methods

	protected override void Awake()
	{
		base.Awake();

		//Debug.Log("");
	}

	protected override void Start()
	{
		base.Start();
		enemyDatabase = EnemyDatabase.instance;

		SetupEnemyInfo();
		SetupAI();
	}

	protected override void Update()
	{
		base.Update();

		if (!isPaused)
		{
			if (ai != null)
				ai.Execute();

			movementHandler.Execute();
			//CheckDeath();
		}
	}

	void CheckDeath()
	{
		if (effectiveHealth <= 0)
		{
			Debug.Log(this.name + " deleted.");
			Destroy(this.gameObject);
		}
	}

	#region Helper funcs
	void SetupEnemyInfo()
	{
		enemyInfo = enemyDatabase.GetEnemyInfo(enemyInfoID);
	}

	void SetupAI()
	{
		//Debug.Log("");
		GameObject containerClone = Instantiate<GameObject>(enemyInfo.aiContainer.gameObject);

		EnemyAIContainer container = containerClone.GetComponent<EnemyAIContainer>();

		ai = container.ai;
		
		Destroy(containerClone.gameObject);

		ai.Init(this);
	}
	#endregion
	#endregion
	
}