using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace yue
{
    //TODO QvBoard_SyncBoardで削除するので、ここ全部いらないかも
    [DefaultExecutionOrder(20)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvBoard_EraserManager : UdonSharpBehaviour
    {
        [SerializeField]
        private QvBoard_Eraser eraser;
        
        public QvBoard_ObjectPool objectPool;
        
        private void Start() => eraser._Init(this);
    }
}