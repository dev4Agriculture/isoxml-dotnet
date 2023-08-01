using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public unsafe class BufferedBinaryReader
    {
        private readonly BinaryReader _binaryReader;
        private byte[] _buffer;
        private int _bufferOffset;//This value describes the index of the first byte in the currently loaded buffer
        private readonly int _bufferSize;
        private readonly int _basePosition;//This value is used trying to put the pointer always in the center
        private readonly int _reloadEnd;//The Trigger end to read new data into the buffer when moving forwards
        private int _bufferPos;//The position inside the buffer

        public int Position => _bufferPos + _bufferOffset;

        public BufferedBinaryReader(FileStream fileStream, int bufferSize, int maximumStepBack = 0)
        {
            if (bufferSize < 1e6)
            {
                throw new ArgumentOutOfRangeException("BufferSize should at least be 100k");
            }
            _binaryReader = new BinaryReader(fileStream);
            _bufferSize = bufferSize;
            _buffer = new byte[bufferSize];
            _bufferOffset = 0;
            _bufferPos = 0;
            _basePosition = maximumStepBack;
            _reloadEnd = bufferSize / 10 * 9;


            var readBytes = _binaryReader.Read(_buffer, 0, _bufferSize);
            if (readBytes < _bufferSize)
            {
                Array.Resize(ref _buffer, readBytes);
            }
        }

        private void FillBuffer()
        {
            _binaryReader.BaseStream.Seek(_bufferOffset, SeekOrigin.Begin);
            if (_buffer.Length < _bufferSize)
            {
                Array.Resize(ref _buffer, _bufferSize);
            }
            _bufferPos = 0;
            var readBytes = _binaryReader.Read(_buffer, 0, _bufferSize);
            if (readBytes < _bufferSize)
            {
                Array.Resize(ref _buffer, readBytes);
            }
        }

        private void CheckBuffer()
        {
            if( _bufferPos > _reloadEnd)
            {
                var bufferStartFrom = _bufferPos - _basePosition;
                var bufferLength = _bufferSize - _bufferPos + _basePosition;
                _bufferOffset += bufferStartFrom;
                Buffer.BlockCopy(_buffer, bufferStartFrom, _buffer, 0, bufferLength);
                var readBytes = _binaryReader.Read(_buffer, _basePosition, _bufferSize - _basePosition);
                _bufferPos = _basePosition;
            }
        }

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

        private byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = _buffer[_bufferPos + i];
            }
            _bufferPos += count;
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            if (_bufferPos > _reloadEnd)
            {
                var bufferStartFrom = _bufferPos - _basePosition;
                var bufferLength = _bufferSize - _bufferPos + _basePosition;
                _bufferOffset += bufferStartFrom;
                Buffer.BlockCopy(_buffer, bufferStartFrom, _buffer, 0, bufferLength);
                var readBytes = _binaryReader.Read(_buffer, _basePosition, _bufferSize - _basePosition);
                _bufferPos = _basePosition;
            }
            return _buffer[++_bufferPos];
            /*
            //CheckBuffer();
            return _buffer[++_bufferPos];
            */
        }


        public int ReadInt32()
        {
            var size = 4;
            _bufferPos += size;
            if (_bufferPos > _reloadEnd)
            {
                CheckBuffer();
            }
            return BitConverter.ToInt32(_buffer, _bufferPos - size);
        }

        public short ReadInt16()
        {
            _bufferPos += 2;
            if (_bufferPos > _reloadEnd)
            {
                CheckBuffer();
            }
            return BitConverter.ToInt16(_buffer, _bufferPos);
        }

        public uint ReadUInt32()
        {
            _bufferPos += 4;
            if (_bufferPos > _reloadEnd)
            {
                CheckBuffer();
            }
            return BitConverter.ToUInt32(_buffer, _bufferPos);
        }

        public ushort ReadUInt16()
        {
            _bufferPos += 2;
            if (_bufferPos > _reloadEnd || _bufferPos > _bufferSize)
            {
                CheckBuffer();
            }
            return BitConverter.ToUInt16(_buffer, _bufferPos - 2);
        }

    }
}
