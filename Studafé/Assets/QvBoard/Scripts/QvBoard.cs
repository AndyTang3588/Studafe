using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem.HID;

namespace yue
{
    // QVPenでのLineRenderに相当するクラス
    [RequireComponent(typeof(ParentConstraint))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QvBoard : UdonSharpBehaviour
    {
        public bool UseVRCObjectSync = false;

        private Vector3 minPos;
        private Vector3 maxPos;

        [NonSerialized] public int poolIndex;

        private GameObject syncBoard;
        private QvBoard_SyncBoard syncBoardComponent;

        private Transform inkPool;

        private QvBoard_BoardMaker _boardMaker;

        private DataList connectedObjects = new DataList();

        private const float checkInterval = 1.0f;
        private float _elapsedTime = 0f;

        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= checkInterval)
            {
                _elapsedTime = 0f;
                CheckConnectedObjectAndDestroy();
            }
        }

        // リスト中のオブジェクトが削除済みかどうか確認して、削除されていればリストから除外
        private void CheckConnectedObjectAndDestroy()
        {
            if(!Networking.IsOwner(syncBoard)) return;
            
            // Log("QvPenBoard CheckConnectedObjectAndDestroy connectedObjects.Count: " + connectedObjects.Count);
            for (int i = 0; i < connectedObjects.Count; i++)
            {
                if (connectedObjects.TryGetValue(i, out DataToken value))
                {
                    Transform obj = (Transform)value.Reference;
                    if (obj != null)
                    {
                        return;
                    }
                }
            }
            if(_boardMaker != null)
            {
                Log("QvPenBoard CheckConnectedObjectAndDestroy AutoDelete this: " + this);
                Log("Networking.IsOwner(gameObject): " + Networking.IsOwner(gameObject));
                Log("connectedObjects.Count: " + connectedObjects.Count);
                _boardMaker.boardPool.Return(poolIndex);
                Destroy(this);
            }
        }

        public void CreateForOwner(Vector3 minPos, Vector3 maxPos, int poolIndex, Transform inkPool,
            QvBoard_BoardMaker boardMaker)
        {
            Log("QvPenBoard CreateForOwner");
            CommonInit(minPos, maxPos, poolIndex, inkPool, boardMaker);
            int lineRenderers = ConstraintLineRenderersWithCollider();
            if(lineRenderers == 0)
            {
                Warning("QvPenBoard CreateForOwner No LineRenderers found. Return");
                Destroy(this);
                return;
            }
            syncBoardComponent.Init(this);
            CreateConstraintWithSyncBoard();
        }

        public void CreateForSync(Vector3 minPos, Vector3 maxPos, int poolIndex, Transform inkPool,
            QvBoard_BoardMaker boardMaker,
            int minInkID, int maxInkID)
        {
            Log("QvPenBoard CreateForSync");
            CommonInit(minPos, maxPos, poolIndex, inkPool, boardMaker, true);
            int lineRenderers = ConstraintLineRenderersWithInkID(minInkID, maxInkID);
            if(lineRenderers == 0)
            {
                Warning("QvPenBoard CreateForSync No LineRenderers found. Return");
                Destroy(this);
                return;
            }
            syncBoardComponent.InitForSync(this);
            SendCustomEventDelayedSeconds(nameof(SyncWithSyncBoard), 1);
        }

        private void CommonInit(Vector3 minPos, Vector3 maxPos, int poolIndex, Transform inkPool,
            QvBoard_BoardMaker boardMaker, bool isSync = false)
        {
            this.minPos = minPos;
            this.maxPos = maxPos;
            this.poolIndex = poolIndex;
            this.inkPool = inkPool;
            this._boardMaker = boardMaker;

            syncBoard = _boardMaker.boardPool.GetObject(poolIndex, !isSync);
            syncBoardComponent = _boardMaker.boardPool.getSyncBoard(poolIndex);
            // transformの設定
            Vector3 size = new Vector3(maxPos.x - minPos.x, maxPos.y - minPos.y,
                maxPos.z - minPos.z);
            Vector3 center = new Vector3((maxPos.x + minPos.x) / 2, (maxPos.y + minPos.y) / 2,
                (maxPos.z + minPos.z) / 2);
            transform.localScale = size * 0.9f; // TODO PickUpと干渉しないようにするため少し小さくする
            transform.position = center;
        }

        public void EraseThis()
        {
            // !!!!!必ずQvboardObjectPoolから呼び出すこと!!!!!
            Log("QvPenBoard EraseThis connectedObjects.Count: " + connectedObjects.Count);
            for (int i = 0; i < connectedObjects.Count; i++)
            {
                if (connectedObjects.TryGetValue(i, out DataToken value))
                {
                    Transform obj = (Transform)value.Reference;
                    Log("QvPenBoard EraseThis obj: " + obj);
                    if (obj != null)
                    {
                        Destroy(obj.GetComponent<MeshCollider>().sharedMesh);
                        if (obj.parent != null)
                        {
                            Destroy(obj.parent.gameObject);
                        }
                    }
                }
            }

            Destroy(gameObject);
        }

        // 自身のTransformの中にあるLineRendererをConstraintで繋ぐ
        private int ConstraintLineRenderersWithCollider()
        {
            Log("QvPenBoard CreateConstraintWithLineRenderers");
            // TODO コライダー範囲のバッファを外から変えられるようにする
            Collider[] hitColliders =
                Physics.OverlapBox(transform.position, transform.localScale / 2 + Vector3.one / 10, transform.rotation);
            Log("QvPenBoard CreateConstraintWithLineRenderers hitColliders.Length: " + hitColliders.Length);
            int lineRendererCount = 0;
            foreach (Collider other in hitColliders)
            {
                if (other == null)
                {
                    Warning("QvPenBoard CreateConstraintWithLineRenderers othersLoop other is Null!!!)");
                    continue;
                }

                MeshCollider meshCollider = other.GetComponent<MeshCollider>();
                if (CheckAndSetConstraint(meshCollider)) lineRendererCount++;
            }
            
            Log("QvPenBoard CreateConstraintWithLineRenderers End lineRrenderCount: " + lineRendererCount);
            return lineRendererCount;
        }

        // 与えられたinkIdの範囲内のInkObjectのTransformの中にあるLineRendererをConstraintで繋ぐ
        private int ConstraintLineRenderersWithInkID(int minInkID, int maxInkID)
        {
            Log("QvPenBoard ConstraintLineRenderersWithInkID minInkID: " + minInkID + " maxInkID: " + maxInkID);
            Transform inkPoolSynced = inkPool.GetChild(0); // syncedである想定
            int lineRendererCount = 0;
            for (int i = 0; i < inkPoolSynced.childCount; i++)
            {
                if (i < minInkID || maxInkID < i) continue;
                Transform inkObject = inkPoolSynced.GetChild(i);
                MeshCollider
                    meshCollider = inkObject.GetChild(0).GetComponent<MeshCollider>(); // InkObjectの子にMeshColliderがある想定
                if (CheckAndSetConstraint(meshCollider)) lineRendererCount++;
            }

            Log("QvPenBoard ConstraintLineRenderersWithInkID End lineRrenderCount: " + lineRendererCount);
            return lineRendererCount;
        }

        private bool CheckAndSetConstraint(MeshCollider meshCollider)
        {
            Log("QvPenBoard CheckAndSetConstraint");
            if (meshCollider == null || meshCollider.sharedMesh == null)
            {
                Log("渡されたColliderはMeshColliderではないか、メッシュが設定されていません。");
                return false;
            }

            ParentConstraint parentConstraint = meshCollider.gameObject.GetComponent<ParentConstraint>();
            if (!CheckIsTargetInk(meshCollider, parentConstraint))
            {
                Debug.Log("QvPenBoard CheckAndSetConstraint CheckIsTargetInk Failed");
                return false;
            }

            SetConstraint(parentConstraint, transform, meshCollider.transform);
            connectedObjects.Add(meshCollider.transform);
            Log("QvPenBoard CheckAndSetConstraint ConstraintSet Success name:" + meshCollider.transform.parent.name);
            return true;
        }

        public void SyncWithSyncBoard()
        {
            Log("QvPenBoard SyncWithSyncBoard poolIndex: " + poolIndex);
            transform.position = syncBoard.transform.position;
            transform.rotation = syncBoard.transform.rotation;

            // VRCObjectSyncはスケールのみ動悸しないので自分側を正とする
            if (UseVRCObjectSync) syncBoard.transform.localScale = transform.localScale;

            CreateConstraintWithSyncBoard();
        }

        private void CreateConstraintWithSyncBoard()
        {
            Log("QvPenBoard CreateConstraintWithSyncBoard");
            ParentConstraint selfParentConstraint = GetComponent<ParentConstraint>();
            SetConstraint(selfParentConstraint, syncBoard.transform, null);
        }

        private void SetConstraint(ParentConstraint parentConstraint, Transform parentTransform,
            Transform transformForOffset)
        {
            Log("QvPenBoard SetConstraint parentConstraint: " + parentConstraint + " parentTransform: " +
                parentTransform + " transformForOffset: " + transformForOffset);

            // 親オブジェクトをソースとして設定
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = parentTransform; // 親オブジェクトをソースに設定
            source.weight = 1.0f; // 拘束の強さを100%に設定

            // ParentConstraintにソースを追加
            parentConstraint.AddSource(source);

            if (transformForOffset != null)
            {
                // 現在の位置と回転をオフセットとして保存
                Vector3 positionOffset = transformForOffset.position - parentTransform.position;
                Vector3 rotationOffset = transformForOffset.eulerAngles - parentTransform.eulerAngles;

                // オフセットを適用
                parentConstraint.translationOffsets = new Vector3[] { positionOffset };
                parentConstraint.rotationOffsets = new Vector3[] { rotationOffset };
            }

            // オフセットを維持するかどうかの設定
            parentConstraint.constraintActive = true;
            parentConstraint.locked = true;
            Log("QvPenBoard SetConstraint End");
        }

        private bool CheckIsTargetInk(MeshCollider meshCollider, ParentConstraint parentConstraint)
        {
            Log("QvPenBoard CheckIsTargetInk");
            GameObject target = meshCollider.gameObject;
            // InkObjectの親の親の親がPenと同じInkPoolであるか（別の色のインクを対象にしないようチェック）
            if (target.transform.parent.parent.parent != inkPool) return false;

            // Constraintが既に設定されている場合はスキップ
            if (parentConstraint.constraintActive) return false;
            return true;
        }

        public Vector3[] PackData()
        {
            Log("QvPenBoard PackData");
            Vector3[] data = new Vector3[3];
            data[0] = minPos;
            data[1] = maxPos;

            int[] minMaxInkID = GetMinMaxInkID();
            data[2] = new Vector3(poolIndex, minMaxInkID[0], minMaxInkID[1]);

            for (int i = 0; i < data.Length; i++)
            {
                Log($"QvPenBoard PackData data {i}: {data[i]}");
            }

            Log("QvPenBoard PackData End");
            return data;
        }

        private int[] GetMinMaxInkID()
        {
            Log("QvPenBoard getMinMaxInkID");
            if (connectedObjects.Count == 0) return new int[] { 0, 0 };

            int minInkID = int.MaxValue;
            int maxInkID = int.MinValue;
            for (int i = 0; i < connectedObjects.Count; i++)
            {
                if (connectedObjects.TryGetValue(i, out DataToken value))
                {
                    Transform obj = (Transform)value.Reference;
                    if (obj == null) continue;
                    
                    int objNum = obj.parent.GetSiblingIndex();
                    if (objNum < minInkID) minInkID = objNum;
                    if (maxInkID < objNum) maxInkID = objNum;
                }
            }

            Log("QvPenBoard getMinMaxInkID End minInkID: " + minInkID + " maxInkID: " + maxInkID);
            return new int[] { minInkID, maxInkID };
        }

        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }

        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0x12, 0xfd, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;

        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard)}{ColorEndTag}] ")
                : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}