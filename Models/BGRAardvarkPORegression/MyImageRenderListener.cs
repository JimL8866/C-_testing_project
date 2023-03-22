using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;

namespace CTGAUAardQuadTest.Models.BGRAardvarkPORegression
{
    public class MyImageRenderListener : IEventListener
    {
        private List<Vector> _imageCoordinates { get; set;  }

        public MyImageRenderListener(List<Vector> imageCoordinates)
        {
            this._imageCoordinates = imageCoordinates;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            switch (type)
            {
                case EventType.RENDER_IMAGE:
                    ImageRenderInfo renderInfo = (ImageRenderInfo) data;
                    this._imageCoordinates.Add(renderInfo.GetStartPoint());
                    break;



                default:
                    break;
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
                return null;
        }
    }
}