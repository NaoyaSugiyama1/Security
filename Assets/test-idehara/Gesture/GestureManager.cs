using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureManager : MonoBehaviour
{
    public enum GestureType { STOP, SLOW, GO, NULL };

    // 認識結果をインスペクタ上で↓この変数をチェックしながらコーディング
    public GestureType currentGesture;
    public float duration;

    // Start is called before the first frame update
    void Start()
    {
        currentGesture = GestureType.NULL;
        duration = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // ジェスチャを認識して、currentGesture と duration を更新する
        // 新しいジェスチャが来たら、duration を 0 でリセット
        duration += Time.deltaTime;
    }
}
