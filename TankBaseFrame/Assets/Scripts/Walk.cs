using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using TankProtocol;

public class Walk : MonoBehaviour {
    
    /// <summary>
    /// 游戏对象的预设
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// 玩家列表 players
    /// </summary>
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    /// <summary>
    /// 自身游戏角色
    /// </summary>
    string playerID = "";
    /// <summary>
    /// 上一次移动的时间
    /// </summary>
    public float lastMoveTime;
    /// <summary>
    /// 单例
    /// </summary>
    public static Walk instance;
    void Start()
    {
        instance = this;
    }

    /// <summary>
    /// 添加玩家列表
    /// </summary>
    /// <param name="id">玩家ip和端口</param>
    /// <param name="pos">游戏对象的位置</param>
    private void AddPlayer(string id, Vector3 pos,int score)
    {
        GameObject player = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();//3D Text
        textMesh.text = id+":"+score;
        players.Add(id,player);
    }
    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id"></param>
    private void DelPlayer(string id)
    {
        //已经初始化该玩家
        if(players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    /// <summary>
    /// 更新分数
    /// </summary>
    /// <param name="id"></param>
    /// <param name="score"></param>
    public void UpdateScore(string id,int score)
    {
        GameObject player = players[id];
        if (player == null)
            return;
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id + ":" + score;
    }
    /// <summary>
    /// 更新信息
    /// 自己的话 就更新分数 防止拉回现象
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="score"></param>
    public void UpdateInfo(string id,Vector3 pos,int score)
    {
        //只更新自己的分数
        if(id == playerID)
        {
            UpdateScore(id,score);
        }
        //其他人
        //已经初始化的玩家
        if(players.ContainsKey(id))
        {
            players[id].transform.position = pos;
            UpdateScore(id,score);
        }
            //尚未初始化的玩家
        else
        {
            AddPlayer(id,pos,score);
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="id"></param>
    public void StartGame(string id)
    {
        playerID = id;
        //产生自己
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
        float x = UnityEngine.Random.Range(-5, 5);
        float y = 0;
        float z = UnityEngine.Random.Range(-5, 5);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(playerID, pos, 0);
        //同步
        SendPos();
        //获取里列表
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList");
        NetMgr.srvConn.Send(proto, GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("PlayerLeave", PlayerLeave);
    }

    /// <summary>
    /// 发送位置
    /// </summary>
    void  SendPos()
    {
        GameObject player = players[playerID];
        Vector3 pos = player.transform.position;
        //消息
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        NetMgr.srvConn.Send(proto);
    }
    /// <summary>
    /// 更新玩家列表
    /// </summary>
    /// <param name="protocol"></param>
    public void GetList(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取头部数值
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        int count = proto.GetInt(start,ref start);
        //遍历
        for (int i = 0; i < count; i++)
        {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start,ref start);
            float y = proto.GetFloat(start,ref start);
            float z = proto.GetFloat(start, ref start);
            int score  = proto.GetInt(start,ref start);
            Vector3 pos = new Vector3(x,y,z);
            UpdateInfo(id, pos, score);
        }
    }
    
    /// <summary>
    /// 更新信息
    /// </summary>
    /// <param name="protocol"></param>
    public void UpdateInfo(ProtocolBase protocol)
    {
        //获取数值
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        string id = proto.GetString(start,ref start);
        float x = proto.GetFloat(start,ref start);
        float y = proto.GetFloat(start, ref start);
        float z = proto.GetFloat(start, ref start);
        int score = proto.GetInt(start, ref start);
        Vector3 pos = new Vector3(x, y, z);
        UpdateInfo(id, pos, score);
    }
    /// <summary>
    /// 玩家离开
    /// </summary>
    /// <param name="protocol"></param>
    public void PlayerLeave(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取数值
        int start = 0;
        string protoName = proto.GetString(start,ref start);
        string id = proto.GetString(start, ref start);
        DelPlayer(id);
    }
    /// <summary>
    /// 玩家移动控制
    /// </summary>
    void  Move()
    {
        if (playerID == "")
        {
            return;
        }
        if (players[playerID] == null)
            return;
        if (Time.time - lastMoveTime < 0.1)
            return;
        lastMoveTime = Time.time;

        GameObject player = players[playerID];
        //上下左右 空格
        if(Input.GetKey(KeyCode.UpArrow))
        {
            player.transform.position += new Vector3(0,0,1);
            SendPos();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            player.transform.position += new Vector3(0, 0,-1);
            SendPos();
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            player.transform.position += new Vector3(-1, 0, 0);
            SendPos();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            player.transform.position += new Vector3(1, 0, 0);
            SendPos();
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("AddScore");
            NetMgr.srvConn.Send(proto);
        }
    }

    void Update()
    {
        Move();
    }
}
