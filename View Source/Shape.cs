using System;
using System.IO;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;

namespace ViewSource
{
    public class Shape
    {
        private readonly float iRadius;
        private readonly float iWidth;
        private readonly float iDepth;

        public Shape(float aRadius, float aWidth, float aDepth)
        {
            iRadius = aRadius;
            iWidth = aWidth;
            iDepth = aDepth;
        }

        public float Radius => iRadius;
        public float Width => iWidth;
        public float Depth => iDepth;
    }
}