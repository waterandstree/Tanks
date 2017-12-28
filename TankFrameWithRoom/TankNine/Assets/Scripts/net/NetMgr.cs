using UnityEngine;
using System.Collections;
using TankProtocol;

/// <summary>
/// 网络管理
/// </summary>
public class NetMgr 
{

    public static Connection srvConn = new Connection();
    //新建平台连接

    public static void  Update () 
    {
        srvConn.Update();
        //平台更新
	}
    /// <summary>
    /// 心跳
    /// </summary>
    /// <returns></returns>
    public static ProtocolBase GetHeatBeatProtocol()
    {
        //具体的发送内容根据服务器设定进行改动
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeatBeat");
        return protocol;
    }
}
