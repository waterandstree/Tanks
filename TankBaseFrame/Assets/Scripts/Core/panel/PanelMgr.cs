using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class PanelMgr : MonoBehaviour
{
    //单利
    public static PanelMgr instance;
    //画板
    private GameObject canvas;
    //面板
    public Dictionary<string,PanelBase> dict;
    //层级
    private Dictionary<PanelLayer, Transform> layerDict;
    //开始
    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();

    }

    //初始化层
    private void InitLayer()
    {
        //画布
        canvas = GameObject.Find("Canvas");
        if(canvas == null)
        {
            Debug.LogError("panelMgr.InitLayer fail,canvas is null");
        }
        //各个层级
        layerDict = new Dictionary<PanelLayer, Transform>();
        foreach(PanelLayer p1 in Enum.GetValues(typeof(PanelLayer)))
        {
            string name = p1.ToString();
            Transform transform = canvas.transform.FindChild(name);
            layerDict.Add(p1,transform);
        }
    }

    public void OpenPanel<T>(string skinPath,params object[] args) where T:PanelBase
    {
        //已经打开
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name))
            return;
        //面板脚本
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name,panel);
        //加载皮肤
        skinPath = (skinPath != "" ? skinPath:panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath);
        if (skin == null)
            Debug.LogError("pannelMgr.OpenPanel fail ,skin is null ,skinPath = "+skinPath);
        panel.skin = (GameObject)Instantiate(skin);
        //坐标
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent,false);
        //panel 的生命周期
        panel.OnShowing();
        //anm
        panel.OnShowed();
    }
    //关闭面板
    public void ClosePanel(string name)
    {
        PanelBase panel = (PanelBase)dict[name];
        if (panel == null)
            return;
        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();
        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }

}

 /// <summary>
/// 分层类型
 /// </summary>
public enum PanelLayer
{
    //面板
    Panel,
    //提示
    Tips
}