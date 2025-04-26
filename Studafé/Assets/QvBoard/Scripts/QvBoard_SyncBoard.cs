using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace yue
{
    public class QvBoard_SyncBoard : UdonSharpBehaviour
    {
        
        // 本当は1つで良いが、安全のため複数持つ
        // public DataList qvBoards = new DataList();
        [System.NonSerialized]
        public QvBoard qvBoard = null;
        
        public void Init(QvBoard qvBoard)
        {
            if(this.qvBoard != null)
            {
                Error("QvBoard_SyncBoard Init qvBoard is not null!! duplicate Set!!");
            }
            transform.position = qvBoard.transform.position;
            transform.rotation = qvBoard.transform.rotation;
            transform.localScale = qvBoard.transform.localScale;

            this.qvBoard = qvBoard;
         }
        
        public void InitForSync(QvBoard qvBoard)
        {
            if(this.qvBoard != null)
            {
                Error("QvBoard_SyncBoard Init qvBoard is not null!! duplicate Set!!");
            }
            // transformの変更はObjectSyncで同期されるので省略
            this.qvBoard = qvBoard;
        }
        
        public void Clear()
        {
            Log("QvBoard_SyncBoard Clear");
            
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.zero;
            
            if (qvBoard != null)
            {
                CallEraseThis(qvBoard);
                qvBoard = null;
            }
        }
        
        private void CallEraseThis(QvBoard qvBoard)
        {
            Log("Check qvBoard.poolIndex:" + qvBoard.poolIndex); 
            Log("QvBoard_SyncBoard CallEraseThis");
            qvBoard.EraseThis();
        }


        
        #region Log

        // private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Log(object o) { }
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xf2, 0xfd, 0xfa, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        private string _logPrefix;
        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvBoard_SyncBoard)}{ColorEndTag}] ") : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}