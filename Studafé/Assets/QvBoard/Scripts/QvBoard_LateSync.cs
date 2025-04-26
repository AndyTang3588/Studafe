
using System;
using QvPen.UdonScript;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using yue;
using Random = UnityEngine.Random;

namespace yue
{
    // QVPenでのQvPen_LateSyncに相当するクラス
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvBoard_LateSync : UdonSharpBehaviour
    {
        [System.NonSerialized] 
        public QvBoard_BoardMaker boardMaker;

        [System.NonSerialized] 
        public QvPen_Pen pen;
        
        [System.NonSerialized] 
        public bool Initialized = false;
        
        private QvBoard[] boardBuffer = { };
        private int boardIndex = -1;
        private VRCPlayerApi master = null;

        private bool amIStartSyncer = false;
        
        private bool isSynceePenSynced = false;
        
        private int joinedNum = 0;
        private int startSyncNum = 0;
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            isSynceePenSynced = false;
            
            if (master == null || player.playerId < master.playerId)
                master = player;

            if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject))
                amIStartSyncer = true;
            
            joinedNum++;
        }

        void Update()
        {
            if(!Initialized) return;
            // クライアント側はペンの同期が終わったら同期を開始する
            // ボードの同期が終わったら終了する
            if (!Networking.IsOwner(gameObject) 
                && !isSynceePenSynced 
                && pen.currentSyncState == QvPen_Pen_SyncState.Finished 
                && currentSyncState != SYNC_STATE_Finished )
            {
                // Log("QvBoard_LateSync Update SetIsSynceePenSynced SYNC_STATE_Finished");
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(SetTrueIsSynceePenSynced));
                SendCustomEventDelayedSeconds(nameof(SetTrueIsSynceePenSynced), 1.84f * (1f + Random.value));
            }

            // オーナー側はLateJoinerのペンの同期が終わったら同期を開始する
            if (isSynceePenSynced && amIStartSyncer)
            {
                if(startSyncNum < joinedNum)
                {
                    startSyncNum++;
                    StartSync();
                }
                isSynceePenSynced = false;
            }
        }

        public void SetTrueIsSynceePenSynced()
        {
            Log("QvBoard_LateSync SetTrueIsSynceePenSynced");
            isSynceePenSynced = true;            
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            master = player;

            // TODO この口はなんのためについている？
            // if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject))
            //     SendCustomEventDelayedSeconds(nameof(StartSync), 1.84f * (1f + Random.value));
        }

        private bool forceStart = false;
        private int retryCount = 0;

        public void StartSync()
        {
            Log("QVBoard_LateSync StartSync startSyncNum" + startSyncNum + " joinedNum" + joinedNum); 
            forceStart = true;
            retryCount = 0;

            SendBiginSignal();
        }
        
        [UdonSynced] private Vector3[] _syncedData;

        private Vector3[] syncedData
        {
            get => _syncedData;
            set
            {
                Log("Set syncedData in OnDesirialization forceStart: " + forceStart);
                for (int i = 0; i < value.Length; i++)
                {
                    Log($"Set syncedData value {i}: {value[i]}");
                }
                
                if (forceStart)
                {
                    _syncedData = new Vector3[] { boardMaker.boardMakerIdVector, beginSignal };

                    if (Networking.IsOwner(gameObject))
                        _RequestSendPackage();
                }
                else
                {
                    // if (pen.currentSyncState != SYNC_STATE_Finished) return;
                    _syncedData = value;

                    if (Networking.IsOwner(gameObject))
                        _RequestSendPackage();
                    else if (_syncedData != null && _syncedData.Length > 0)
                        UnpackData(_syncedData);
                }
            }
        }

        public override void OnPreSerialization()
            => _syncedData = syncedData;

        public override void OnDeserialization()
        {
            Log("QvBoard_LateSync OnDeserialization");
            syncedData = _syncedData;
        }

        private const int maxRetryCount = 3;

        public override void OnPostSerialization(SerializationResult result)
        {
            Log("QvBoard_LateSync OnPostSerialization");
            
            isInUseSyncBuffer = false;

            if (!result.success)
            {
                if (retryCount++ < maxRetryCount)
                    SendCustomEventDelayedSeconds(nameof(_RequestSendPackage), 1.84f);
            }
            else
            {
                retryCount = 0;

                var signal = GetCalibrationSignal(syncedData);
                if (signal == errorSignal)
                    return;
                else if (signal == beginSignal)
                {
                    forceStart = false;

                    boardBuffer = boardMaker.boardPoolSynced.GetComponentsInChildren<QvBoard>();

                    boardIndex = -1;
                }
                else if (signal == endSignal)
                {
                    boardBuffer = new QvBoard[] { };

                    syncedData = new Vector3[] { };
                    isInUseSyncBuffer = false;

                    return;
                }

                var board = GetNextBoard();
                if (board)
                    SendData(boardMaker._PackData(board));
                else
                    SendEndSignal();
            }
        }

        // Sync state
        [System.NonSerialized] private int currentSyncState = SYNC_STATE_Idle;
        private const int SYNC_STATE_Idle = 0;
        private const int SYNC_STATE_Started = 1;
        private const int SYNC_STATE_Finished = 2;

        private void UnpackData(Vector3[] data)
        {
            Log("QvBoard_LateSync UnpackData currentSyncState" + currentSyncState);
            if (currentSyncState == SYNC_STATE_Finished)
            {
                return;
            }

            var signal = GetCalibrationSignal(data);
            Log("QvBoard_LateSync UnpackData signal" + signal);
            if (signal == beginSignal)
            {
                if (currentSyncState == SYNC_STATE_Idle)
                    currentSyncState = SYNC_STATE_Started;
            }
            else if (signal == endSignal)
            {
                if (currentSyncState == SYNC_STATE_Started)
                    currentSyncState = SYNC_STATE_Finished;
            }
            else
            {
                boardMaker._UnpackData(data);
            }
        }

        private readonly Vector3 beginSignal = new Vector3(2.7182818e8f, 1f, 6.2831853e4f);
        private readonly Vector3 endSignal = new Vector3(2.7182818e8f, 0f, 6.2831853e4f);
        private readonly Vector3 errorSignal = new Vector3(2.7182818e8f, -1f, 6.2831853e4f);

        private void SendBiginSignal()
            => SendData(new Vector3[] { boardMaker.boardMakerIdVector, beginSignal });

        private void SendEndSignal()
            => SendData(new Vector3[] { boardMaker.boardMakerIdVector, endSignal });

        private Vector3 GetCalibrationSignal(Vector3[] data)
            => data.Length > 1 ? data[1] : errorSignal;

        private bool isInUseSyncBuffer = false;

        private void SendData(Vector3[] data)
        {
            Log("QvBoard_LateSync SendData");
            if (!isInUseSyncBuffer)
                syncedData = data;
        }

        private bool _isNetworkSettled = false;

        private bool isNetworkSettled
            => _isNetworkSettled || (_isNetworkSettled = Networking.IsNetworkSettled);

        public void _RequestSendPackage()
        {
            Log("_RequestSendPackage");
            if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject))
            {
                if (!isNetworkSettled)
                {
                    SendCustomEventDelayedSeconds(nameof(_RequestSendPackage), 1.84f);
                    return;
                }

                isInUseSyncBuffer = true;
                RequestSerialization();
            }
        }

        private QvBoard GetNextBoard()
        {
            boardIndex = Mathf.Max(-1, boardIndex);

            while (++boardIndex < boardBuffer.Length)
            {
                var board = boardBuffer[boardIndex];
                if (board)
                    return board;
            }

            return null;
        }
           
        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xc2, 0xcd, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;
        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_LateSync)}{ColorEndTag}] ") : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}