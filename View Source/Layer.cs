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

        public virtual void Write(Shape aShape, Point aPosition, int aChannel)
        {
            // write paths or mid-cricle

            var circle = new SvgCircle();

            var radius = aShape.Radius - aShape.Width / 2;

            circle.Fill = SvgPaintServer.None;
            circle.CenterX = new SvgUnit(SvgUnitType.Millimeter, aPosition.X);
            circle.Radius = new SvgUnit(SvgUnitType.Millimeter, radius);

            if (aChannel == 0)
            {
                circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y - aShape.Radius - 1);
            }
            else
            {
                circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y + aShape.Radius + 1);
            }

            circle.Stroke = new SvgColourServer(System.Drawing.Color.Black);
            circle.StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, aShape.Width);

            iGroup.Children.Add(circle);
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
                // write paths for the outer circle

                var circle = new SvgCircle();

                circle.Fill = SvgPaintServer.None;
                circle.CenterX = new SvgUnit(SvgUnitType.Millimeter, aPosition.X);
                circle.Radius = new SvgUnit(SvgUnitType.Millimeter, aShape.Radius);

                if (aChannel == 0)
                {
                    circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y - aShape.Radius - 1);
                }
                else
                {
                    circle.CenterY = new SvgUnit(SvgUnitType.Millimeter, aPosition.Y + aShape.Radius + 1);
                }

                iGroup.Children.Add(circle);
            }
        }
    }
}