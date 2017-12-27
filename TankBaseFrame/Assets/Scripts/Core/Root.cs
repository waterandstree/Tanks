using UnityEngine;
using System.Collections;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //后台运行
        Application.runInBackground = true;
        //界面分离
        PanelMgr.instance.OpenPanel<LoginPanel>("");
	}

    void Update()
    {
        NetMgr.Update();
    }
	
}
