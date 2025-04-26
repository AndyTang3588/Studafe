
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace yue
{
    // 現段階ではBoardMakerで出来ないManual実行のためのクラス
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvBoard_BoardManager : UdonSharpBehaviour
    {
        [SerializeField]
        private QvBoard_BoardMaker boardMaker;
        
        [SerializeField]
        private QvBoard_ObjectPool objectPool;
        
        private void Start()
        {
            boardMaker._Init(this, objectPool);
        }

        public bool _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject))
            {
                _ClearSyncBuffer();
                return true;
            }
            else
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                return Networking.IsOwner(gameObject);
            }
        }
        
        private bool _isNetworkSettled = false;
        private bool isNetworkSettled
            => _isNetworkSettled || (_isNetworkSettled = Networking.IsNetworkSettled);

        
        [UdonSynced]
        private Vector3[] _syncedData;
        private Vector3[] syncedData
        {
            get => _syncedData;
            set
            {
                Log("QvBoard_BoardManager syncedData set");
                if (!isNetworkSettled)
                    return;

                Log("Set syncedData");
                for (int i = 0; i < value.Length; i++)
                {
                    Log($"Set syncedData value {i}: {value[i]}");
                }
                
                _syncedData = value;

                RequestSendPackage();

                if (_syncedData != null)
                    boardMaker._UnpackData(_syncedData);
            }
        }

        private bool isInUseSyncBuffer = false;
        private void RequestSendPackage()
        {
            if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject) && !isInUseSyncBuffer)
            {
                isInUseSyncBuffer = true;
                RequestSerialization();
            }
        }

        public void _SendData(Vector3[] data)
        {
            if (!isInUseSyncBuffer)
                syncedData = data;
        }

        public override void OnPreSerialization()
            => _syncedData = syncedData;

        public override void OnDeserialization()
            => syncedData = _syncedData;

        public override void OnPostSerialization(SerializationResult result)
        {
            isInUseSyncBuffer = false;

            // if (result.success)
            //     pen.ExecuteEraseInk();
            // else
            //     pen.DestroyJustBeforeInk();
        }

        public void _ClearSyncBuffer()
        {
            syncedData = new Vector3[] { };
            isInUseSyncBuffer = false;
        }
        
        // TODO HOTFIX 要ClearButton対応
        public void Clear()
        {
            // bordmakerのboardPoolSyncedの子のQVBoardを全て削除する
            QvBoard[] qvBoards = boardMaker.boardPoolSynced.GetComponentsInChildren<QvBoard>(); 
            for(int i = 0; i < qvBoards.Length; i++)
            {
                objectPool.Return(qvBoards[i].poolIndex);
            }
        }
        
        #region Log

        private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xf2, 0xbd, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;
        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_BoardManager)}{ColorEndTag}] ") : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}
