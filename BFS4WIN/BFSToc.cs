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
    Each slot (44 byte) contains: key (64bit), startNonce (64bit), nonces(64bit), startPos(64bit), pos(64bit), status(32bit)
    startPos: 4k sector number where the plot file starts
    status:
      1 = OK -> File is ready to use
      2 = WRITING -> File is incomplete. Current position is saved in parameter 'pos'.
      3 = PLOTTING -> File is incomplete. Current nonce is saved in parameter 'pos'.
      4 = CONVERTING -> File is incomplete. Current scoop is saved in parameter 'pos'.
    pos: Counter value used for wrinting and plotting.

    After TOC table of 52 defect areas (624 byte):
    Each entry (12 byte) contains: startPos(64bit), size(32bit)
    startPos: 4k sector number where the defect area starts
    size: Size of defect area as 4k sector count

    Last 12 byte of 4k TOC:
    Size of disk in 4k sectors (64bit)
    Last 4 byte of 4k TOC is CRC(32bit) -> Algo is CRC32
    */
    /// </summary>

    //PoC PlotFile structure
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlotFile
    {
        public UInt64 id;
        public UInt64 startNonce;
        public UInt64 nonces;
        public UInt64 startPos;
        public UInt64 pos;
        public UInt32 status;
    }

    //BadSector strcuture
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BadSector
    {
        public UInt64 startPos;
        public UInt32 size;
    }

    //BFSTOC
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BFSTOC
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public PlotFile[] plotFiles;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
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

        public int AddPlotFile(UInt64 id, UInt64 start, UInt64 nonces, UInt32 status, UInt64 pos)
        {
            int position = PlotFileCount();
            //return false if no slot left
            if (position == 32) return -1;
            
            //return false if no disk space left
            ulong freespace;
            ulong newStartPos;
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
            if (nonces * 64 > freespace) return -1;

            //add file
            plotFiles[pos].id = id;
            plotFiles[pos].startNonce = start;
            plotFiles[pos].nonces = nonces;
            plotFiles[pos].status = status;
            plotFiles[pos].pos = pos;
            plotFiles[pos].startPos = newStartPos;
            UpdateCRC32();
            return (int)pos;
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

}
