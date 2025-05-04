using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.v8xuael90eyq")]
    public class Screen : IwaSync3Base
    {
#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("指定控制源的iwaSync3")]
        IwaSync3 iwaSync3;

        [SerializeField]
        [Tooltip("如果目标材质有多个，请指定索引")]
        int materialIndex = 0;
        [SerializeField]
        [Tooltip("指定要应用的材质属性名")]
        string textureProperty = "_MainTex";
        [SerializeField]
        [Tooltip("空闲时是否关闭显示")]
        bool idleScreenOff = false;
        [SerializeField]
        [Tooltip("空闲时若要显示指定纹理，请在此设置")]
        Texture idleScreenTexture = null;
        [SerializeField]
        [Tooltip("指定屏幕的宽高比")]
        float aspectRatio = 1.777778f;
        [SerializeField]
        [Tooltip("是否反转镜子中显示的画面")]
        bool defaultMirror = true;
        [SerializeField]
        [Tooltip("指定亮度倍率")]
        [Range(0f, 5f)]
        float defaultEmissiveBoost = 1f;
        [SerializeField]
        [Tooltip("指定绘制目标的渲染器")]
        Renderer screen;
#pragma warning restore CS0414
    }
}
