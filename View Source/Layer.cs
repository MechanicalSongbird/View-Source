using Svg;

namespace ViewSource
{
    public class Layer
    {
        protected readonly SvgGroup iGroup;

        public Layer()
        {
            iGroup = new SvgGroup();
            iGroup.ID = "visual";
        }

        public SvgGroup Group => iGroup;

        protected void Write(float aRadius, float aWidth, float aStroke, Point aPosition, int aChannel)
        {
            var circle = new SvgCircle();

            circle.Fill = SvgPaintServer.None;
            circle.CenterX = new SvgUnit(SvgUnitType.Millimeter, aPosition.X);
            circle.Radius = new SvgUnit(SvgUnitType.Millimeter, aRadius);

            if (aChannel == 0)
            {
                circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y - aRadius - aWidth / 2);
            }
            else
            {
                circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y + aRadius + aWidth / 2);
            }

            if (aStroke != 0)
            {
                circle.Stroke = new SvgColourServer(System.Drawing.Color.Black);
                circle.StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, aStroke);
            }

            iGroup.Children.Add(circle);
        }

        public virtual void Write(Shape aShape, Point aPosition, int aChannel)
        {
            Write(aShape.Radius, aShape.Width, aShape.Width, aPosition, aChannel);
        }
    }

    public class LayerDrill : Layer
    {
        private readonly float iWidth;
        private readonly float iDepth;

        public LayerDrill(float aWidth, float aDepth)
        {
            iWidth = aWidth;
            iDepth = aDepth;
            iGroup.ID = string.Format("drill width {0}, depth {1}", iWidth, iDepth);
        }

        public override void Write(Shape aShape, Point aPosition, int aChannel)
        {
            if (aShape.Width == iWidth && aShape.Depth == iDepth)
            {
                Write(aShape.Radius, aShape.Width, 0, aPosition, aChannel);
            }
        }
    }
}