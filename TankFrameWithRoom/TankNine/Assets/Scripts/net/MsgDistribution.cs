using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TankProtocol;

/// <summary>
/// 消息分配中心
/// 添加、删除、执行(收到消息的时候)
/// </summary>
public class MsgDistribution 
{
    /// <summary>
    /// 每帧处理消息的数量
    /// </summary>
    public int num = 15;
    /// <summary>
    /// 消息列表
    /// </summary>
    public List<ProtocolBase> msgList = new List<ProtocolBase>();
    /// <summary>
    /// 委托类型
    /// </summary>
    /// <param name="proto"></param>
    public delegate void Delegate(ProtocolBase proto);
    //事件监听表
    /// <summary>
    /// 事件监听表
    /// 注册一次终生执行
    /// </summary>
    private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();
    /// <summary>
    /// 事件监听表
    /// 注册一次执行一次
    /// </summary>
    private Dictionary<string, Delegate> onceDict = new Dictionary<string, Delegate>();

    public void Update()
    {
        for (int i = 0; i < num; i++)
        {
            if(msgList.Count > 0)
            {
                DispatchMsgEvent(msgList[0]);
                lock(msgList)
                {
                    msgList.RemoveAt(0);
                }
            }
            else
            {
                break;
            }
        }
    }
    /// <summary>
    /// 消息分发
    /// </summary>
    /// <param name="protocol"></param>
    public void DispatchMsgEvent(ProtocolBase protocol)
    {
        string name = protocol.GetName();
        Debug.Log("分发处理消息"+name);
        if(eventDict.ContainsKey(name))
        {
            eventDict[name](protocol);//将委托的参数
        }
        if(onceDict.ContainsKey(name))
        {
            onceDict[name](protocol);
            onceDict[name] = null;
            onceDict.Remove(name);
        }
    }
    /// <summary>
    /// 添加监听事件
    /// 注册一次永久使用
    /// </summary>
    /// <param name="name">协议的名字</param>
    /// <param name="cb">委托函数</param>
    public void AddListener(string name,Delegate cb)
    {
        if(eventDict.ContainsKey(name))
        {
            eventDict[name] += cb;//多个事件
        }
        else
        {
            eventDict[name] = cb;
        }
    }

    /// <summary>
    /// 添加单次监听事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cb"></param>
    public void AddOnceListener(string name,Delegate cb)
    {
        if (onceDict.ContainsKey(name))
        {
            onceDict[name] += cb;//多个事件
        }
        else
        {
            onceDict[name] = cb;
        }
    }
    /// <summary>
    /// 删除监听事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cb"></param>
    public void DelListener(string name,Delegate cb)
    {
        if(eventDict.ContainsKey(name))
        {
            eventDict[name] -= cb;
            if (eventDict[name] == null)
                eventDict.Remove(name);
        }
    }

    /// <summary>
    /// 删除单次监听事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cb"></param>
    public void DelOnceListener(string name,Delegate cb)
    {
        if(onceDict.ContainsKey(name))
        {
            onceDict[name] -= cb;
            if (onceDict[name] == null)
                onceDict.Remove(name);
        }
    }

}
