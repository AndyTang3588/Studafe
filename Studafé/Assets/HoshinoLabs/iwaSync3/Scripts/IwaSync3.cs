using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.w55eaf4xlnuu")]
    public class IwaSync3 : IwaSync3Base
    {
#pragma warning disable CS0414
// main
[SerializeField]
IwaSync3 iwaSync3;

// control
[SerializeField]
[Tooltip("指定使用默认播放时的播放模式")]
TrackMode defaultMode = TrackMode.Video;
[SerializeField]
[Tooltip("指定默认播放时要播放的视频URL")]
string defaultUrl;
[SerializeField]
[Tooltip("指定默认播放的延迟秒数")]
float defaultDelay = 0f;
[SerializeField]
[Tooltip("是否允许通过滑块调整播放位置")]
bool allowSeeking = true;
[SerializeField]
[Tooltip("是否默认启用单次循环")]
bool defaultLoop = false;
[SerializeField]
[Tooltip("按下左右快进按钮时移动的秒数")]
float seekTimeSeconds = 10f;
[SerializeField]
[Tooltip("时间显示的格式")]
string timeFormat = @"hh\:mm\:ss\:ff";

// sync
[SerializeField]
[Tooltip("进行播放时间同步修正的间隔")]
float syncFrequency = 9.2f;
[SerializeField]
[Tooltip("进行播放时间同步修正的阈值")]
float syncThreshold = 0.92f;

// error handling
[SerializeField]
[Tooltip("错误发生时的最大重试次数")]
[Range(0, 10)]
int maxErrorRetry = 3;
[SerializeField]
[Tooltip("判定视频无法正常播放所需的时间")]
[Range(10f, 30f)]
float timeoutUnknownError = 10f;
[SerializeField]
[Tooltip("播放失败时重试前的等待时间")]
[Range(6f, 30f)]
float timeoutPlayerError = 6f;
[SerializeField]
[Tooltip("发生视频限速时重试前的等待时间")]
[Range(6f, 30f)]
float timeoutRateLimited = 6f;
[SerializeField]
[Tooltip("发生错误时是否允许降低最大分辨率")]
bool allowErrorReduceMaxResolution = true;

// lock
[SerializeField]
[Tooltip("是否默认启用主锁状态")]
bool defaultLock = false;
[SerializeField]
[Tooltip("是否将主锁状态扩展给实例拥有者")]
bool allowInstanceOwner = true;

// video
[SerializeField]
[Tooltip("当播放视频的分辨率可选时，选择的最大分辨率")]
int maximumResolution = 720;
[SerializeField]
[Tooltip("画面亮度系数")]
[Range(0f, 1f)]
float defaultBrightness = 1f;

// audio
[SerializeField]
[Tooltip("是否默认静音")]
bool defaultMute = false;
[SerializeField]
[Tooltip("默认的最小音量")]
[Range(0f, 1f)]
float defaultMinVolume = 0f;
[SerializeField]
[Tooltip("默认的最大音量")]
[Range(0f, 1f)]
float defaultMaxVolume = 0.5f;
[SerializeField]
[Tooltip("默认音量")]
[Range(0f, 1f)]
float defaultVolume = 0.184f;

// extra
[SerializeField]
//[Tooltip("Low latency playback of live stream")]
[Tooltip("是否在直播中使用低延迟播放")]
bool useLowLatency = false;
#pragma warning restore CS0414
}
}
