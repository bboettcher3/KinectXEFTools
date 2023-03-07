using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KinectXEFTools;

namespace XEFExtract
{
    class XEFDepthWriter : IXEFDataWriter, IDisposable
    {
        //
        //  Members
        //

        private uint _depthframesize = NuiConstants.STREAM_DEPTH_WIDTH * NuiConstants.STREAM_DEPTH_HEIGHT * 2; // 16bpp
        private long _depthStartTime = -1;
        private StreamWriter _writer = null;
        private bool _seenEvent = false;

        //
        //  Properties
        //

        public string FilePath { get; private set; }

        public long EventCount { get; private set; }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public TimeSpan Duration { get { return EndTime - StartTime; } }
        
        //
        //  Constructor
        //

        public XEFDepthWriter(string path)
        {
            FilePath = path;
            EventCount = 0;
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;

            _writer = new StreamWriter(path);
            WriteHeaders();
        }

        ~XEFDepthWriter()
        {
            Dispose(false);
        }

        //
        //	IDisposable
        //

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    UpdateFrameCount();

                    // Dispose managed resources
                    _writer.Dispose();
                }

                disposed = true;
            }
        }
        
        //
        //  Methods
        //

        private void WriteHeaders()
        {
            _writer.WriteLine("EventIndex,Time");
            // Write initial headers
            //_writer.Write(EventCount); // 8 bytes -- RESERVED (this is updated after enumerating through all the frames)
            //_writer.Write(NuiConstants.STREAM_DEPTH_WIDTH); // 4 bytes
            //_writer.Write(NuiConstants.STREAM_DEPTH_HEIGHT); // 4 bytes
            //_writer.Write(_depthframesize); // 4 bytes
        }

        private void UpdateFrameCount()
        {
            //Console.WriteLine("Depth Frames: " + depthframecount);

            // Finish writing depth file
            //_writer.Seek(0, SeekOrigin.Begin);
            //_writer.Write(EventCount); // Write depth frame count -- seek back to reserved location
            //_writer.Seek(0, SeekOrigin.End); // Reset to end
        }

        public void Close()
        {
            Dispose(true);
        }

        public void ProcessEvent(XEFEvent ev)
        {
            if (ev.EventStreamDataTypeId != StreamDataTypeIds.Depth)
            {
                return;
            }

            // Update start/end time
            if (!_seenEvent)
            {
                StartTime = ev.RelativeTime;
                _seenEvent = true;
            }
            EndTime = ev.RelativeTime;

            // Get frame time
            long frameTime = ev.RelativeTime.Ticks;
            if (_depthStartTime < 0) _depthStartTime = frameTime;
            frameTime -= _depthStartTime;

            // Write index and time
            _writer.Write("{0},{1}",
            ev.EventIndex,
            ev.RelativeTime.Ticks);

            // Compute scaled depth values
            byte[] frameData = ev.EventData;
            int max = 0;
            for (int i = 0; i < frameData.Length; i += 2)
            {
                int depth = frameData[i] | frameData[i + 1] << 8;
                if (depth > max) max = depth;
            }
            for (int i = 0; i < frameData.Length; i += 2)
            {
                int depth = frameData[i] | frameData[i + 1] << 8;
                depth = (int)(((float)depth / max) * 255);
                _writer.Write(",{0}", depth);
            }

            // Write to stream file
            //depthWriter.Write(frameTime); // 8 bytes
            //_writer.Write(EventCount);
            //_writer.Write(ev.EventData);
            _writer.WriteLine();

            EventCount++;
        }
    }
}
