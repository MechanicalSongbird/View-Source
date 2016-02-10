using System;
using System.IO;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;

using Svg;

namespace ViewSource
{
    public class ViewSource
    {
        const float marginX = 100F;
        const float marginY = 100F;
        const float cellWidth = 47F;  // 1 * 45 + 2 inter-circle gap
        const float cellHeight = 97F; // 2 * 45 + 2 inter-circle gap + 5 inter line gap
        //const float cellWidth = 53F;  // 1 * 51 + 2 inter-circle gap
        //const float cellHeight = 109F; // 2 * 51 + 2 inter-circle gap + 5 inter line gap

        public ViewSource()
        {
        }

        public void Run(string[] aArgs)
        {
            ProcessArguments(aArgs);
        }

        private void ProcessArguments(string[] aArgs)
        {
            if (aArgs.Length < 1)
            {
                throw new Exception("Filename not specified");
            }

            if (aArgs.Length < 2)
            {
                throw new Exception("Width not specified");
            }

            if (aArgs.Length < 3)
            {
                throw new Exception("Depth not specified");
            }

            var inputFilename = aArgs[0];

            var outputFilename = CreateOutputFilename(inputFilename);

            var samples = LoadStereoSamples(inputFilename, "Unable to read audio file");

            var width = ProcessValue(aArgs[1], "Invalid width specified");

            var height = ProcessValue(aArgs[2], "Invalid height specified");

            var shapeCount = width * height;

            var partitions = PartitionArray(samples, shapeCount);

            var shapeIndexes = CalculateShapeIndexesFromPartitions(partitions);

            var layers = CreateLayers();

            var shapes = CreateShapes();

            var shapePositions = CreateShapePositions(width, height);

            var enumerator = shapePositions.AsEnumerable().GetEnumerator();

            foreach (var entry in shapeIndexes)
            {
                enumerator.MoveNext();

                WriteShapes(entry, enumerator.Current, layers, shapes);
            }

            var svg = new SvgDocument();

            svg.X = 0;
            svg.Y = 0;
            svg.Width = new SvgUnit(SvgUnitType.Millimeter, width * cellWidth + 2 * marginX);
            svg.Height = new SvgUnit(SvgUnitType.Millimeter, height * cellHeight + 2 * marginY);

            foreach (var layer in layers)
            {
                svg.Children.Add(layer.Group);
            }

            svg.Write(outputFilename);
        }

        private static void WriteShapes(int[] aShapeIndexes, Point aPosition, Layer[] aLayers, Shape[] aShapes)
        {
            int channel = 0;

            foreach (var entry in aShapeIndexes)
            {
                WriteShape(entry, aPosition, channel++, aLayers, aShapes);
            }
        }

        private static void WriteShape(int aShapeIndex, Point aPosition, int aChannel, Layer[] aLayers, Shape[] aShapes)
        {
            foreach (var layer in aLayers)
            {
                layer.Write(aShapes[aShapeIndex], aPosition, aChannel);
            }
        }

        private static int ProcessValue(string aValue, string aError)
        {
            int value;

            if (int.TryParse(aValue, out value))
            {
                if (value > 0)
                {
                    return value;
                }
            }

            throw new Exception(aError);
        }

        private string CreateOutputFilename(string aFilename)
        {
            return aFilename + ".svg";
        }

        private static byte[] ReadBytes(string aFilename, string aError)
        {
            try
            {
                return File.ReadAllBytes(aFilename);
            }
            catch
            {
                throw new Exception(aError);
            }
        }

        private static float[][] LoadStereoSamples(string aFilename, string aError)
        {
            try
            {
                using (var reader = new WaveFileReader(aFilename))
                {
                    var frames = new List<float[]>();

                    while (true)
                    {
                        var frame = reader.ReadNextSampleFrame();

                        if (frame == null)
                        {
                            return frames.ToArray();
                        }

                        if (frame.Length != 2)
                        {
                            break;
                        }

                        frames.Add(frame);
                    }
                }
            }
            catch
            {
            }

            throw new Exception(aError);
        }

        private static T[][] PartitionArray<T>(T[] aArray, int aCount)
        {
            var partitionEntries = aArray.Length / aCount;

            while (partitionEntries * aCount < aArray.Length)
            {
                partitionEntries++;
            }

            var index = 0;

            var remaining = aArray.Length;

            var partitions = new List<T[]>();

            while (remaining > 0)
            {
                var entries = partitionEntries;

                if (entries > remaining)
                {
                    entries = remaining;
                }

                var partition = new T[entries];

                Array.Copy(aArray, index, partition, 0, entries);

                partitions.Add(partition);

                index += entries;

                remaining -= entries;
            }

            return partitions.ToArray();
        }

        private static int[][] CalculateShapeIndexesFromPartitions(float[][][] aPartitions)
        {
            var shapeIndexes = new List<int[]>();

            foreach (var partition in aPartitions)
            {
                shapeIndexes.Add(CalculateShapeIndexesFromPartition(partition).ToArray());
            }

            return shapeIndexes.ToArray();
        }

        private static IEnumerable<int> CalculateShapeIndexesFromPartition(float[][] aPartition)
        {
            var left = aPartition.Select(v => v.ElementAt(0));
            var right = aPartition.Select(v => v.ElementAt(1));

            yield return CalculateShapeIndexFromSamples(left);
            yield return CalculateShapeIndexFromSamples(right);
        }

        private static int CalculateShapeIndexFromSamples(IEnumerable<float> aSamples)
        {
            float total = 0;

            foreach (var sample in aSamples)
            {
                var abs = sample > 0 ? sample : -sample;
                total += abs;
            }

            return (int)total % 128;
        }

        private static Point[] CreateShapePositions(int aWidth, int aHeight)
        {
            var positions = new List<Point>();

            float y = marginY + cellHeight / 2;

            for (int county = 0; county < aHeight; county++)
            {
                float x = marginX + cellWidth / 2;

                for (int countx = 0; countx < aWidth; countx++)
                {
                    positions.Add(new Point(x, y));

                    x += cellWidth;
                }

                y += cellHeight;
            }

            return positions.ToArray();
        }

        private static Layer[] CreateLayers()
        {
            var widths = new[] { 1.5F, 2.5F, 3.0F, 6.0F };
            var depths = new[] { 1F, 3F, 5F, 7F };

            var layers = new List<Layer>();

            foreach (var width in widths)
            {
                foreach (var depth in depths)
                {
                    layers.Add(new LayerDrill(width, depth));
                }
            }

            layers.Add(new Layer()); // the master layer

            return layers.ToArray();
        }

        private static Shape[] CreateShapes()
        {
            // 8 Circle size: 45mm, 40mm, 35mm, 30mm, 25mm, 20mm, 15mm, 10mm
            // 4 Drill bit widths - line widths; 1.5mm, 2.5mm, 3mm, 6mm
            // 4 Drill depths; 7mm, 5mm, 3mm, 1mm

            var radii = new [] { 22.5F, 20F, 17.5F, 15F, 22.5F, 10F, 7.5F, 5F };
            var widths = new [] { 1.5F, 2.5F, 3.0F, 6.0F };
            var depths = new [] { 1F, 3F, 5F, 7F };

            var shapes = new List<Shape>();

            foreach (var width in widths)
            {
                foreach (var depth in depths)
                {
                    foreach (var radius in radii)
                    {
                        shapes.Add(new Shape(radius, width, depth));
                    }
                }
            }

            return shapes.ToArray();
        }
    }
}