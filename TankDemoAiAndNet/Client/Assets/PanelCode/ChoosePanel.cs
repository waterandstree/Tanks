using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoosePanel : PanelBase
{
    public static int type;
    private Button aiFight;
    private Button netFight;
    private Button infoBtn;
    private Button closeBtn;

    #region 生命周期
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "ChoosePanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        aiFight = skinTrans.FindChild("AI").GetComponent<Button>();
        netFight = skinTrans.FindChild("Net").GetComponent<Button>();
        infoBtn = skinTrans.FindChild("infoBtn").GetComponent<Button>();
        closeBtn = skinTrans.FindChild("CloseBtn").GetComponent<Button>();
        //添加事件
        aiFight.onClick.AddListener(OnAiClick);
        netFight.onClick.AddListener(OnNetClick);
        infoBtn.onClick.AddListener(OnInfoClick);
        closeBtn.onClick.AddListener(OnEndGame);
    }
    #endregion


    public void OnAiClick()
    {
        type = 0;
        //设置
        PanelMgr.instance.OpenPanel<OptionPanel>("");
        PanelMgr.instance.ClearPanels();
    }

    public void OnInfoClick()
    {
        PanelMgr.instance.OpenPanel<InfoPanel>("");
    }

    public void OnNetClick()
    {
        type = 1;
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }
    public void OnEndGame()
    {
        Application.Quit();
    }
}
