using System;
using System.Collections.Generic;
using LSL;

namespace LSL4VVVV
{ 
    public class LSLInlet
    {
        // connexion on first loop
        private bool isInit = false;

        // we'll handle only one stream at the moment
        private LSL.StreamInlet inlet;

        private String name;
        private String type;
        private int nbChannels = 0;
        private int maxBufLen;

        // keeping an eye on last values seen
        private float[] lastSample;
        private double lastTimestamp;

        // FIXME: can only deal with Floats
        // TODO: catch disconnect
        public LSLInlet(String name, String type, int maxBufLen = 360)
        {
            this.name = name;
            this.type = type;
            this.maxBufLen = maxBufLen;
            connect();
        }

        private void connect()
        {
            if (!isInit)
            {
                String pred = "type = '" + this.type + "' and name = '" + this.name + "'";
                Console.WriteLine("connect");
                LSL.StreamInfo[] results = LSL.LSL.resolve_stream(pred, 1, 1);
                Console.WriteLine("Number of streams: " + results.Length);
                // retrieve arbitrarely the first result
                if (results.Length > 0)
                {
                    inlet = new LSL.StreamInlet(results[0], maxBufLen);
                    nbChannels = inlet.info().channel_count();
                    Console.WriteLine("Pick the first stream, with " + nbChannels + " channels.");
                    isInit = true;
                }
            }
        }

        // stupid function, pull last sample from input buffer
        public void pullLast(out IEnumerable <float> sample, out double timestamp, out bool updated)
        {
            connect();
            float[] newSample = new float[nbChannels];
            double newTimestamp = 0;
            bool gotNewData = false;
            if (isInit)
            {
                // we gonna fech all valies, using 0 timeout
                newTimestamp = inlet.pull_sample(newSample, 0);
                // from the lib, new sample if timestamp greater than 0
                while (newTimestamp > 0)
                {
                    gotNewData = true;
                    lastSample = newSample;
                    lastTimestamp = newTimestamp;
                    newTimestamp = inlet.pull_sample(newSample, 0);
                }
            }
            sample = lastSample;
            timestamp = lastTimestamp;
            updated = gotNewData;
        }

        static public LSLInlet Init(LSLInlet target, String name, String type, int maxBufLen = 360)
        {
            if (target == null)
            {
                target = new LSLInlet(name, type, maxBufLen);
            }
            return target;
        }
    }
}
