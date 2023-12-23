using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Mumei.XR.Cardboard
{
    public class CameraPreview : MonoBehaviour
    {
        public Renderer targetRenderer;

        ARCameraManager cameraManager_ = null;
        Texture2D cameraTexture_ = null;

        void Start()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
        }

        void Update()
        {
            if (cameraManager_ == null)
            {
                cameraManager_ = FindObjectOfType<ARCameraManager>();
                cameraManager_.frameReceived += onCameraFrameReceived;
            }

            if (cameraTexture_ != null)
            {
                targetRenderer.material.mainTexture = cameraTexture_;
            }
        }

        void OnDestroy()
        {
            if (cameraManager_ != null)
            {
                cameraManager_.frameReceived -= onCameraFrameReceived;
            }
        }

        void onCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (!cameraManager_.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                return;
            }

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            unsafe
            {
                int size = image.GetConvertedDataSize(conversionParams);
                var buffer = new NativeArray<byte>(size, Allocator.Temp);
                image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
                image.Dispose();

                if (cameraTexture_ == null)
                {
                    cameraTexture_ = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
                }
                cameraTexture_.LoadRawTextureData(buffer);
                cameraTexture_.Apply();
            }
        }
    }
}
