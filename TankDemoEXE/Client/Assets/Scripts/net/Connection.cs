using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using TankProtocol;

/// <summary>
/// 连接类
/// 关联到消息分发中心，收到消息时候调用消息分发中心的Update
/// </summary>
public class Connection 
{

    /// <summary>
    /// 常量---用在读取字节流
    /// const
    /// </summary>
    const int BUFFER_SIZE = 1024;
    /// <summary>
    /// 异步套接字
    /// </summary>
    private Socket socket;
    /// <summary>
    /// 读取缓冲区
    /// </summary>
    private byte[] readBuff = new byte[BUFFER_SIZE];
    /// <summary>
    /// 读取当前缓冲区的长度
    /// </summary>
    private int buffCount = 0;
    /// <summary>
    /// 粘包和分包--消息长度
    /// </summary>
    private Int32 msgLength = 0;
    /// <summary>
    /// 粘包和分包--消息内容
    /// </summary>
    private byte[] lenBytes = new byte[sizeof(Int32)];
    /// <summary>
    /// 协议
    /// </summary>
    public ProtocolBase proto;
    /// <summary>
    /// 上一次发送心跳信号的时间
    /// </summary>
    public float lastTrickTime = 0;
    /// <summary>
    /// 心跳发送的间隔
    /// </summary>
    public float heartBeatTime = 30;
    /// <summary>
    /// 消息分发实例
    /// </summary>
    public MsgDistribution msgDist = new MsgDistribution();
    /// <summary>
    /// 连接的状态枚举类(Status)
    /// </summary>
    public enum Status
    {
        None,
        Connected,
    };
    /// <summary>
    /// 默认连接的状态
    /// </summary>
    public Status status = Status.None;
    /// <summary>
    /// 连接服务端
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool Connect(string host,int port)
    {
        try
        {
            //socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //connect
            socket.Connect(host,port);
            //BeginReceive
            socket.BeginReceive(readBuff,buffCount,BUFFER_SIZE-buffCount,SocketFlags.None,ReceiveCb,readBuff);
            Debug.Log("连接成功");
            //Status Change
            status = Status.Connected;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("连接失败:"+e.Message);
            return false;
        }
    }
    /// <summary>
    /// 异步接受的回调事件
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar);
            buffCount = buffCount + count;
            ProcessData();
            socket.BeginReceive(readBuff, buffCount, 
			BUFFER_SIZE - buffCount, SocketFlags.None, 
			ReceiveCb, readBuff);
        }
        catch (Exception e)
        {
            Debug.Log("ReceiveCb失败"+e.Message);
            status = Status.None;
        }
    }
    /// <summary>
    /// 消息处理
    /// </summary>
    public void  ProcessData()
    {
        //粘包和分包处理
        if(buffCount < sizeof(Int32))
        {
            return;
        }
        //包体的长度
        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes,0);
        if(buffCount < msgLength + sizeof(Int32))
        {
            return;
        }
        //协议解码
        ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
        Debug.Log("收到的消息" + protocol.GetDesc());
        lock(msgDist.msgList)
        {
            msgDist.msgList.Add(protocol);
        }
        //清除已经处理的消息
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);//bug  readBuff
        buffCount = count;
        if(buffCount > 0)
        {
            ProcessData();
        }
    }
    /// <summary>
    /// 发送消息
    /// 不添加实现消息的监听
    /// </summary>
    /// <param name="protocol"></param>
    /// <returns></returns>
    public bool Send(ProtocolBase protocol)
    {
        if(status != Status.Connected)
        {
            Debug.LogError("[Connection]还没有客户端连接就发送数据是不好的");
            return false;//bug
        }
        //协议处理
        byte[] b = protocol.Ecode();
        //粘包处理
        byte[] length = BitConverter.GetBytes(b.Length);    //GetBytes
        byte[] sendbuff = length.Concat(b).ToArray();
        socket.Send(sendbuff);
        Debug.Log("发送消息"+protocol.GetDesc());
        return true;
    }
    /// <summary>
    /// 发送消息
    /// 带上实现消息的监听
    /// </summary>
    /// <param name="protocol">协议</param>
    /// <param name="cbName">协议的名字</param>
    /// <param name="cb">消息分发的实现消息的监听的回调函数</param>
    /// <returns></returns>
    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb)
    {
        if (status != Status.Connected)
        {
            return false;
        }
        msgDist.AddOnceListener(cbName,cb);
        return Send(protocol);
    }
    /// <summary>
    /// 发送消息
    /// 带上实现消息的监听
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="cb"></param>
    /// <returns></returns>
    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb)
    {
        string cbName = protocol.GetName();
        return Send(protocol,cbName,cb);
    }
    /// <summary>
    /// 没有继承MonoBehaviour
    /// </summary>
	public void Update () {
	    //消息
        msgDist.Update();
        //心跳
        if(status == Status.Connected)
        {
            if(Time.time - lastTrickTime > heartBeatTime)
            {
                ProtocolBase protocol = NetMgr.GetHeatBeatProtocol();
                Send(protocol);//不监听的心跳
                lastTrickTime = Time.time;
            }
        }
	}

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <returns></returns>
    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("关闭失败" + e.Message);
            return false;

        }
    }
}
