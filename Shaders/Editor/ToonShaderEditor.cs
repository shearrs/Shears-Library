using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shears.Shaders.Editor
{
    public class ToonShaderEditor : ShaderGUI
    {
        private MaterialEditor materialEditor;
        private MaterialProperty[] properties;

        private enum RendererType
        {
            Mesh = 0,
            Sprite = 1,
        };

        private enum SurfaceType
        {
            Opaque = 0,
            Transparent = 1,
        };

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            this.materialEditor = materialEditor;
            this.properties = properties;

            var alphaClipProp = FindProperty("_ALPHA_CLIP");
            var rimLightingProp = FindProperty("_RIM_LIGHTING");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Color Settings", EditorStyles.boldLabel);
            ShaderProperty("_Color");
            ShaderProperty("_MainTex");
            ShaderProperty(alphaClipProp);

            if (alphaClipProp.intValue > 0)
                ShaderProperty("_AlphaClipThreshold");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Lighting Settings", EditorStyles.boldLabel);
            ShaderProperty("_ColorBands");
            ShaderProperty("_Smoothness");
            ShaderProperty("_RECEIVE_SHADOWS");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Rim Lighting Settings", EditorStyles.boldLabel);
            ShaderProperty(rimLightingProp);

            if (rimLightingProp.intValue > 0)
            {
                ShaderProperty("_RimLightRadius");
                ShaderProperty("_RimLightSmoothness");
                ShaderProperty("_RimLightStrength");
                ShaderProperty("_RimLightCutoff");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Rendering Settings", EditorStyles.boldLabel);
            ShaderProperty("_Cull");

            var surfaceProp = FindProperty("_SurfaceType");

            var currentSurfaceType = (SurfaceType)surfaceProp.intValue;
            currentSurfaceType = (SurfaceType)
                EditorGUILayout.EnumPopup("Surface Type", currentSurfaceType);

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Surface Type");
                surfaceProp.intValue = (int)currentSurfaceType;

                foreach (var target in materialEditor.targets)
                {
                    var mat = (Material)target;
                    SetBlendMode(mat, currentSurfaceType, alphaClipProp.intValue > 0);
                }
            }
        }

        private void SetBlendMode(Material mat, SurfaceType type, bool alphaClip)
        {
            var srcProp = FindProperty("_SrcBlend");
            var dstProp = FindProperty("_DstBlend");
            var zWriteProp = FindProperty("_ZWrite");

            switch (type)
            {
                case SurfaceType.Opaque:
                    if (alphaClip)
                    {
                        srcProp.intValue = (int)BlendMode.One;
                        dstProp.intValue = (int)BlendMode.Zero;
                        zWriteProp.intValue = 1;
                        mat.renderQueue = (int)RenderQueue.AlphaTest;
                        mat.SetOverrideTag("RenderType", "AlphaTest");
                    }
                    else
                    {
                        srcProp.intValue = (int)BlendMode.One;
                        dstProp.intValue = (int)BlendMode.Zero;
                        zWriteProp.intValue = 1;
                        mat.renderQueue = (int)RenderQueue.Geometry;
                        mat.SetOverrideTag("RenderType", "Opaque");
                    }

                    break;
                case SurfaceType.Transparent:
                    srcProp.intValue = (int)BlendMode.SrcAlpha;
                    dstProp.intValue = (int)BlendMode.OneMinusSrcAlpha;
                    zWriteProp.intValue = 0;
                    mat.renderQueue = (int)RenderQueue.Transparent;
                    mat.SetOverrideTag("RenderType", "Transparent");

                    break;
            }
        }

        private void ShaderProperty(string identifier, string name = null)
        {
            ShaderProperty(FindProperty(identifier), name);
        }

        private void ShaderProperty(MaterialProperty property, string name = null)
        {
            materialEditor.ShaderProperty(property, name ?? property.displayName);
        }

        private MaterialProperty FindProperty(string name)
        {
            return FindProperty(name, properties);
        }
    }
}
