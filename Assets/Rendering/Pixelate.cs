using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateRenderFeature : ScriptableRendererFeature
{
    class PixelatePass : ScriptableRenderPass
    {
        private Material _material;

        private RTHandle _cameraColorTarget;
        private RTHandle _tempRT;

        public PixelatePass(Material material)
        {
            _material = material;
        }

        public void Setup(RTHandle cameraColorTarget)
        {
            _cameraColorTarget = cameraColorTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Crée une texture temporaire compatible RTHandle
            RenderingUtils.ReAllocateIfNeeded(
                ref _tempRT,
                cameraTextureDescriptor,
                name: "_PixelateTempTex"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("PixelatePass");

            // Shader pass unique = index 0
            Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _tempRT, _material, 0);
            Blitter.BlitCameraTexture(cmd, _tempRT, _cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Rien à libérer : RTHandle est géré par le renderer
        }
    }

    public Material pixelateMaterial;
    PixelatePass _pass;

    public override void Create()
    {
        _pass = new PixelatePass(pixelateMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pixelateMaterial == null) return;

        _pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(_pass);
    }
}