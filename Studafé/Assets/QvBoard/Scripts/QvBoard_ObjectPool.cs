using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace yue
{
    // TODO HOTFIX VRCObjectPoolを使う予定だったが、不具合が多いので自作に変更
    // [RequireComponent(typeof(VRCObjectPool))]

    // 設計思想 _isUsedを変更したい場合はそのひとがオーナーになる。オブジェクトのオーナーはオブジェクトを渡す戻す際の実行者
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvBoard_ObjectPool : UdonSharpBehaviour
    {
        
        private GameObject[] _pool;
        private QvBoard_SyncBoard[] _syncBoards;
        private MeshRenderer[] _meshRenderers;

        [UdonSynced] private bool[] _isUsed = new bool[1];
        private bool[] oldIsUsed = new bool[1];
        private bool[] isUsed
        {
            get => _isUsed;
            set => SetIsUsed(value);
        }

        private void SetIsUsed(int index, bool value)
        {
            bool[] tmpBool = (bool[])_isUsed.Clone();
            tmpBool[index] = value;
            SetIsUsed(tmpBool);
        }

        private void SetIsUsed(bool[] value)
        {
            Log("Set isUsed");
            if (!isNetworkSettled)
            {
                Log("Network is not settled");
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                Log($"Set isUsed value {i}: {value[i]}");
                Log($"Set isUsed oldIsUsed {i}: {oldIsUsed[i]}");
                if (oldIsUsed[i] && !value[i])
                {
                    Log("Value Changed to NotUsed:" + i);
                    _syncBoards[i].Clear();
                }
            }

            _isUsed = value;
            oldIsUsed = (bool[])value.Clone();
            RequestSendPackage();
        }

        public override void OnPreSerialization()
            => _isUsed = isUsed;

        public override void OnDeserialization()
            => isUsed = _isUsed;

        public override void OnPostSerialization(SerializationResult result)
        {
            isInUseSyncBuffer = false;
        }

        private bool isInUseSyncBuffer = false;

        private void RequestSendPackage()
        {
            Log("QvBoardObjectPool RequestSendPackage");
            if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject) && !isInUseSyncBuffer)
            {
                isInUseSyncBuffer = true;
                RequestSerialization();
            }
        }


        private bool _isNetworkSettled = false;

        private bool isNetworkSettled
            => _isNetworkSettled || (_isNetworkSettled = Networking.IsNetworkSettled);


        private bool _initialized = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            Log("QvBoardObjectPool Initialize");

            // 子オブジェクトをすべてプールに設定
            int childCount = transform.childCount;
            _pool = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                _pool[i] = transform.GetChild(i).gameObject;
            }

            
            _isUsed = new bool[_pool.Length];
            oldIsUsed = new bool[_pool.Length];
            _syncBoards = new QvBoard_SyncBoard[_pool.Length];
            _meshRenderers = new MeshRenderer[_pool.Length];
            for (int i = 0; i < _pool.Length; i++)
            {
                _syncBoards[i] = _pool[i].GetComponent<QvBoard_SyncBoard>();
                _meshRenderers[i] = _pool[i].GetComponent<MeshRenderer>();
            }


            _initialized = true;
        }

        public int TryToSpawn()
        {
            Log("QvBoardObjectPool TryToSpawn");
            Initialize();


            for (int i = 0; i < isUsed.Length; i++)
            {
                if (!isUsed[i])
                {
                    Debug.Log("QvBoardObjectPool TryToSpawn i: " + i);
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    Networking.SetOwner(Networking.LocalPlayer, _pool[i]);
                    SetIsUsed(i, true);
                    CheckPoolAllUsedAndSet();
                    return i;
                }
            }

            Error("QvBoardObjectPool Failed to spawn object from pool");
            return -1;
        }

        public GameObject GetObject(int index, bool switchOwner)
        {
            Log("QvBoardObjectPool GetObject index: " + index);
            if (0 <= index && index < _pool.Length)
            {
                CheckPoolAllUsedAndSet();
                if (switchOwner) Networking.SetOwner(Networking.LocalPlayer, _pool[index]);
                return _pool[index];
            }

            Error("QvBoardObjectPool Invalid index: " + index);
            return null;
        }

        public QvBoard_SyncBoard getSyncBoard(int index)
        {
            Log("QvBoardObjectPool getSyncBoard index: " + index);
            if (0 <= index && index < _syncBoards.Length)
            {
                return _syncBoards[index];
            }

            return null;
        }

        public void Return(int index)
        {
            Log("QvBoardObjectPool ReturnObject index: " + index);
            if (0 <= index && index < _pool.Length)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                Networking.SetOwner(Networking.LocalPlayer, _pool[index]);
                SetIsUsed(index, false);
                // TODO 回数減らせるはず
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(AllPoolNotUsed));
            }
            else
            {
                Error("QvBoardObjectPool Invalid or NotUsed index: " + index);
            }
        }


        public void ReturnAll()
        {
            Log("QvBoardObjectPool ReturnAll");
            Initialize();

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            for (int i = 0; i < _syncBoards.Length; i++)
            {
                // TODO 非効率
                Return(i);
            }
        }

        public void AllPoolUsed()
        {
            Log("QvBoardObjectPool AllPoolUsed");
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _meshRenderers[i].enabled = true;
            }
        }

        public void AllPoolNotUsed()
        {
            Log("QvBoardObjectPool AllPoolNotUsed");
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _meshRenderers[i].enabled = false;
            }
        }

        private void CheckPoolAllUsedAndSet()
        {
            Log("QvBoardObjectPool CheckPoolAllActiveAndSet CountActive: " + CountActive + " PoolLength: " +
                _pool.Length);
            if (_pool.Length <= CountActive)
            {
                Warning("QvBoardObjectPool Pool is full");
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(AllPoolUsed));
            }
        }


        private int CountActive
        {
            get
            {
                Initialize();

                int count = 0;
                for (int i = 0; i < isUsed.Length; i++)
                {
                    if (isUsed[i])
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0x52, 0x5d, 0xfa, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;

        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_ObjectPool)}{ColorEndTag}] ")
                : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}