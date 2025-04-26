
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace aki_lua87.UdonScripts.marubatu
{
    public class uGUIConnector : UdonSharpBehaviour
    {
        [SerializeField] private marubatsugame marubatsugameBehavior;

        public void PushMasu0()
        {
            VirtualInteract(marubatsugameBehavior.masu[0], "0");
        }

        public void PushMasu1()
        {
            VirtualInteract(marubatsugameBehavior.masu[1], "1");
        }

        public void PushMasu2()
        {
            VirtualInteract(marubatsugameBehavior.masu[2], "2");
        }

        public void PushMasu3()
        {
            VirtualInteract(marubatsugameBehavior.masu[3], "3");
        }

        public void PushMasu4()
        {
            VirtualInteract(marubatsugameBehavior.masu[4], "4");
        }

        public void PushMasu5()
        {
            VirtualInteract(marubatsugameBehavior.masu[5], "5");
        }

        public void PushMasu6()
        {
            VirtualInteract(marubatsugameBehavior.masu[6], "6");
        }

        public void PushMasu7()
        {
            VirtualInteract(marubatsugameBehavior.masu[7], "7");
        }

        public void PushMasu8()
        {
            VirtualInteract(marubatsugameBehavior.masu[8], "8");
        }

        public void PushReset()
        {
            marubatsugameBehavior.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetGame");
            // marubatsugameBehavior.ResetGame();
        }

        private void VirtualInteract(GameObject masu, string index)
        {
            // masuの下のmaruとbatuを取得
            var maru = masu.transform.Find("maru").gameObject;
            var batu = masu.transform.Find("batu").gameObject;
            // 書き込まれていた場合は挿入不可
            if (maru.activeSelf || batu.activeSelf)
            {
                return;
            }
            if (marubatsugameBehavior.isGameEnd)
            {
                // ゲーム終了してたら書けないように
                return;
            }
            // 描写
            if (!marubatsugameBehavior.isFigureMaru)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WriteMaru" + index);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WriteBatu" + index);
            }
        }

        public void WriteMaru0()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[0].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu0()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[0].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru1()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[1].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu1()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[1].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru2()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[2].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu2()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[2].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru3()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[3].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu3()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[3].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru4()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[4].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu4()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[4].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru5()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[5].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu5()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[5].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru6()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[6].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu6()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[6].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru7()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[7].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu7()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[7].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteMaru8()
        {
            marubatsugameBehavior.NextFigure();
            var maru = marubatsugameBehavior.masu[8].transform.Find("maru").gameObject;
            maru.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }

        public void WriteBatu8()
        {
            marubatsugameBehavior.NextFigure();
            var batu = marubatsugameBehavior.masu[8].transform.Find("batu").gameObject;
            batu.SetActive(true);
            marubatsugameBehavior.GameEndCheck();
        }
    }
}
