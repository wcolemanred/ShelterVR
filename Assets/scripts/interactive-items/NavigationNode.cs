using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GvrAudioSource))]
public class NavigationNode : MonoBehaviour {

	private enum NodeState
	{
		Idle = 0,
		Hovered	= 1,
		Selected = 2
	}

	[Header("Audio clips")]
	public AudioClip nodeHoveredAudio;
	public AudioClip nodeClickAudio;
	[Header("Materials")]
	public Material nodeIdleMaterial;
	public Material nodeHoveredMaterial;
	public Material nodeSelectedMaterial;

	private NodeState nodeState = NodeState.Idle;
	private GvrAudioSource audioSource;

	private static NavigationNode currentNode;


	void Start () {
		audioSource = GetComponent<GvrAudioSource>();
	}

	public void onNavigateToNode()
	{
		// Move player
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		MobileElement mobilePlayer = player.GetComponent<MobileElement>();
		mobilePlayer.setDesination(transform.position);
		StartCoroutine(mobilePlayer.speedModifierCoroutine(0, 2, 2));
		// Toggle node
		playAudio(nodeClickAudio);
		StartCoroutine(toggleCurrentNode());
	}

	private IEnumerator toggleCurrentNode()
	{
		setNodeState(NodeState.Selected);
		GetComponent<BoxCollider>().enabled = false;
		yield return new WaitForSeconds(1f);
		setNodeState(NodeState.Idle);
		currentNode.GetComponent<BoxCollider>().enabled = true;
		currentNode = this;
	}

	public static void initCurrentNode(NavigationNode node)
	{
		currentNode = node;
		currentNode.GetComponent<BoxCollider>().enabled = false;
	}

	public void onHoverStart()
	{
		setNodeState(NodeState.Hovered);
		playAudio(nodeHoveredAudio);
	}

	public void onHoverEnd()
	{
		if (nodeState != NodeState.Selected)
			setNodeState(NodeState.Idle);
	}

	private void setNodeState(NodeState newState)
	{
		switch (newState)
		{
			case NodeState.Idle:		GetComponent<Renderer>().material = nodeIdleMaterial;		break;
			case NodeState.Hovered:		GetComponent<Renderer>().material = nodeHoveredMaterial;	break;
			case NodeState.Selected:	GetComponent<Renderer>().material = nodeSelectedMaterial;	break;
		}
		nodeState = newState;
	}

	private void playAudio(AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.Play();
	}
}
