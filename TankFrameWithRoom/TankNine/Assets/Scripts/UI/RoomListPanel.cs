using UnityEngine;
using System.Collections;
using TankProtocol;
using UnityEngine.UI;

public class RoomListPanel : PanelBase {

    private Text idText;
    private Text winText;
    private Text lostText;
    private Transform content;
    private Transform viewPort;
    private GameObject roomPrefab;
    private Button closeBtn;
    private Button reflashBtn;
    private Button newBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomListPanel";
        layer = PanelLayer.Panel;
    }
    
    public override void OnShowing()
    {
        base.OnShowing();
        //获取Transform
        Transform skinTrans = skin.transform;
        Transform listTrans = skinTrans.FindChild("ListImage");
        Transform winTrans = skinTrans.FindChild("WinImage");
        //获取成绩栏部件
        idText = winTrans.FindChild("IDText").GetComponent<Text>();
        winText = winTrans.FindChild("WinText").GetComponent<Text>();
        lostText = winTrans.FindChild("LostText").GetComponent<Text>();
        //获取列表栏的部件
        Transform scrollRect = listTrans.FindChild("ScrollRect");
        content = scrollRect.FindChild("Content");
        roomPrefab = content.FindChild("RoomPrefab").gameObject;
        roomPrefab.SetActive(false);

        closeBtn = listTrans.FindChild("CloseBtn").GetComponent<Button>();
        newBtn = listTrans.FindChild("NewBtn").GetComponent<Button>();
        reflashBtn = listTrans.FindChild("ReflashBtn").GetComponent<Button>();
        //按钮事件
        reflashBtn.onClick.AddListener(OnReflashClick);
        newBtn.onClick.AddListener(OnNewClick);
        closeBtn.onClick.AddListener(OnCloseClick);
        //监听
        NetMgr.srvConn.msgDist.AddListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.AddListener("GetRoomList", RecvGetRoomList);//bu
        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);

        protocol = new ProtocolBytes();
        protocol.AddString("GetAchieve");
        NetMgr.srvConn.Send(protocol);
    }
    /// <summary>
    /// 界面关闭的时候需关闭全程监听
    /// </summary>
    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);//bu
    }
    /// <summary>
    /// 获取成绩回调
    /// </summary>
    /// <param name="protocol"></param>
    public void RecvGetAchieve(ProtocolBase protocol)
    {
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int win = proto.GetInt(start,ref start);
        int lost = proto.GetInt(start,ref start);
        //处理
        idText.text = "指挥官:" + GameMgr.instance.id;//这里注意id
        winText.text = win.ToString();
        lostText.text = lost.ToString();
    }
   /// <summary>
   /// 获取房间列表回调
   /// </summary>
   /// <param name="protocol"></param>
    public void RecvGetRoomList(ProtocolBase protocol)
    {
        //清理
        ClearRoomUnit();
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        int count = proto.GetInt(start,ref start);//房间数
        for (int i = 0; i < count; i++)
        {
            //房间人数
            int num = proto.GetInt(start,ref start);
            int status = proto.GetInt(start,ref start);
            GenerateRoomUnit(i,num,status);
        }
    }
    /// <summary>
    /// 清理房间列表
    /// </summary>
    public void ClearRoomUnit()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            if(content.GetChild(i).name.Contains("Clone"))
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }
    /// <summary>
    /// 创建一个房间单元
    /// 刷新房间列表
    /// </summary>
    /// <param name="i">房间序号(从0开始)</param>
    /// <param name="num">房间里的玩家数</param>
    /// <param name="status">房间状态</param>
    public void GenerateRoomUnit(int i,int num,int status)
    {
        //添加房间
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0,(i+1)*110);
        GameObject o = Instantiate(roomPrefab);
        o.transform.SetParent(content);
        o.SetActive(true);
        //房间信息
        Transform trans = o.transform;
        Text nameText = trans.FindChild("nameText").GetComponent<Text>();
        Text countText = trans.FindChild("CountText").GetComponent<Text>();
        Text statusText = trans.FindChild("StatusText").GetComponent<Text>();

        nameText.text = "序号:" + (i + 1).ToString();
        countText.text = "人数:" + num.ToString();
        if(status == 1)
        {
            statusText.color = Color.black;
            statusText.text = "状态:准备中";
        }
        else
        {
            statusText.color = Color.red;
            statusText.text = "状态:开战中";
        }
        //按钮事件
        Button btn = trans.FindChild("JoinButton").GetComponent<Button>();
        btn.name = i.ToString();//改变按钮的名字，以便给OnJoinBtnClick传参
        btn.onClick.AddListener(delegate() {
            OnJoinBtnClick(btn.name);
        });
    }
    /// <summary>
    /// 刷新按钮
    /// </summary>
    public void OnReflashClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);
    }
    /// <summary>
    /// 加入按钮
    /// </summary>
    /// <param name="name"></param>
    public void OnJoinBtnClick(string name)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");

        protocol.AddInt(int.Parse(name));
        NetMgr.srvConn.Send(protocol,OnJoinBtnBack);
        Debug.Log("请求进入房间"+name);
    }
    /// <summary>
    /// 加入房间返回事件
    /// </summary>
    /// <param name="protocol"></param>
    public void OnJoinBtnBack(ProtocolBase protocol)
    {
        //解析参数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string proName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start,ref start);
        //处理
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","成功进入房间!");
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","进入房间失败");
        }
    }
    /// <summary>
    /// 新建房间
    /// </summary>
    public void OnNewClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        NetMgr.srvConn.Send(protocol,OnNewBack);
    }
    /// <summary>
    /// 新建房间回调
    /// </summary>
    public void OnNewBack(ProtocolBase protocol)
    {
        //解析参数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        int ret = proto.GetInt(start,ref start);
        //处理
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","创建成功!");
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","创建房间失败!");
        }
    }
    /// <summary>
    /// 登出按钮
    /// </summary>
    public void OnCloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        NetMgr.srvConn.Send(protocol,OnCloseBack);
    }
    /// <summary>
    /// 登出返回
    /// </summary>
    /// <param name="protocol"></param>
    public void OnCloseBack(ProtocolBase protocol)
    {
        PanelMgr.instance.OpenPanel<TipPanel>("","登出成功！");
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        NetMgr.srvConn.Close();
    }
}
