using UnityEngine;
using System.Collections;

public class GameMgr : MonoBehaviour {

    public static GameMgr instance;
    public string id = "Tank";

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
