using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BFS4WIN
{
    public class Scoop
    {
        public byte[] byteArrayField;

        public Scoop(int nonces)
        {
            Array.Resize(ref byteArrayField, 64 * nonces);
        }

        public byte[] ToByteArray()
        {
            return byteArrayField;
        }

        public int Size()
        {
            return byteArrayField.Length * 64;
        }

    }

}
