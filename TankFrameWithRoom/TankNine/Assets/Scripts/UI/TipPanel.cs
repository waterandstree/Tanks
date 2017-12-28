using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TipPanel : PanelBase {

    private Text text;
    private Button btn;
    string str = "";

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "TipPanel";
        layer = PanelLayer.Tips;
        //参数
        if(args.Length == 1)
        {
            str = (string)args[0];
        }
    }
    /// <summary>
    /// 提示面板，显示之前
    /// </summary>
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //文字
        text = skinTrans.FindChild("Text").GetComponent<Text>();
        text.text = str;
        //关闭按钮
        btn = skinTrans.FindChild("Btn").GetComponent<Button>();
        btn.onClick.AddListener(OnBtnClick);
    } 
    /// <summary>
    /// 关闭按钮
    /// </summary>
    public void OnBtnClick()
    {
        Close();
    }
}
