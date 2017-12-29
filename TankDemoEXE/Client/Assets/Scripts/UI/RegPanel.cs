using UnityEngine;
using System.Collections;
using TankProtocol;
using UnityEngine.UI;


public class RegPanel : PanelBase {

    private InputField idInput;
    private InputField pwInput;
    private InputField repInput;
    private Button regBtn;
    private Button closeBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RegPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();

        Transform skinTrans = skin.transform;
        idInput = skinTrans.FindChild("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.FindChild("PWInput").GetComponent<InputField>();
        repInput = skinTrans.FindChild("RepInput").GetComponent<InputField>();
        regBtn = skinTrans.FindChild("RegBtn").GetComponent<Button>();
        closeBtn = skinTrans.FindChild("CloseBtn").GetComponent<Button>();
        regBtn.onClick.AddListener(OnRegClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }

    private void OnRegClick()
    {
        //用户名和密码为空
        if (idInput.text == "" | pwInput.text == "")
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "用户名和密码不能为空！");
            return;
        }
        //两次的密码不同
        if(pwInput.text != repInput.text)
        {
             PanelMgr.instance.OpenPanel<TipPanel>("","两次输入的密码不同！");
            return;
        }
        //连接服务器
        if(NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if(!NetMgr.srvConn.Connect(host,port))
                PanelMgr.instance.OpenPanel<TipPanel>("","连接服务器失败");
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送"+protocol.GetDesc());
        NetMgr.srvConn.Send(protocol,OnRegBack);
    }
    public  void OnRegBack(ProtocolBase protocol)
    {
        ProtocolBytes proto =(ProtocolBytes)protocol;
        int start = 0;
        string name = proto.GetString(start,ref start);
        int ret = proto.GetInt(start,ref start);
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","注册成功");
            PanelMgr.instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","注册失败，请更换用户名");
        }
    }

    public void OnCloseClick()
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }
}
