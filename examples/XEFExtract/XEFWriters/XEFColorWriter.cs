using KinectXEFTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace XEFExtract
{
    class XEFColorWriter : IXEFDataWriter, IDisposable
    {
        //
        //  Members
        //

        private VideoWriter _writer;

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

        public XEFColorWriter(string path)
        {
            FilePath = path;
            EventCount = 0;
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;

            _writer = new VideoWriter(path,
                VideoWriter.FrameFormat.GRAY8,
                VideoWriter.VideoCodec.MPEG4,
                512,
                424,
                15,
                400000);
        }

        ~XEFColorWriter()
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
                    // Dispose managed resources
                    _writer.Dispose();
                }

                disposed = true;
            }
        }

        //
        //  Methods
        //

        public void Close()
        {
            Dispose(true);
        }

        public void ProcessEvent(XEFEvent ev)
        {
            /*if (ev.EventStreamDataTypeId != StreamDataTypeIds.UncompressedColor)
            {
                return;
            } */
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

            _writer.WriteFrame(ev.EventData, ev.RelativeTime - StartTime);

            EventCount++;
        }
    }
}
