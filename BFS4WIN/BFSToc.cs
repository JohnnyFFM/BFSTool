using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BFS4WIN
{
    /// <summary>
    /// Summary description for BFSTOC.
    /// implements BFS1 designed by brmmmm
    //definition used:
    /*Contents TOC (BFS1):
    Size is 4k (Original) + 4k (Backup).
    First 4 byte: BFS1
    A table of 32 slots for plot files.
    Each slot (36 byte) contains: key (64bit), startNonce (64bit), nonces(32bit), stagger(32bit), startPos(32bit), status(32bit), pos(32bit)
    If stagger=0 -> POC2 file
    startPos: 4k sector number where the plot file starts
    status:
        1=File is ready to use
        2=File is incomplete (writing or plotting)
        3=Converting to POC2
    pos: Counter value used for plotting and converting.
    After TOC table of defect areas (888 byte):
    Each entry (8 byte) contains: startPos(32bit), size(32bit)
    startPos: 4k sector number where the defect area starts
    size: Size of defect area as 4k sector count
    Last 8 byte of 4k TOC are:
    Change counter(32bit)
    CRC(32bit) -> Algo is CRC32
    */
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BFSTOC
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public PlotFile[] plotFiles;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 110)]
        public BadSector[] badSectors;
        public UInt64 diskspace;
        public UInt32 crc32;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
        public byte[] reserved;

        public static BFSTOC FromSector(byte[] buff)
        {
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            BFSTOC s = (BFSTOC)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BFSTOC));
            handle.Free();
            return s;
        }

        public static BFSTOC emptyToc(UInt64 totalSectors, UInt32 bytesPerSector)
        {
            BFSTOC s = new BFSTOC
            {
                version = Encoding.ASCII.GetBytes("BFS1"),
                diskspace = (((totalSectors * bytesPerSector / 4096)-2)/64)*64
            };
            s.UpdateCRC32();
            return s;
        }

        public void UpdateCRC32()
        {
            crc32 = CRC.CRC32(ToByteArray());
        }

        public int PlotFileCount()
        {
            int count = 0;
            foreach (PlotFile plotFile in plotFiles)
            {
                if (plotFile.id != 0) count++;
            }
            return count;
        }

        public Boolean AddPlotFile(ulong id, ulong start,uint nonces,uint stagger,uint status,uint pos)
        {
            int position = PlotFileCount();
            //return false if no slot left
            if (position == 32) return false;
            
            //return false if no disk space left
            ulong freespace;
            uint newStartPos;
            if (pos == 0)
            {
                freespace = diskspace;
                newStartPos = 2;
            }
            else
            {
                newStartPos = plotFiles[pos - 1].startPos + plotFiles[pos - 1].nonces / 64;
                freespace = diskspace - newStartPos + 2;
            }
            if (nonces / 64 > freespace) return false;

            //add file
            plotFiles[pos].id = id;
            plotFiles[pos].startNonce = start;
            plotFiles[pos].nonces = nonces;
            plotFiles[pos].stagger = stagger;
            plotFiles[pos].status = status;
            plotFiles[pos].pos = pos;
            plotFiles[pos].startPos = newStartPos;
            UpdateCRC32();
            return true;
        }

        public byte[] ToByteArray()
        {
            byte[] buff = new byte[TSSize.Size];
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return buff;
        }
    }

    internal sealed class TSSize
    {
        public static int _size;

        static TSSize()
        {
            _size = Marshal.SizeOf(typeof(BFSTOC));
        }

        public static int Size
        {
            get
            {
                return _size;
            }
        }
    }

    //Plotfile data
    //Each slot(36 bytes) contains: key(64bit), startNonce(64bit), nonces(32bit), stagger(32bit), startPos(32bit), status(32bit), pos(32bit)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlotFile
    {
        public ulong id;
        public ulong startNonce;
        public uint nonces;
        public uint stagger;
        public uint startPos;
        public uint status;
        public uint pos;
    }

    //Plotfile data
    //Each slot(8 bytes) contains: startPos(32bit),endPos(32bit)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BadSector
    {
        public uint startPos;
        public uint endPos;
    }
}
