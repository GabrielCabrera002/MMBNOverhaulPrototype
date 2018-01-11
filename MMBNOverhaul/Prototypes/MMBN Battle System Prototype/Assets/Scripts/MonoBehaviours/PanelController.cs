﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PanelController : ObservableMonoBehaviour, IBattlefieldPanel, IPausable
{
	#region Events
	public UnityEvent Paused { get; protected set; }
	public UnityEvent Unpaused { get; protected set; }


	#endregion

	#region Fields
	[SerializeField] protected int panelTypeId = 0;
	[SerializeField] protected Vector2 _posOnGrid;
	[SerializeField] protected Combatant _owner; // is it the player? The enemy?

	[Header("Set programmatically based on id")]
	[SerializeField] PanelInfo panelInfo;

	GameController gameController;
	IEnumerator flashCoroutine = null;
	
	#endregion 
	
	#region Properties
	public BoxCollider boxCollider { get; protected set; }

	public bool isPaused { get; protected set; }

	#region Implemented through panelInfo
	public string panelName
	{
		get { return panelInfo.name; }
	}
	public int panelId 
	{
		get { return panelTypeId; }
	}
    public bool traversable 
	{
		get { return ((IBattlefieldPanel)panelInfo).traversable; }
		set { panelInfo.traversable = value; }
	}

    public Material material
	{
		get { return centerRenderer.material; }
		set 
		{ 
			//panelInfo.material = value; 
			//renderer.material = value; 
			centerRenderer.material = value;
		}
	}

    public PanelType type
	{
	 	get { return ((IBattlefieldPanel)panelInfo).type; }
	 	set { panelInfo.type = value; }
	}


    public PanelEffect effect
	{
		get { return panelInfo.effect; }
		set { panelInfo.effect = value; }
	}


	#endregion

	public GameObject frame { get; protected set; }	
	public GameObject center { get; protected set; }
	public Vector2 posOnGrid
	{
		get { return _posOnGrid; }
		protected set { _posOnGrid = value; }
	}

	public Combatant owner { get { return _owner; } }
	
	public Renderer centerRenderer { get; protected set; }
	// the frame is what this script should be attached to, the center is the 
	// child of the frame where the material is applied

	#endregion

	#region Methods

	#region Initialization

	protected override void Awake()
	{
		frame = this.gameObject;
		
		base.Awake();
		isPaused = 		false;
		Paused = 		new UnityEvent();
		Unpaused = 		new UnityEvent();
		boxCollider = 	GetComponent<BoxCollider>();
	}

    // Use this for initialization
    protected override void Start () 
	{
		base.Start();
		
		center = transform.Find("PanelCenter").gameObject;
		centerRenderer = center.GetComponent<Renderer>();

		GetPanelData();
		ApplyPanelData();

		gameController = GameController.instance;
		gameController.Paused.AddListener(Pause);
		gameController.Unpaused.AddListener(Unpause);
		
	}
	
	#endregion

	#region Update
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();

		// Because of the update execution order, this needs to be in Update for the healthbar 
		// to change color properly when an effect changes the player's HP. 
		if (!isPaused)
		{
			if (effect != null)
				effect.Execute();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		
	}

	#endregion

	/// <summary>
	/// For things like changing a normal panel to a broken one, etc.
	/// </summary>
	/// <param name="newPanel"></param>
	public void ChangeTo(PanelInfo newPanel)
	{
		if (this.panelInfo.Equals(newPanel))
			Debug.LogWarning(name + ": this panel is already a " + newPanel.name);
		else 
		{
			panelInfo = newPanel;
			ApplyPanelData();
		}
	}

	public void FlashMaterial(Material material, float duration = 1f, float interval = 0.25f)
	{
		if (flashCoroutine == null)
		{
			flashCoroutine = FlashMaterialCoroutine(material, duration, interval);
			StartCoroutine(flashCoroutine);
		}
		else 
		{
			StopCoroutine(flashCoroutine);
			flashCoroutine = FlashMaterialCoroutine(material, duration, interval);
			StartCoroutine(flashCoroutine);
		}
	}

	IEnumerator FlashMaterialCoroutine(Material material, float duration = 1f, float interval = 0.25f)
	{
		Material originalMaterial = this.material;

		this.material = material;

		float timer = duration;
		float intervalTimer = interval;

		while (timer > 0)
		{
			if (!isPaused)
			{
				// switch the panel's mat based on the interval timer
				intervalTimer -= Time.deltaTime;

				if (intervalTimer <= 0)
				{
					if (this.material == originalMaterial)
						this.material = material;
					else if (this.material = material)
						this.material = originalMaterial;

					intervalTimer = interval;
				}
				
				timer -= Time.deltaTime;
			}

			yield return null;
		}

		this.material = originalMaterial;
		flashCoroutine = null;
	}


	#region IPausable

	public void Pause()
	{
		isPaused = true;
	}

	public void Unpause()
	{
		isPaused = false;
	}


	#endregion

	#region Helper funcs

	void GetPanelData()
	{
		panelInfo = PanelDatabase.instance.GetPanel(panelTypeId);
	}

	void ApplyPanelData()
	{
		panelInfo.Init(this);

		/*
		if (panelInfo.name.Contains("Cra"))
			Debug.Log("Applying cracked panel data.");
			*/
			
		if (panelInfo.material != null)
			centerRenderer.material = panelInfo.material;
		else
			centerRenderer.material = renderer.material;
		

		centerRenderer.material.mainTexture = panelInfo.texture;
			
	}

	#endregion

	#endregion
	
}
