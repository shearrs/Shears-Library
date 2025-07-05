using Shears.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class GridBackground : VisualElement
    {
        private static readonly CustomStylePropertyDefault<float> SpacingProperty = new("--spacing", 50f);
        private static readonly CustomStylePropertyDefault<int> LineWidthProperty = new("--line-width", 1);
        private static readonly CustomStylePropertyDefault<int> ThickLineWidthProperty = new("--thick-line-width", 5);
        private static readonly CustomStylePropertyDefault<int> ThickLineIntervalProperty = new("--thick-line-interval", 5);
        private static readonly CustomStylePropertyDefault<Color> LineColorProperty = new("--line-color", new Color(0f, 0f, 0f, 0.18f));
        private static readonly CustomStylePropertyDefault<Color> ThickLineColorProperty = new("--thick-line-color", new Color(0f, 0f, 0f, 0.38f));

        private readonly GraphView graphView;

        private float spacing;
        private int lineWidth;
        private int thickLineWidth;
        private int thickLineInterval;
        private Color lineColor;
        private Color thickLineColor;

        private Vector3 viewPos;
        private Quaternion viewRot;
        private Vector3 viewScale;

        public GridBackground(GraphView graphView)
        {
            this.graphView = graphView;

            pickingMode = PickingMode.Ignore;
            AddToClassList("gridBackground");

            generateVisualContent += GenerateVisualContent;

            schedule.Execute(() =>
            {
                if (HasViewportChanged())
                {
                    UpdateGrid();
                    MarkDirtyRepaint();
                }
            }).Every(1);
        }

        private bool HasViewportChanged()
        {
            ITransform transform = graphView.ViewTransform;

            return viewPos != transform.position || viewRot != transform.rotation || viewScale != transform.scale;
        }

        private void UpdateGrid()
        {
            var transform = graphView.ViewTransform;

            viewPos = transform.position;
            viewRot = transform.rotation;
            viewScale = transform.scale;
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            VisualElement viewContainer = graphView.ContentViewContainer;

            float spacing = Mathf.Max(1, this.spacing);
            float thickSpacing = spacing * thickLineInterval;

            Vector2 topLeft = this.ChangeCoordinatesTo(viewContainer, Vector2.zero);
            topLeft -= new Vector2(topLeft.x % spacing, topLeft.y % spacing) + new Vector2(spacing, spacing);

            Vector2 topLeftThick = topLeft;
            topLeftThick -= new Vector2(topLeft.x % thickSpacing, topLeft.y % thickSpacing) + new Vector2(thickSpacing, thickSpacing);

            topLeft = viewContainer.ChangeCoordinatesTo(this, topLeft);
            topLeftThick = viewContainer.ChangeCoordinatesTo(this, topLeftThick);

            Vector2 scaledSpacing = spacing * new Vector2(viewContainer.transform.scale.x, viewContainer.transform.scale.y);
            float maxWidth = resolvedStyle.width;
            float maxHeight = resolvedStyle.height;

            var painter = context.painter2D;

            // Thin Lines
            painter.BeginPath();

            CreateVerticalLines(painter, topLeft, maxWidth, maxHeight, scaledSpacing.x);
            CreateHorizontalLines(painter, topLeft, maxWidth, maxHeight, scaledSpacing.y);

            DrawLines(painter, lineWidth, lineColor);

            // Thick Lines
            scaledSpacing *= thickLineInterval;
            painter.BeginPath();

            CreateVerticalLines(painter, topLeftThick, maxWidth, maxHeight, scaledSpacing.x);
            CreateHorizontalLines(painter, topLeftThick, maxWidth, maxHeight, scaledSpacing.y);

            DrawLines(painter, thickLineWidth, thickLineColor);
        }

        private void CreateVerticalLines(Painter2D painter, Vector2 startPos, float maxWidth, float maxHeight, float spacing)
        {
            Vector2 currentPos = startPos;

            while (currentPos.x < maxWidth)
            {
                painter.MoveTo(currentPos);
                painter.LineTo(new(currentPos.x, maxHeight));

                currentPos.x += spacing;
            }
        }

        private void CreateHorizontalLines(Painter2D painter, Vector2 startPos, float maxWidth, float maxHeight, float spacing)
        {
            Vector2 currentPos = startPos;

            while (currentPos.y < maxHeight)
            {
                painter.MoveTo(currentPos);
                painter.LineTo(new(maxWidth, currentPos.y));

                currentPos.y += spacing;
            }
        }

        private void DrawLines(Painter2D painter, int lineWidth, Color color)
        {
            painter.lineWidth = lineWidth * graphView.ViewTransform.scale.x;
            painter.strokeColor = color;
            painter.Stroke();
        }

        [EventInterest(EventInterestOptions.Inherit)]
        [EventInterest(typeof(CustomStyleResolvedEvent))]
        protected override void HandleEventTrickleDown(EventBase evt)
        {
            base.HandleEventTrickleDown(evt);

            if (evt.eventTypeId == CustomStyleResolvedEvent.TypeId())
            {
                var styleEvt = (CustomStyleResolvedEvent)evt;
                ICustomStyle style = styleEvt.customStyle;

                spacing = SpacingProperty.GetValueOrDefault(style);
                lineWidth = LineWidthProperty.GetValueOrDefault(style);
                thickLineWidth = ThickLineWidthProperty.GetValueOrDefault(style);
                thickLineInterval = ThickLineIntervalProperty.GetValueOrDefault(style);
                lineColor = LineColorProperty.GetValueOrDefault(style);
                thickLineColor = ThickLineColorProperty.GetValueOrDefault(style);
            }
        }
    }
}
