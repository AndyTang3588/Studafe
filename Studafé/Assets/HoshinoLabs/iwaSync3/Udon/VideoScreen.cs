using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace HoshinoLabs.IwaSync3.Udon
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoScreen : VideoCoreEventListener
    {
        [Header("Main")]
        [SerializeField]
        VideoCore core;

        [Header("Options")]
        [SerializeField]
        int materialIndex = 0;
        [SerializeField]
        string textureProperty = "_MainTex";
        [SerializeField]
        bool idleScreenOff = false;
        [SerializeField]
        Texture idleScreenTexture = null;
        [SerializeField]
        float aspectRatio = 1.777778f;
        [SerializeField, FieldChangeCallback(nameof(mirror))]
        bool defaultMirror = true;
        [SerializeField, Range(0f, 5f), FieldChangeCallback(nameof(emissiveBoost))]
        float defaultEmissiveBoost = 1f;
        [SerializeField]
        Renderer screen;

        MaterialPropertyBlock _properties;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Started `{nameof(VideoScreen)}`.");

            core.AddListener(this);

            _properties = new MaterialPropertyBlock();
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }
        #endregion

        #region VideoEvent
        public override void OnVideoEnd()
        {
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            ValidateView();
        }

        public override void OnVideoStart()
        {
            ValidateView();
        }
        #endregion

        #region VideoCoreEvent
        public override void OnPlayerPlay()
        {
            ValidateView();
        }

        public override void OnPlayerPause()
        {
            ValidateView();
        }

        public override void OnPlayerStop()
        {
            ValidateView();
        }

        public override void OnChangeBrightness() {
            ValidateView();
        }
        #endregion

        public void ValidateView()
        {
            screen.enabled = !idleScreenOff || core.isPlaying;
            var texture = idleScreenTexture;
            var aspect = aspectRatio;
            if (texture != null) {
                aspect = (float)texture.width / texture.height;
            }
            if (core.isPlaying)
            {
                if(core.texture == null) {
                    SendCustomEventDelayedFrames(nameof(ValidateView), 0);
                }
                else {
                    texture = core.texture;
                    aspect = aspectRatio;
                }
            }
            _properties.Clear();
            if (texture != null) {
                _properties.SetTexture(textureProperty, texture);
            }
            _properties.SetFloat("_AspectRatio", aspect);
            _properties.SetInt("_IsAVProVideo", core.isPlaying ? (core.isModeVideo ? 0 : 1) : 0);
#if UNITY_STANDALONE_WIN || UNITY_IOS
            _properties.SetInt("_IsVerticalMirror", core.isPlaying ? (core.isModeVideo ? 0 : 1) : 0);
#else
            _properties.SetInt("_IsVerticalMirror", 0);
#endif
            _properties.SetInt("_IsMirror", defaultMirror ? 1 : 0);
            var emissiveBoost = core.brightness * defaultEmissiveBoost;
            _properties.SetFloat("_EmissiveBoost", emissiveBoost);
            _properties.SetInt("_IsPlaying", core.isPlaying ? 1 : 0);
            screen.SetPropertyBlock(_properties, materialIndex);
        }

        private void Update()
        {
            if (!core.isPlaying)
                return;

            var texture = core.texture;
            if (texture != null)
            {
                _properties.SetTexture(textureProperty, texture);
                screen.SetPropertyBlock(_properties, materialIndex);
            }
        }

        public bool mirror
        {
            get => defaultMirror;
            set
            {
                defaultMirror = value;
                UpdateMirror();
            }
        }

        void UpdateMirror()
        {
            ValidateView();
        }

        public float emissiveBoost
        {
            get => defaultEmissiveBoost;
            set
            {
                defaultEmissiveBoost = Mathf.Clamp(value, 0f, 5f);
                UpdateEmissiveBoost();
            }
        }

        void UpdateEmissiveBoost()
        {
            ValidateView();
        }
    }
}
