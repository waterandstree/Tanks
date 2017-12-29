using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TankProtocol;

public class LoginPanel : PanelBase {

    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;

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
    /// <summary>
    /// 登陆事件监听
    /// </summary>
    public void OnLoginClick()
    {
        //用户名和密码不能为空
        if(idInput.text == ""||pwInput.text == "")
        {
PanelMgr.instance.OpenPanel<TipPanel>("","用户密码不能为空");
            return;
        }
        //连接服务器
        if(NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if (!NetMgr.srvConn.Connect(host, port))
               PanelMgr.instance.OpenPanel<TipPanel>("","连接服务器失败");
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送"+protocol.GetDesc());
        NetMgr.srvConn.Send(protocol,OnLoginBack);
    }
    /// <summary>
    /// 实现登陆消息的监听事件
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
            PanelMgr.instance.OpenPanel<TipPanel>("","登入成功");
            //开始游戏
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            GameMgr.instance.id = idInput.text;
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","登陆失败，请检查用户名和密码");
        }
    }
    /// <summary>
    /// 注册按钮
    /// </summary>
    private void OnRegClick()
    {
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    
}
