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

    //classic MBR
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MBR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] mbr;

        //Create classic MBR for BFS
        public MBR(UInt32 totalSectors, UInt32 bytesPerSector) {
            mbr = new byte[4096];
            //Calculate Parition Size
            UInt64 partitionSize = (2 + ((((UInt64)totalSectors * bytesPerSector / 4096) - 3) / 64) * 64)*(4096/bytesPerSector);
            UInt32 partitionSizeX = (UInt32)partitionSize;
            byte[] lbaSize = BitConverter.GetBytes(partitionSize);
            //Partiton Start (CHS)
            mbr[447] = 0x00;
            mbr[448] = 0x09; //first sector is 1 not 0
            mbr[449] = 0x00;
            //Partition ID
            mbr[450] = 0x64;
            //Partiton End (CHS)
            mbr[451] = 0xFE;
            mbr[452] = 0xFF;
            mbr[453] = 0xFF;
            //Starting Sector (LBA)
            mbr[454] = 0x08;
            mbr[455] = 0x00;
            mbr[456] = 0x00;
            mbr[457] = 0x00;
            //Partition Size (LBA)
            mbr[458] = lbaSize[0];
            mbr[459] = lbaSize[1];
            mbr[460] = lbaSize[2];
            mbr[461] = lbaSize[3];
            //Magic Bytes
            mbr[510] = 0x55;
            mbr[511] = 0xAA;
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
                diskspace = (((totalSectors * bytesPerSector / 4096) - 3) / 64) * 64,
            };
            s.plotFiles = new PlotFile[32];
            s.plotFiles[0].startPos = 2;
            
            /*
            //define GPT PMBR
            //Partiton Start (CHS)
            s.mbr[447] = 0x00;
            s.mbr[448] = 0x02;
            s.mbr[449] = 0x00;
            //Partition ID
            s.mbr[450] = 0xEE;
            //Partiton End (CHS)
            s.mbr[451] = 0xFE;
            s.mbr[452] = 0xFF;
            s.mbr[453] = 0xFF;
            //Starting Sector (LBA)
            s.mbr[454] = 0x01; //in CHS a partition can only start at the beginning of a track, 3F is second track. 
            s.mbr[455] = 0x00;
            s.mbr[456] = 0x00;
            s.mbr[457] = 0x00;
            //Partition Size (LBA)
            s.mbr[458] = 0xFF;
            s.mbr[459] = 0xFF;
            s.mbr[460] = 0xFF;
            s.mbr[461] = 0xFF;
            //Magic Bytes
            s.mbr[510] = 0x55;
            s.mbr[511] = 0xAA;
            */
            //s.UpdateCRC32();
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
            if (position == 0)
            {
                freespace = diskspace;
                newStartPos = plotFiles[0].startPos;
            }
            else
            {
                newStartPos = plotFiles[position - 1].startPos + plotFiles[position - 1].nonces * 64;
                freespace = diskspace - newStartPos + 2;
            }
            if (nonces * 64 > freespace) return -1;

            //add file
            plotFiles[position].id = id;
            plotFiles[position].startNonce = start;
            plotFiles[position].nonces = nonces;
            plotFiles[position].status = status;
            plotFiles[position].pos = pos;
            plotFiles[position].startPos = newStartPos;
            UpdateCRC32();
            return (int)position;
        }

        public byte[] ToByteArray()
        {
            byte[] buff = new byte[TSSize.Size];
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return buff;
        }


        /*
         * mS 	 Anzahl der Sektoren pro Zylinder 
 mH 	 Anzahl der Köpfe 
 S 	 Sektor 
 H 	 Kopf 
 C 	 Zylinder 
LBA = (C*mH*mS) + (H*mS) + S - 

    S=LBA+1-(((LBA+1-S-(H*ms))/mH*mS)*mH*mS) + (((LBA+1-S-(C*mH*mS))/ms)*mS)1


         * 
         * 
         */

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
