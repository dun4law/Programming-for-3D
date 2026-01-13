using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Contrast Stretch")]
    public class ContrastStretch : MonoBehaviour
    {

        [Range(0.0001f, 1.0f)]
        public float adaptationSpeed = 0.02f;

        [Range(0.0f,1.0f)]
        public float limitMinimum = 0.2f;

        [Range(0.0f, 1.0f)]
        public float limitMaximum = 0.6f;

        private RenderTexture[] adaptRenderTex = new RenderTexture[2];
        private int curAdaptIndex = 0;

        public Shader   shaderLum;
        private Material m_materialLum;
        protected Material materialLum {
            get {
                if ( m_materialLum == null ) {
                    m_materialLum = new Material(shaderLum);
                    m_materialLum.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_materialLum;
            }
        }

        public Shader   shaderReduce;
        private Material m_materialReduce;
        protected Material materialReduce {
            get {
                if ( m_materialReduce == null ) {
                    m_materialReduce = new Material(shaderReduce);
                    m_materialReduce.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_materialReduce;
            }
        }

        public Shader   shaderAdapt;
        private Material m_materialAdapt;
        protected Material materialAdapt {
            get {
                if ( m_materialAdapt == null ) {
                    m_materialAdapt = new Material(shaderAdapt);
                    m_materialAdapt.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_materialAdapt;
            }
        }

        public Shader   shaderApply;
        private Material m_materialApply;
        protected Material materialApply {
            get {
                if ( m_materialApply == null ) {
                    m_materialApply = new Material(shaderApply);
                    m_materialApply.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_materialApply;
            }
        }

        void Start()
        {
            if (!shaderAdapt.isSupported || !shaderApply.isSupported || !shaderLum.isSupported || !shaderReduce.isSupported) {
                enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            for( int i = 0; i < 2; ++i )
            {
                if ( !adaptRenderTex[i] ) {
                    adaptRenderTex[i] = new RenderTexture(1, 1, 0);
                    adaptRenderTex[i].hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        void OnDisable()
        {
            for( int i = 0; i < 2; ++i )
            {
                DestroyImmediate( adaptRenderTex[i] );
                adaptRenderTex[i] = null;
            }
            if ( m_materialLum )
                DestroyImmediate( m_materialLum );
            if ( m_materialReduce )
                DestroyImmediate( m_materialReduce );
            if ( m_materialAdapt )
                DestroyImmediate( m_materialAdapt );
            if ( m_materialApply )
                DestroyImmediate( m_materialApply );
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {

            const int TEMP_RATIO = 1;
            RenderTexture rtTempSrc = RenderTexture.GetTemporary(source.width/TEMP_RATIO, source.height/TEMP_RATIO);
            Graphics.Blit (source, rtTempSrc, materialLum);

            const int FINAL_SIZE = 1;

            while( rtTempSrc.width > FINAL_SIZE || rtTempSrc.height > FINAL_SIZE )
            {
                const int REDUCE_RATIO = 2;
                int destW = rtTempSrc.width / REDUCE_RATIO;
                if ( destW < FINAL_SIZE ) destW = FINAL_SIZE;
                int destH = rtTempSrc.height / REDUCE_RATIO;
                if ( destH < FINAL_SIZE ) destH = FINAL_SIZE;
                RenderTexture rtTempDst = RenderTexture.GetTemporary(destW,destH);
                Graphics.Blit (rtTempSrc, rtTempDst, materialReduce);

                RenderTexture.ReleaseTemporary( rtTempSrc );
                rtTempSrc = rtTempDst;
            }

            CalculateAdaptation( rtTempSrc );

            materialApply.SetTexture("_AdaptTex", adaptRenderTex[curAdaptIndex] );
            Graphics.Blit (source, destination, materialApply);

            RenderTexture.ReleaseTemporary( rtTempSrc );
        }

        private void CalculateAdaptation( Texture curTexture )
        {
            int prevAdaptIndex = curAdaptIndex;
            curAdaptIndex = (curAdaptIndex+1) % 2;

            float adaptLerp = 1.0f - Mathf.Pow( 1.0f - adaptationSpeed, 30.0f * Time.deltaTime );
            const float kMinAdaptLerp = 0.01f;
            adaptLerp = Mathf.Clamp( adaptLerp, kMinAdaptLerp, 1 );

            materialAdapt.SetTexture("_CurTex", curTexture );
            materialAdapt.SetVector("_AdaptParams", new Vector4(
                                                        adaptLerp,
                                                        limitMinimum,
                                                        limitMaximum,
                                                        0.0f
                                                        ));

            Graphics.SetRenderTarget(adaptRenderTex[curAdaptIndex]);
            GL.Clear(false, true, Color.black);
            Graphics.Blit (
                adaptRenderTex[prevAdaptIndex],
                adaptRenderTex[curAdaptIndex],
                materialAdapt);
        }
    }
}
