using UnityEngine;
using System.Collections;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        Application.runInBackground = true;
        PanelMgr.instance.OpenPanel<ChoosePanel>("");
	}

    void Update()
    {
        NetMgr.Update();
    }
}
