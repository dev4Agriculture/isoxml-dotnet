using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    using System;
    using System.IO;

    public class BufferedBinaryReader
    {
        private readonly BinaryReader binaryReader;
        private byte[] _buffer;
        private int _bufferOffset;
        private readonly int _bufferSize;
        private readonly int _basePosition;
        private readonly int _reloadStart;
        private readonly int _reloadEnd;
        private readonly int _positionCheckCount;
        private readonly int _positionCheckEveryXReads;
        private int _bufferPos;

        public BufferedBinaryReader(FileStream fileStream, int bufferSize)
        {
            if( bufferSize < 1e6)
            {
                throw new ArgumentOutOfRangeException("BufferSize should at least be 100k");
            }
            binaryReader = new BinaryReader(fileStream);
            this._bufferSize = bufferSize;
            _buffer = new byte[bufferSize];
            _bufferOffset = 0;
            _bufferPos = 0;
            _basePosition = bufferSize/2;
            _reloadStart = bufferSize / 10;
            _reloadEnd = bufferSize / 10 * 9;
            FillBuffer();
        }

        private void FillBuffer()
        {
            binaryReader.BaseStream.Seek(_bufferOffset, SeekOrigin.Begin);
            if (_buffer.Length < _bufferSize)
            {
               Array.Resize(ref _buffer, _bufferSize);
            }
            _bufferPos = 0;
            var readBytes = binaryReader.Read(_buffer, 0, _bufferSize);
            if (readBytes < _bufferSize)
            {
                Array.Resize(ref _buffer, readBytes);
            }
        }

        public long Position => _bufferOffset + _bufferPos;

        public void Seek(int position)
        {
            int absolutePosition = _bufferOffset + position;
            if (absolutePosition < _bufferOffset || absolutePosition >= _bufferOffset + _buffer.Length)
            {
                _bufferOffset = absolutePosition;
                FillBuffer();
            }
            else
            {
                _bufferPos = position;
            }
        }

        public byte ReadByte()
        {
            if (_bufferPos >= _buffer.Length - 200)
            {
                _bufferOffset += _bufferPos;
                FillBuffer();
            }

            var value = _buffer[_bufferPos];
            _bufferPos++;
            return value;
        }

        public int ReadInt32()
        {
            var bytes = ReadBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public short ReadInt16()
        {
            var bytes = ReadBytes(2);
            return BitConverter.ToInt16(bytes, 0);
        }

        public uint ReadUInt32()
        {
            var bytes = ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ushort ReadUInt16()
        {
            var bytes = ReadBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = _buffer[_bufferPos +i];
            }
            _bufferPos += count;
            return bytes;
        }
    }
}
