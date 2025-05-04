using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.qzpvauo31rqw")]
    public class Speaker : IwaSync3Base
    {
#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("指定控制源的iwaSync3")]
        IwaSync3 iwaSync3;
        [SerializeField]
        [Tooltip("为每个播放模式设置使用与否的掩码\n可以为各模式指定是否启用")]
        TrackModeMask mask = (TrackModeMask)(-1);
        [SerializeField]
        [Tooltip("指定是否设置为**主扬声器**\n在Video模式时优先使用此扬声器")]
        bool primary = false;

        [SerializeField]
        [Tooltip("指定音频最大可听距离")]
        float maxDistance = 12f;

        [SerializeField]
        [Tooltip("指定是否启用**3D立体音效**")]
        bool spatialize = false;

        [SerializeField]
        [Tooltip("指定输出到扬声器的音频信号类型")]
        ChannelMode mode;
#pragma warning restore CS0414
    }
}
