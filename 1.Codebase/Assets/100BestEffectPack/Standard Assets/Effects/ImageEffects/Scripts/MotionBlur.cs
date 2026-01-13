using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Blur/Motion Blur (Color Accumulation)")]
    [RequireComponent(typeof(Camera))]
    public class MotionBlur : ImageEffectBase
    {
        [Range(0.0f, 0.92f)]
        public float blurAmount = 0.8f;
        public bool extraBlur = false;

        private RenderTexture accumTexture;

        override protected void Start()
        {
            base.Start();
        }

        override protected void OnDisable()
        {
            base.OnDisable();
            DestroyImmediate(accumTexture);
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {

            if (accumTexture == null || accumTexture.width != source.width || accumTexture.height != source.height)
            {
                DestroyImmediate(accumTexture);
                accumTexture = new RenderTexture(source.width, source.height, 0);
                accumTexture.hideFlags = HideFlags.HideAndDontSave;
                Graphics.Blit( source, accumTexture );
            }

            if (extraBlur)
            {
                RenderTexture blurbuffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
                Graphics.Blit(accumTexture, blurbuffer);
                Graphics.Blit(blurbuffer,accumTexture);
                RenderTexture.ReleaseTemporary(blurbuffer);
            }

            blurAmount = Mathf.Clamp( blurAmount, 0.0f, 0.92f );

            material.SetTexture("_MainTex", accumTexture);
            material.SetFloat("_AccumOrig", 1.0F-blurAmount);

            Graphics.Blit (source, accumTexture, material);
            Graphics.Blit (accumTexture, destination);
        }
    }
}
