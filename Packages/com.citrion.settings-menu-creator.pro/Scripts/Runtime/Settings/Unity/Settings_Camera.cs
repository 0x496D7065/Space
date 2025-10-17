using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;

namespace CitrioN.SettingsMenuCreator
{
  [DisplayName("Allow HDR (Builtin & URP)")]
  public class Setting_AllowHDR : Setting_Camera<bool>
  {
    public override string RuntimeName => "Allow HDR";

    public override string EditorName => "Allow HDR (Builtin & URP)";

    public override string PropertyName => nameof(Camera.allowHDR);
  }

  [DisplayName("Allow MSAA")]
  public class Setting_AllowMSAA : Setting_Camera<bool>
  {
    public override string RuntimeName => "Allow MSAA";

    public override string EditorName => "Allow MSAA";

    public override string PropertyName => nameof(Camera.allowMSAA);
  }

  // TODO Is a problematic setting
  //public class Setting_AllowDynamicResolution : Setting_Camera<bool>
  //{
  //  public override string RuntimeName => "Dynamic Resolution";

  //  public override string PropertyName => nameof(Camera.allowDynamicResolution);
  //}

  // Moved into free version
  //public class Setting_FieldOfView : Setting_Camera<float>
  //{
  //  public Setting_FieldOfView()
  //  {
  //    options.AddMinMaxRangeValues("50", "100");
  //    options.AddStepSize("1");

  //    defaultValue = 60;
  //  }

  //  public override string PropertyName => nameof(Camera.fieldOfView);
  //}

  // TODO Does a property or field for this exist?
  //public class FieldOfViewAxisSetting : Setting_Camera<FieldOfViewAxis>
  //{
  //  public override string PropertyName => nameof(Camera.fieldOfView);
  //}

  public class Setting_NearClipPlane : Setting_Camera<float>
  {
    public Setting_NearClipPlane()
    {
      options.AddMinMaxRangeValues("0.01", "0.5");
      options.AddStepSize("0.01");

      defaultValue = 0.3f;
    }

    public override string PropertyName => nameof(Camera.nearClipPlane);
  }

  public class Setting_FarClipPlane : Setting_Camera<float>
  {
    public Setting_FarClipPlane()
    {
      options.AddMinMaxRangeValues("500", "2000");
      options.AddStepSize("10");

      defaultValue = 1000;
      assignUnityValueAsDefault = false;
    }

    public override string PropertyName => nameof(Camera.farClipPlane);
  }

  public class Setting_IsOrthographic : Setting_Camera<bool>
  {
    public override string RuntimeName => "Orthographic";

    public override string PropertyName => nameof(Camera.orthographic);

    public Setting_IsOrthographic()
    {
      defaultValue = false;
    }
  }

  public class Setting_OrthographicSize : Setting_Camera<float>
  {
    public Setting_OrthographicSize()
    {
      options.AddMinMaxRangeValues("1", "50");
      options.AddStepSize("1");

      defaultValue = 5;
    }

    public override string PropertyName => nameof(Camera.orthographicSize);
  }

  [DisplayName("Rendering Path (Builtin)")]
  public class Setting_RenderingPath : Setting_Camera<RenderingPath>
  {
    public override string EditorName => "Rendering Path (Builtin)";

    public override string PropertyName => nameof(Camera.renderingPath);

    public Setting_RenderingPath()
    {
      defaultValue = RenderingPath.UsePlayerSettings;
    }
  }

  public class Setting_UseOcclusionCulling : Setting_Camera<bool>
  {
    public override string PropertyName => nameof(Camera.useOcclusionCulling);
  }

  //public class Setting_LensShift : Setting_Camera<Vector2, Vector2>
  //{
  //  public override string PropertyName => nameof(Camera.lensShift);
  //}

  public class Setting_CameraAspect : Setting_Camera<float>
  {
    public override string RuntimeName => "Aspect";

    public override string PropertyName => nameof(Camera.aspect);

    public Setting_CameraAspect()
    {
      options.AddMinMaxRangeValues("0.1", "3");
      options.AddStepSize("0.1");

      defaultValue = 1.626953f;
    }
  }

  public class Setting_CameraDepth : Setting_Camera<float>
  {
    public override string RuntimeName => "Depth";

    public override string PropertyName => nameof(Camera.depth);

    public Setting_CameraDepth()
    {
      options.AddMinMaxRangeValues("-10", "10");
      options.AddStepSize("1");

      defaultValue = 0;
    }
  }

  public class Setting_DepthTextureMode : Setting_Camera<DepthTextureMode>
  {
    public override string RuntimeName => "Depth Texture Mode";

    public override string PropertyName => nameof(Camera.depthTextureMode);

    public Setting_DepthTextureMode()
    {
      // We remove the motion vectors options because it does not load if set
      var motionVectorsOption = options.Find(o => o.Key == nameof(DepthTextureMode.MotionVectors));
      if (motionVectorsOption != null)
      {
        options?.Remove(motionVectorsOption);
      }
    }
  }

  // TODO Make this work?
  //public class Setting_TargetDisplay : Setting_Camera<int>
  //{
  //  public override string PropertyName => nameof(Camera.targetDisplay);

  //  public Setting_TargetDisplay()
  //  {
  //    options.Add(new StringToStringRelation("1", "Display 1"));
  //    options.Add(new StringToStringRelation("2", "Display 2"));
  //    options.Add(new StringToStringRelation("3", "Display 3"));
  //    options.Add(new StringToStringRelation("4", "Display 4"));
  //    options.Add(new StringToStringRelation("5", "Display 5"));
  //    options.Add(new StringToStringRelation("6", "Display 6"));
  //    options.Add(new StringToStringRelation("7", "Display 7"));
  //    options.Add(new StringToStringRelation("8", "Display 8"));
  //    options.Add(new StringToStringRelation("9", "Display 9"));

  //    assignUnityValueAsDefault = true;
  //  }
  //}

  public class Setting_StereoTargetEye : Setting_Camera<StereoTargetEyeMask>
  {
    public override string PropertyName => nameof(Camera.stereoTargetEye);

    public Setting_StereoTargetEye()
    {
      defaultValue = StereoTargetEyeMask.Both;
    }
  }

  public class Setting_StereoConvergence : Setting_Camera<float>
  {
    public override string PropertyName => nameof(Camera.stereoConvergence);

    public Setting_StereoConvergence()
    {
      options.AddMinMaxRangeValues("1", "50");
      options.AddStepSize("1");

      defaultValue = 10;
    }
  }

  //public class Setting_StereoSeparation : Setting_Camera<float>
  //{
  //  public override string PropertyName => nameof(Camera.stereoSeparation);
  //}

  // TODO Enable once Vector2 field is added
  //public class Setting_SensorSize : Setting_Camera<Vector2>
  //{
  //  public override string PropertyName => nameof(Camera.sensorSize);
  //}

  public class Setting_TransparencySortMode : Setting_Camera<TransparencySortMode>
  {
    public override string PropertyName => nameof(Camera.transparencySortMode);
  }

  public class Setting_OpaqueSortMode : Setting_Camera<OpaqueSortMode>
  {
    public override string PropertyName => nameof(Camera.opaqueSortMode);
  }

  public class Setting_LayerCullSpherical : Setting_Camera<bool>
  {
    public override string PropertyName => nameof(Camera.layerCullSpherical);

    public Setting_LayerCullSpherical()
    {
      defaultValue = false;
    }
  }

  public class Setting_GateFitMode : Setting_Camera<Camera.GateFitMode>
  {
    public override string PropertyName => nameof(Camera.gateFit);

    public Setting_GateFitMode()
    {
      defaultValue = Camera.GateFitMode.Horizontal;
    }
  }

  public class Setting_ForceIntoRenderTexture : Setting_Camera<bool>
  {
    public override string PropertyName => nameof(Camera.forceIntoRenderTexture);

    public Setting_ForceIntoRenderTexture()
    {
      defaultValue = false;
    }
  }

  [DisplayName("Camera Clear Flags (Builtin & URP)")]
  public class Setting_CameraClearFlags : Setting_Camera<CameraClearFlags>
  {
    public override string RuntimeName => "Clear Flags";

    public override string EditorName => base.EditorName + " (Builtin & URP)";

    public override string PropertyName => nameof(Camera.clearFlags);
  }

  public class Setting_ClearStencilAfterLightingPass : Setting_Camera<bool>
  {
    public override string PropertyName => nameof(Camera.clearStencilAfterLightingPass);

    public Setting_ClearStencilAfterLightingPass()
    {
      defaultValue = false;
    }
  }

  // TODO Add layer mask support via dropdown that supports multi select?
  //public class Setting_EventMask : Setting_Camera<int>
  //{
  //  public override string PropertyName => nameof(Camera.eventMask);
  //}

  public class Setting_IsPhysical : Setting_Camera<bool>
  {
    public override string RuntimeName => "Use Physical Camera Properties";

    public override string PropertyName => nameof(Camera.usePhysicalProperties);

    public Setting_IsPhysical()
    {
      defaultValue = false;
    }
  }

  // Requires enabled physcial properties
  public class Setting_FocalLength : Setting_Camera<float>
  {
    public override string PropertyName => nameof(Camera.focalLength);

    public Setting_FocalLength()
    {
      options.AddMinMaxRangeValues("0.1047227", "100");
      options.AddStepSize("0.001");

      defaultValue = 50;
    }
  }

  // Requires enabled physcial properties
  [DisplayName("Focus Distance (2022+)")]
  public class Setting_FocusDistance : Setting_Camera<float>
  {
    public override string EditorName => "Focus Distance (2022+)";

#if UNITY_2022_1_OR_NEWER
    public override string PropertyName => nameof(Camera.focusDistance);
#else
    public override string PropertyName => "focusDistance";
#endif

    public Setting_FocusDistance()
    {
      options.AddMinMaxRangeValues("0.1", "100");
      options.AddStepSize("0.1");

      defaultValue = 10;
    }
  }

  #region Unused Physical Camera Settings
  // Requires enabled physcial properties
  //public class Setting_PhysicalCameraISO : Setting_Camera<int>
  //{
  //  public override string PropertyName => nameof(Camera.iso);
  //}

  //// Requires enabled physcial properties
  //public class Setting_PhysicalCameraShutterSpeed : Setting_Camera<float>
  //{
  //  public override string PropertyName => nameof(Camera.shutterSpeed);
  //}

  //// Requires enabled physcial properties
  //public class Setting_PhysicalCameraAperture : Setting_Camera<float>
  //{
  //  public override string PropertyName => nameof(Camera.aperture);
  //}

  //// Requires enabled physcial properties
  //public class Setting_PhysicalCameraBladeCount : Setting_Camera<int>
  //{
  //  public override string PropertyName => nameof(Camera.bladeCount);
  //}

  //// Requires enabled physcial properties
  //public class Setting_PhysicalCameraBarrelClipping : Setting_Camera<float>
  //{
  //  public override string PropertyName => nameof(Camera.barrelClipping);
  //}

  //// Requires enabled physcial properties
  //public class Setting_PhysicalCameraAnamorphism : Setting_Camera<float>
  //{
  //  public override string PropertyName => nameof(Camera.anamorphism);
  //}
  #endregion
}