
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Taurus;
public static class MenuRender
{
  public static bool Ready
  {
    get
    {
      if (SystemInfo.graphicsDeviceType == (GraphicsDeviceType) 4)
        return false;
      return !((Object) GraphicsSettings.currentRenderPipeline != (Object) null) ? !Application.isPlaying || Time.frameCount > 0 : RenderPipelineManager.currentPipeline != null && !RenderPipelineManager.currentPipeline.disposed;
    }
  }
  public static void Configure(Camera camera)
  {
    ((Behaviour) camera).enabled = false;
    if (((Object) GraphicsSettings.currentRenderPipeline == (Object) null))
    {
      camera.stereoTargetEye = (StereoTargetEyeMask) 0;
    }
    else
    {
      if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
        return;
      UniversalAdditionalCameraData additionalCameraData = CameraExtensions.GetUniversalAdditionalCameraData(camera);
      additionalCameraData.allowXRRendering = false;
      additionalCameraData.renderPostProcessing = false;
      additionalCameraData.renderShadows = false;
      additionalCameraData.requiresColorOption = (CameraOverrideOption) 0;
      additionalCameraData.requiresDepthOption = (CameraOverrideOption) 0;
      additionalCameraData.volumeLayerMask = ((LayerMask)0);
    }
  }
  public static bool TryRender(Camera camera)
  {
    if (!MenuRender.Ready)
      return false;
    RenderTexture active = RenderTexture.active;
    try
    {
      if (((Object) GraphicsSettings.currentRenderPipeline == (Object) null))
      {
        camera.Render();
        return true;
      }
      if (!(RenderPipelineManager.currentPipeline is UniversalRenderPipeline))
        return false;
      UniversalRenderPipeline.SingleCameraRequest singleCameraRequest = new UniversalRenderPipeline.SingleCameraRequest()
      {
        destination = camera.targetTexture
      };
      if (!RenderPipeline.SupportsRenderRequest<UniversalRenderPipeline.SingleCameraRequest>(camera, singleCameraRequest))
        return false;
      RenderPipeline.SubmitRenderRequest<UniversalRenderPipeline.SingleCameraRequest>(camera, singleCameraRequest);
      return true;
    }
    finally
    {
      RenderTexture.active = active;
    }
  }
}
