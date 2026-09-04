using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shears.Shaders
{
    [Serializable]
    public class OutlineGroup
    {
        private const string IGNORE_DEPTH_KEYWORD = "IGNORE_DEPTH";
        private const string SECOND_COLOR_KEYWORD = "SECOND_COLOR";
        private const string THIRD_COLOR_KEYWORD = "THIRD_COLOR";
        private static readonly int STENCIL_ID = Shader.PropertyToID("_StencilRef");
        private static readonly int Z_TEST_ID = Shader.PropertyToID("_ZTest");
        private static readonly int FIRST_COLOR_ID = Shader.PropertyToID("_FirstOutlineColor");
        private static readonly int SECOND_COLOR_ID = Shader.PropertyToID("_SecondOutlineColor");
        private static readonly int THIRD_COLOR_ID = Shader.PropertyToID("_ThirdOutlineColor");
        private static readonly int OUTLINE_WIDTH_ID = Shader.PropertyToID("_OutlineWidth");
        private static readonly int OUTLINE_HARDNESS_ID = Shader.PropertyToID("_OutlineHardness");

        [Header("Render Targets")]
        [SerializeField]
        private RenderPassEvent _injectionPoint;

        [SerializeField]
        private LayerMask _layerMask;

        [SerializeField]
        private RenderingLayerMask _renderLayerMask = RenderingLayerMask.defaultRenderingLayerMask;

        [Header("Depth")]
        [SerializeField, Range(0, 255)]
        private int _stencil;

        [SerializeField]
        private CompareFunction _zTest;

        [SerializeField]
        private bool _ignoreDepthForWidth;

        [Header("Line Settings")]
        [SerializeField]
        private float _outlineWidth;

        [SerializeField, Range(0, 1)]
        private float _outlineHardness;

        [Header("Color Settings")]
        [SerializeField, Range(1, 3)]
        private int _colorCount = 1;

        [SerializeField]
        private Color _firstOutlineColor;

        [SerializeField]
        private Color _secondOutlineColor;

        [SerializeField]
        private Color _thirdOutlineColor;

        [NonSerialized]
        private int _previousStencil;

        [NonSerialized]
        private CompareFunction _previousZTest;

        [NonSerialized]
        private bool _previousIgnoreDepthForWidth;

        [NonSerialized]
        private float _previousOutlineWidth;

        [NonSerialized]
        private float _previousOutlineHardness;

        [NonSerialized]
        private Color _previousFirstColor;

        [NonSerialized]
        private Color? _previousSecondColor;

        [NonSerialized]
        private Color? _previousThirdColor;

        private Material _whiteMaskMaterial;
        private Material _jumpFloodMaterial;

        private LocalKeyword IgnoreDepthKeyword => new(JumpFloodShader, IGNORE_DEPTH_KEYWORD);
        private LocalKeyword SecondColorKeyword => new(JumpFloodShader, SECOND_COLOR_KEYWORD);
        private LocalKeyword ThirdColorKeyword => new(JumpFloodShader, THIRD_COLOR_KEYWORD);
        internal Shader WhiteMaskShader { get; set; }
        internal Shader JumpFloodShader { get; set; }
        internal Material WhiteMaskMaterial => _whiteMaskMaterial;
        internal Material JumpFloodMaterial => _jumpFloodMaterial;
        public int Stencil
        {
            get => _stencil;
            set => _stencil = Mathf.Clamp(value, 0, 255);
        }
        public CompareFunction ZTest
        {
            get => _zTest;
            set => _zTest = value;
        }
        public float OutlineWidth
        {
            get => _outlineWidth;
            set => _outlineWidth = Mathf.Max(0, value);
        }
        public float OutlineHardness
        {
            get => _outlineHardness;
            set => _outlineHardness = Mathf.Clamp01(value);
        }
        public Color OutlineColor
        {
            get => _firstOutlineColor;
            set => _firstOutlineColor = value;
        }
        public RenderPassEvent InjectionPoint
        {
            get => _injectionPoint;
            set => _injectionPoint = value;
        }
        public LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }
        public RenderingLayerMask RenderLayerMask
        {
            get => _renderLayerMask;
            set => _renderLayerMask = value;
        }

        public OutlineGroup(
            int stencil = 1,
            CompareFunction zTest = CompareFunction.LessEqual,
            float outlineWidth = 24.0f,
            float outlineHardness = 1.0f,
            Color? outlineColor = null,
            LayerMask? layerMask = null,
            RenderingLayerMask? renderLayerMask = null
        )
        {
            _stencil = stencil;
            _zTest = zTest;
            _outlineWidth = outlineWidth;
            _outlineHardness = outlineHardness;
            _firstOutlineColor = outlineColor ?? Color.white;
            _layerMask = layerMask ?? -1;
            _renderLayerMask = renderLayerMask ?? RenderingLayerMask.defaultRenderingLayerMask;
        }

        public void InitializeDefaultValues()
        {
            _stencil = 1;
            _zTest = CompareFunction.LessEqual;
            _ignoreDepthForWidth = false;
            _outlineWidth = 24.0f;
            _outlineHardness = 1.0f;
            _colorCount = 1;
            _firstOutlineColor = Color.white;
            _secondOutlineColor = Color.white;
            _thirdOutlineColor = Color.white;
            _injectionPoint = RenderPassEvent.AfterRenderingTransparents;
            _layerMask = -1;
            _renderLayerMask = RenderingLayerMask.defaultRenderingLayerMask;
        }

        public void Dispose()
        {
            if (WhiteMaskMaterial != null)
                CoreUtils.Destroy(WhiteMaskMaterial);

            if (JumpFloodMaterial != null)
                CoreUtils.Destroy(JumpFloodMaterial);
        }

        public void UpdateMaterialProperties(CameraData cameraData)
        {
            bool maskChanged = ReallocateMaterialIfNeeded(ref _whiteMaskMaterial, WhiteMaskShader);
            bool floodChanged = ReallocateMaterialIfNeeded(ref _jumpFloodMaterial, JumpFloodShader);

            if (_whiteMaskMaterial == null || _jumpFloodMaterial == null)
                return;

            if (maskChanged || floodChanged)
                ResetTrackers();

            if (_stencil != _previousStencil)
            {
                _previousStencil = _stencil;
                WhiteMaskMaterial.SetInteger(STENCIL_ID, _stencil);
                JumpFloodMaterial.SetInteger(STENCIL_ID, _stencil);
            }

            if (_zTest != _previousZTest)
            {
                _previousZTest = _zTest;
                WhiteMaskMaterial.SetInteger(Z_TEST_ID, (int)_zTest);
            }

            float renderScale = cameraData.renderScale;
            float outlineWidth = _outlineWidth * renderScale;

            if (outlineWidth != _previousOutlineWidth)
            {
                _previousOutlineWidth = outlineWidth;
                JumpFloodMaterial.SetFloat(OUTLINE_WIDTH_ID, outlineWidth);
            }

            if (_ignoreDepthForWidth != _previousIgnoreDepthForWidth)
            {
                _previousIgnoreDepthForWidth = _ignoreDepthForWidth;
                JumpFloodMaterial.SetKeyword(IgnoreDepthKeyword, _ignoreDepthForWidth);
            }

            if (_firstOutlineColor != _previousFirstColor)
            {
                _previousFirstColor = _firstOutlineColor;
                JumpFloodMaterial.SetColor(FIRST_COLOR_ID, _firstOutlineColor);
            }

            if (!_previousThirdColor.HasValue && _colorCount == 3)
            {
                _jumpFloodMaterial.SetKeyword(SecondColorKeyword, false);
                _jumpFloodMaterial.SetKeyword(ThirdColorKeyword, true);
            }
            else if (
                (!_previousSecondColor.HasValue || _previousThirdColor.HasValue)
                && _colorCount == 2
            )
            {
                _jumpFloodMaterial.SetKeyword(ThirdColorKeyword, false);
                _jumpFloodMaterial.SetKeyword(SecondColorKeyword, true);
            }
            else if (_colorCount == 1)
            {
                if (_previousThirdColor.HasValue)
                    _jumpFloodMaterial.SetKeyword(ThirdColorKeyword, false);
                if (_previousSecondColor.HasValue)
                    _jumpFloodMaterial.SetKeyword(SecondColorKeyword, false);
            }

            if (_previousSecondColor.HasValue && _colorCount < 2)
                _previousSecondColor = null;
            else if (_previousSecondColor != _secondOutlineColor && _colorCount > 1)
            {
                _previousSecondColor = _secondOutlineColor;
                _jumpFloodMaterial.SetColor(SECOND_COLOR_ID, _secondOutlineColor);
            }

            if (_previousThirdColor.HasValue && _colorCount < 3)
                _previousThirdColor = null;
            else if (_previousThirdColor != _thirdOutlineColor && _colorCount > 2)
            {
                _previousThirdColor = _thirdOutlineColor;
                _jumpFloodMaterial.SetColor(THIRD_COLOR_ID, _thirdOutlineColor);
            }

            if (_outlineHardness != _previousOutlineHardness)
            {
                _previousOutlineHardness = _outlineHardness;
                JumpFloodMaterial.SetFloat(OUTLINE_HARDNESS_ID, _outlineHardness);
            }
        }

        public bool IsValid() => WhiteMaskMaterial != null && JumpFloodMaterial != null;

        private bool ReallocateMaterialIfNeeded(ref Material material, Shader shader)
        {
            bool materialChanged = false;

            if (material != null && material.shader != shader)
            {
                CoreUtils.Destroy(material);
                material = null;
                materialChanged = true;
            }

            if (material == null && shader != null)
            {
                material = CoreUtils.CreateEngineMaterial(shader);
                materialChanged = true;
            }

            return materialChanged;
        }

        private void ResetTrackers()
        {
            _previousStencil = -1;
            _previousZTest = CompareFunction.Disabled;
            _previousIgnoreDepthForWidth = false;
            _previousOutlineWidth = -1;
            _previousOutlineHardness = -1;
            _previousFirstColor = Color.clear;
            _previousSecondColor = null;
            _previousThirdColor = null;
        }
    }
}
