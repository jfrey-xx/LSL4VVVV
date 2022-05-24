﻿using System;
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
        public void pullLast(out float[] sample, out double timestamp)
        {
            connect();
            float[] newSample = new float[nbChannels];
            double newTimestamp = -1;
            if (isInit)
            {
                // 0 timeout
                newTimestamp = inlet.pull_sample(newSample, 0);
            }
            sample = newSample;
            timestamp = newTimestamp;
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