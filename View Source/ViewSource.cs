using System;
using System.IO;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;

namespace ViewSource
{
    public class ViewSource
    {
        private string iInputFilename;
        private string iOutputFilename;
        private float[][] iSamples;
        private int iWidth;
        private int iHeight;
        private int iShapeCount;

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

            iInputFilename = aArgs[0];

            iOutputFilename = CreateOutputFilename(iInputFilename);

            iSamples = LoadStereoSamples(iInputFilename, "Unable to read audio file");

            iWidth = ProcessValue(aArgs[1], "Invalid width specified");

            iHeight = ProcessValue(aArgs[2], "Invalid height specified");

            iShapeCount = iWidth * iHeight;

            var partitions = PartitionArray(iSamples, iShapeCount);

            var shapeIndexes = CalculateShapeIndexesFromPartitions(partitions);
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

        private int[][] CalculateShapeIndexesFromPartitions(float[][][] aPartitions)
        {
            var shapeIndexes = new List<int[]>();

            foreach (var partition in aPartitions)
            {
                shapeIndexes.Add(CalculateShapeIndexesFromPartition(partition).ToArray());
            }

            return shapeIndexes.ToArray();
        }

        private IEnumerable<int> CalculateShapeIndexesFromPartition(float[][] aPartition)
        {
            var left = aPartition.Select(v => v.ElementAt(0));
            var right = aPartition.Select(v => v.ElementAt(1));

            yield return CalculateShapeIndexFromSamples(left);
            yield return CalculateShapeIndexFromSamples(right);
        }

        private int CalculateShapeIndexFromSamples(IEnumerable<float> aSamples)
        {
            float total = 0;

            foreach (var sample in aSamples)
            {
                var abs = sample > 0 ? sample : -sample;
                total += abs;
            }

            return (int)total % 128;
        }
    }
}