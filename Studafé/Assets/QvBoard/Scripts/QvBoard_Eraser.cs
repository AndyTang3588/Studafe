using QvPen.UdonScript;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace yue
{
    /// <summary>
    /// QVBoardに消しゴムを当てると消し、内部のInkオブジェクトも消すクラス
    /// </summary>
    ///
    /// <remarks>
    ///　QVBoardは単純に削除し、Syncオブジェクトはプールに返す。
    /// InkObjectはQVPenのEraserの機能を利用して削除する。
    /// </remarks>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class QvBoard_Eraser : UdonSharpBehaviour
    {
        bool isErasing = false;
        private SphereCollider sphereCollider;
        private float eraserRadius = 0.0f;
        
        // EraserManager
        private QvBoard_EraserManager manager;

        private QvPen_Eraser eraser;
        
        
        public void _Init(QvBoard_EraserManager manager)
        {
            this.manager = manager;
            sphereCollider = GetComponent<SphereCollider>();
            eraserRadius = sphereCollider.radius;
            eraser = GetComponent<QvPen_Eraser>();
        }


        public override void OnPickupUseDown()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartErasing));
        }

        public override void OnPickupUseUp()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(FinishErasing));
        }
        public void StartErasing()
        {
            isErasing = true;
        }

        public void FinishErasing()
        {
            isErasing = false;
        }
        
        private readonly Collider[] results = new Collider[4];
        public override void PostLateUpdate()
        {
            if (isErasing && eraser.IsUser)
            {
                int count = Physics.OverlapSphereNonAlloc(transform.position, eraserRadius, results, -1,
                    QueryTriggerInteraction.Collide);
                for (int i = 0; i < count; i++)
                {
                    if(results[i] == null) continue;
                    QvBoard board = results[i].gameObject.GetComponent<QvBoard>();
                    Log("QVBoard_Eraser: Physics.OverlapSphereNonAlloc board: " + board);
                    if (board != null )
                    {
                        EraseSyncBoard(board.poolIndex);
                    }
                }
            }
        }
        
        private void EraseSyncBoard(int index)
        {
            Log("QvBoard_Eraser: EraseSyncBoard index: " + index);
            manager.objectPool.Return(index);
        }

        
        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xf2, 0xfd, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;
        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_Eraser)}{ColorEndTag}] ") : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion

    }
}