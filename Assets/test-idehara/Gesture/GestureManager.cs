using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureManager : MonoBehaviour
{
    public enum GestureType { STOP, SLOW, GO, NULL };

    // 認識結果をインスペクタ上で↓この変数をチェックしながらコーディング
    public GestureType currentGesture;
    public float duration;
    public StickRecognizer sr;

    // Start is called before the first frame update
    void Start()
    {
        currentGesture = GestureType.NULL;
        duration = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // StickRecognizer である sr に、棒の位置や動きを問い合わせ、
        // 関節情報などを加味して、ジェスチャを認識する。
        // ジェスチャを認識したら、currentGesture と duration を更新する
        // 認識できなければ、currentGesture は NULL に。
        // 新しいジェスチャが来たら、duration を 0 でリセット
        duration += Time.deltaTime;
    }
}
