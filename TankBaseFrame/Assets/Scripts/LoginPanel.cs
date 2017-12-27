using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TankProtocol;

public class LoginPanel : PanelBase
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;

    #region 生命周期
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.FindChild("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.FindChild("PWInput").GetComponent<InputField>();
        loginBtn = skinTrans.FindChild("LoginBtn").GetComponent<Button>();
        regBtn = skinTrans.FindChild("RegBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }
    #endregion
    /// <summary>
    /// 登入事件监听
    /// </summary>
    public void OnLoginClick()
    {
        //用户名和密码不能为空
        if(idInput.text == "" | pwInput.text == "")
        {
            Debug.Log("用户名和密码部位空");
            return;
        }
        //尚未连接，先连接
        if(NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host,port);
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送 "+protocol.GetDesc());
        NetMgr.srvConn.Send(protocol,OnLoginBack);
    }
    /// <summary>
    /// 实现登入消息的监听事件
    /// </summary>
    /// <param name="protocol"></param>
    public void OnLoginBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        int ret = proto.GetInt(start,ref start);
        if(ret == 0)
        {
            Debug.Log("登入成功！");
            //开始游戏
            Walk.instance.StartGame(idInput.text);
            Close();
        }
        else
        {
            Debug.Log("登录失败！");
        }
    }


    /// <summary>
    /// 注册按钮事件监听
    /// </summary>
    public void OnRegClick()
    {
        PanelMgr.instance.OpenPanel<RegPanel>("");
		Close();
    }
}
