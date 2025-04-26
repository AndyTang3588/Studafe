using System;
using QvPen.UdonScript;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.Udon.Common.Interfaces;

namespace yue
{
    // QVPenでのPenに相当するクラス
    [RequireComponent(typeof(QvPen_Pen))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class QvBoard_BoardMaker : UdonSharpBehaviour
    {
        public GameObject QvPenBoardPrefab;

        private QvBoard_BoardManager manager;
        
        public GameObject inkPosition;
        public QvPen_LateSync inkPool;
        
        [NonSerialized]
        public QvBoard_ObjectPool boardPool;

        private bool isDrawing = false;
        private bool startDrawing = false;

        private Vector3 tempMinPos;
        private Vector3 tempMaxPos;


        //Sync用IDデータ
        private int boardId;
        public Vector3 boardMakerIdVector { get; private set; }

        // [System.NonSerialized]
        // TODO boardPoolのNotSynced版を使うタイミングがあるかは要調査 
        // →データを途中まで読み込むようなケース（同期時点で全てのデータが揃っていないケース）が存在するか？
        // public Transform boardPool;
        // [System.NonSerialized]
        public Transform boardPoolSynced;
        // public Transform boardPoolNotSynced;

        public QvBoard_LateSync syncer;

        public void _Init(QvBoard_BoardManager manager, QvBoard_ObjectPool boardPool)
        {
            this.manager = manager;
            this.boardPool = boardPool;
            boardId = string.IsNullOrEmpty(Networking.GetUniqueName(gameObject))
                ? 0
                : Networking.GetUniqueName(gameObject).GetHashCode();
            boardMakerIdVector = new Vector3((boardId >> 24) & 0x00ff, (boardId >> 12) & 0x0fff, boardId & 0x0fff);
            syncer.boardMaker = this;
            syncer.pen = GetComponent<QvPen_Pen>();
            syncer.Initialized = true;
        }

        void Update()
        {
            if (!isDrawing)
            {
                return;
            }


            Vector3 currentPosition = inkPosition.transform.position;

            // X軸の上限下限を更新
            if (currentPosition.x < tempMinPos.x)
            {
                tempMinPos.x = currentPosition.x;
            }

            if (currentPosition.x > tempMaxPos.x)
            {
                tempMaxPos.x = currentPosition.x;
            }

            // Y軸の上限下限を更新
            if (currentPosition.y < tempMinPos.y)
            {
                tempMinPos.y = currentPosition.y;
            }

            if (currentPosition.y > tempMaxPos.y)
            {
                tempMaxPos.y = currentPosition.y;
            }

            // Z軸の上限下限を更新
            if (currentPosition.z < tempMinPos.z)
            {
                tempMinPos.z = currentPosition.z;
            }

            if (currentPosition.z > tempMaxPos.z)
            {
                tempMaxPos.z = currentPosition.z;
            }
        }

        public override void OnPickup()
        {
            manager._TakeOwnership();
        }

        public override void OnPickupUseDown()
        {
            if (!startDrawing)
            {
                startDrawing = true;
                // 初期化
                tempMinPos = inkPosition.transform.position;
                tempMaxPos = inkPosition.transform.position;
            }

            if (!isDrawing)
            {
                isDrawing = true;
            }
        }

        public override void OnPickupUseUp()
        {
            isDrawing = false;
        }

        public override void OnDrop()
        {
            if (startDrawing)
            {
                QvBoard qvBoard = CreateQvBoard(tempMinPos, tempMaxPos);
                isDrawing = false;
                startDrawing = false;
                InitMinMax();

                if (qvBoard != null)
                {
                    Vector3[] data = _PackData(qvBoard);
                    manager._SendData(data);
                }
            }
        }

        private void InitMinMax()
        {
            tempMinPos = Vector3.zero;
            tempMaxPos = Vector3.zero;
        }


        private QvBoard CreateQvBoard(Vector3 inputMinPos, Vector3 inputMaxPos)
        {
            Log("CreateQvBoard");

            // ボードの作成者は、自分でSyncBoardを調達し設定を行う。
            int index = boardPool.TryToSpawn();
            if (index == -1) return null;

            GameObject qvBoard = Instantiate(QvPenBoardPrefab);
            qvBoard.transform.SetParent(boardPoolSynced.transform);

            // QvBoardComponentに値を設定
            QvBoard qvBoardCompornent = qvBoard.GetComponent<QvBoard>();
            qvBoardCompornent.CreateForOwner(inputMinPos, inputMaxPos, index, inkPool.transform, this);
     
            return qvBoardCompornent;
        }

        private void CreateQvBoardInSync(Vector3 inputMinPos, Vector3 inputMaxPos, int poolIndex,
            int minInkID,
            int maxInkID)
        {
            Log("CreateQvBoardInSync");
            GameObject qvBoard = Instantiate(QvPenBoardPrefab);
            qvBoard.transform.SetParent(boardPoolSynced.transform);
            
            // QvBoardComponentに値を設定
            QvBoard qvBoardCompornent = qvBoard.GetComponent<QvBoard>();
            qvBoardCompornent.CreateForSync(inputMinPos, inputMaxPos, poolIndex, inkPool.transform, this, minInkID,
                maxInkID);
        }


        //Sync用関数
        public Vector3[] _PackData(QvBoard board)
        {
            Log("QvPenBoardMaker _PackData");
            if (!board || !transform)
                return null;

            return board.PackData();
        }

        public void _UnpackData(Vector3[] data)
        {
            if (Networking.IsOwner(gameObject)) return;
            Log("QvPenBoardMaker _UnPackData");

            if (data.Length != 3)
            {
                Error("QvPenBoardMaker UnPackData Invalid data length" + data.Length);
                return;
            }

            // TODO HOTFIX inkIDはQVPenから取る
            CreateQvBoardInSync(data[0], data[1], (int)data[2].x, (int)data[2].y, (int)data[2].z);
        }


        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xb2, 0xfd, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;

        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_BoardMaker)}{ColorEndTag}] ")
                : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}