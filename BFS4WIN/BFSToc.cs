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

    //BadSector structure
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BadSector
    {
        public UInt64 startPos;
        public UInt32 size;
    }


    //GPT
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GPT
    {
        public MBR pmbr;
        public GPTHeader gptHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)]
        public byte[] reserved;
        public GPTPartitionTable gptPartitionTable;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3072)]
        public byte[] reserved2;
        //Create GPT for BFS
        public GPT(UInt64 totalSectors, UInt32 bytesPerSector)
        {

            pmbr = new MBR(totalSectors > UInt32.MaxValue ? UInt32.MaxValue : (UInt32)totalSectors, bytesPerSector, true);
            gptHeader = new GPTHeader((UInt32)totalSectors, bytesPerSector);
            reserved = new byte[420];
            gptPartitionTable = new GPTPartitionTable();
            reserved2 = new byte[3072];
            //create partition entry
            gptPartitionTable.partitions = new GPTPartition[152];
            gptPartitionTable.partitions[0].BFSParitionType = new Guid("53525542-4354-494F-4E46-494C45535953");
            gptPartitionTable.partitions[0].partitionGuid = Guid.NewGuid();
            gptPartitionTable.partitions[0].startPos = 40;
            gptPartitionTable.partitions[0].endPos = (5+2 + ((((UInt64)totalSectors * bytesPerSector / 4096) - 12) / 64) * 64) * (4096 / bytesPerSector);
            gptPartitionTable.partitions[0].attributes = 0;
            //gptEntries[0].partitionName = //Optional Name :-)
            //Update CRCs
            gptHeader.UpdateCRC32(CRC.CRC32(gptPartitionTable.ToByteArray()));
            
        }


        public byte[] ToByteArray()
        {
            byte[] buff = new byte[Marshal.SizeOf(typeof(GPT))];
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return buff;
        }

        public static GPT FromSectors(byte[] buff)
        {
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            GPT s = (GPT)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(GPT));
            handle.Free();
            return s;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GPTPartitionTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public GPTPartition[] partitions;

        public byte[] ToByteArray()
        {
            byte[] buff = new byte[Marshal.SizeOf(typeof(GPTPartitionTable))];
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return buff;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GPTPartition
    {
        //BFS Partition Type GUID
        public Guid BFSParitionType;
        public Guid partitionGuid;
        public UInt64 startPos;
        public UInt64 endPos;
        public UInt64 attributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)]
        public byte[] partitionName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GPTHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] signature;
        public UInt32 revision;
        public UInt32 headerSize;
        public UInt32 crc32Header;
        public UInt32 reserved1;
        public UInt64 currentLba;
        public UInt64 backupLba;
        public UInt64 firstUsableLba;
        public UInt64 lastUsableLba;
        public Guid diskGuid;
        public UInt64 entriesStart;
        public UInt32 entriesCount;
        public UInt32 entriesSize;
        public UInt32 crc32Array;

        //Create GPT for BFS
        public GPTHeader(UInt64 totalSectors, UInt32 bytesPerSector)
    {

            signature = new byte[] { 69, 70, 73, 32, 80, 65, 82, 84 };
            revision = 256*256;
            headerSize = 92;
            crc32Header = 0;
            reserved1 = 0;
            currentLba = 1;
            backupLba = totalSectors - 5;
            firstUsableLba = 40;
            lastUsableLba = totalSectors -6;
            diskGuid = Guid.NewGuid();
            entriesStart = 2;
            entriesCount = 128;
            entriesSize = 128;
            crc32Array = 0;
        }

        public void UpdateCRC32(UInt32 partitionTablecCRC32)
        {
            crc32Array = partitionTablecCRC32;
            crc32Header = 0;
            crc32Header = CRC.CRC32(ToByteArray());
        }

        public byte[] ToByteArray()
        {
            byte[] buff = new byte[Marshal.SizeOf(typeof(GPTHeader))];
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return buff;
        }

    }

    //MBR
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MBR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] mbr;

        //Create MBR for BFS
        public MBR(UInt32 totalSectors, UInt32 bytesPerSector, Boolean gpt) {
            mbr = new byte[512];
            if (gpt)
            {
                byte[] lbaSize = BitConverter.GetBytes(totalSectors-1);        
                //define GPT PMBR
                //Partiton Start (CHS)
                mbr[447] = 0x00;
                mbr[448] = 0x02;
                mbr[449] = 0x00;
                //Partition ID
                mbr[450] = 0xEE;
                //Partiton End (CHS)
                mbr[451] = 0xFE;
                mbr[452] = 0xFF;//TODO FF
                mbr[453] = 0xFF;//TODO FF
                //Starting Sector (LBA)
                mbr[454] = 0x01;
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
            else
            {
                //Calculate Parition Size
                UInt64 partitionSize = (2 + ((((UInt64)totalSectors * bytesPerSector / 4096) - 3) / 64) * 64) * (4096 / bytesPerSector);
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
        }

        public byte[] ToByteArray()
        {
            byte[] buff = new byte[512];
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

        public static BFSTOC emptyToc(UInt64 totalSectors, UInt32 bytesPerSector,Boolean gpt)
        {
            BFSTOC s = new BFSTOC
            {
                version = Encoding.ASCII.GetBytes("BFS1"),
                diskspace = (((totalSectors * bytesPerSector / 4096) - 3) / 64) * 64,
            };
            s.plotFiles = new PlotFile[32];
            if(gpt)
            {
                s.plotFiles[0].startPos = 6;
            }
            else
            {
                s.plotFiles[0].startPos = 2;
            }
            s.UpdateCRC32();
            return s;
        }

        public void UpdateCRC32()
        {
            crc32 = 0;
            crc32 = CRC.CRC32BFSTOC(ToByteArray());
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

        public Boolean DeleteLastPlotFile()
        {
            int position = PlotFileCount();
            //return false if no file left
            if (position == 0) return false;
            position -= 1;
            //for the first file, don_t killstartpos
            //add file
            plotFiles[position].id = 0;
            plotFiles[position].startNonce = 0;
            plotFiles[position].nonces = 0;
            plotFiles[position].status = 0;
            plotFiles[position].pos = 0;
            if (position != 0) plotFiles[position].startPos = 0;
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
