using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VisualNovelNodeNetwork
{
    /// <summary>
    /// This is to give a preview of the slected node without causing too much overhead during the DragOver event
    /// (Dragging a node in real time can cause some undefined behavior and sometimes the node's Drop event gets ignored)
    /// </summary>
    public class DragNodePreviewAdorner : Adorner
    {
        private Point _zoomedMousePoint;
        private double _currentScale = 1.0;
        private readonly Size _size;

        public DragNodePreviewAdorner(UIElement adornedElement, Size size) : base(adornedElement)
        {
            _size = size;
            IsHitTestVisible = false;
        }

        public void UpdatePosition(Point zoomedPoint, double scale)
        {
            _zoomedMousePoint = zoomedPoint;
            _currentScale = scale;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double scaledWidth = _size.Width * _currentScale;
            double scaledHeight = _size.Height * _currentScale;

            Point offset = new Point(_zoomedMousePoint.X - (scaledWidth * 0.5), _zoomedMousePoint.Y - (scaledHeight * 0.5));

            Rect rect = new Rect(offset, new Size(scaledWidth, scaledHeight));
            Brush brush = new SolidColorBrush(Color.FromArgb(100, 0, 120, 215)); // Semi-transparent blue
            Pen pen = new Pen(Brushes.DodgerBlue, 1.5 * _currentScale);
            drawingContext.DrawRectangle(brush, pen, rect);
        }
    }
}
