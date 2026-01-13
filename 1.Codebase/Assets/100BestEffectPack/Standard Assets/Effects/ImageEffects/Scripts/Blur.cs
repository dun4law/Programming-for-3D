using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Blur/Blur")]
    public class Blur : MonoBehaviour
    {

        [Range(0,10)]
        public int iterations = 3;

        [Range(0.0f,1.0f)]
        public float blurSpread = 0.6f;

        public Shader blurShader = null;

        static Material m_Material = null;
        protected Material material {
            get {
                if (m_Material == null) {
                    m_Material = new Material(blurShader);
                    m_Material.hideFlags = HideFlags.DontSave;
                }
                return m_Material;
            }
        }

        protected void OnDisable() {
            if ( m_Material ) {
                DestroyImmediate( m_Material );
            }
        }

        protected void Start()
        {

            if (!blurShader || !material.shader.isSupported) {
                enabled = false;
                return;
            }
        }

        public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
        {
            float off = 0.5f + iteration*blurSpread;
            Graphics.BlitMultiTap (source, dest, material,
                                   new Vector2(-off, -off),
                                   new Vector2(-off,  off),
                                   new Vector2( off,  off),
                                   new Vector2( off, -off)
                );
        }

        private void DownSample4x (RenderTexture source, RenderTexture dest)
        {
            float off = 1.0f;
            Graphics.BlitMultiTap (source, dest, material,
                                   new Vector2(-off, -off),
                                   new Vector2(-off,  off),
                                   new Vector2( off,  off),
                                   new Vector2( off, -off)
                );
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination) {
            int rtW = source.width/4;
            int rtH = source.height/4;
            RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

            DownSample4x (source, buffer);

            for(int i = 0; i < iterations; i++)
            {
                RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
                FourTapCone (buffer, buffer2, i);
                RenderTexture.ReleaseTemporary(buffer);
                buffer = buffer2;
            }
            Graphics.Blit(buffer, destination);

            RenderTexture.ReleaseTemporary(buffer);
        }
    }
}
