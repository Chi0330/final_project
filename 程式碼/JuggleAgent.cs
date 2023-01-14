using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class JuggleAgent : Agent
{
    public Rigidbody ball; //定義球體
    Rigidbody player; //定義球員
    public float speed = 30.0f; 
    public TextMeshPro display;
    float diff = 0.0f; // 球體當前的y軸與上一步的的差異
    float previousDiff = 0.0f; // 保留了上一个diff
    float previousY = 5.0f; // 球體上一步的y軸的值
    bool collied = false; //是否碰撞

    private void OnCollisionEnter(Collision collision) //碰撞檢測
    {
        if(collision.rigidbody == ball)
        {
            collied = true;
        }
    }

    public override void InitializeAgent() //操控球員
    {
        player = this.GetComponent<Rigidbody>(); 
    }

    public override void CollectObservations() //偵測遊戲資訊
    {
        AddVectorObs(ball.transform.localPosition); //位置 3d
        AddVectorObs(ball.velocity); //速度 3d
        AddVectorObs(ball.rotation); //角度 4d
        AddVectorObs(ball.angularVelocity); //角速度 3d

        AddVectorObs(player.transform.localPosition);
        AddVectorObs(player.velocity);
        AddVectorObs(player.rotation);
        AddVectorObs(player.angularVelocity);
    }

    public override void AgentAction(float[] vectorAction) //控制信號
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        if(player.transform.localPosition.y == 1.0f) //只能在地面起跳
        {
            controlSignal.y = vectorAction[2] * 10.0f; //瞬間施力
        }

        player.AddForce(controlSignal * speed); //放大控制信號

        diff = ball.transform.localPosition.y - previousY; //球體Y軸變化量
        if(diff > 0.0f && previousDiff < 0.0f && collied)  //球體發生碰撞 reward + 0.1
        {
            AddReward(0.1f);
        }
        collied = false; //初始碰撞變數
        previousDiff = diff; //紀錄當前的 球體Y軸變化量
        previousY = ball.transform.localPosition.y; //紀錄當前球體Y值

        if(ball.transform.localPosition.y < 1.5f || Mathf.Abs(player.transform.localPosition.x) > 10.0f || Mathf.Abs(player.transform.localPosition.z) > 10.0f) //判斷遊戲是否結束
        {
            Done();
        }
        display.text = GetCumulativeReward().ToString("N1"); //顯示reward總分
    }

    public override void AgentReset() //重置遊戲環境
    {
        ball.transform.localPosition = new Vector3(Random.value * 10 - 5, 5.0f, Random.value * 10 - 5); //初始球體位置
        ball.velocity = Vector3.zero; //球體速度改為0
        ball.rotation = Quaternion.Euler(Vector3.zero); //球體角度改為0
        ball.angularVelocity = Vector3.zero; //球體角速度改為0

        player.transform.localPosition = Vector3.up;
        player.velocity = Vector3.zero;
        player.rotation = Quaternion.Euler(Vector3.zero);
        player.angularVelocity = Vector3.zero;

        diff = 0.0f; //重置各項變數
        previousDiff = 0.0f;
        previousY = 5.0f;
        collied = false;
    }

    public override float[] Heuristic() //手動操作
    {
        float[] vectorAction = new float[3];
        vectorAction[0] = Input.GetAxis("Horizontal");
        vectorAction[1] = Input.GetAxis("Vertical");
        vectorAction[2] = Input.GetAxis("Jump");
        return vectorAction;
    }
}
