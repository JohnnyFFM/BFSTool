using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS4WIN
{
    static class BFS
    {
        public static BFSTOC bfsTOC;
        private static GPT gpt;
        private static LowLevelDiskAccess llda;

        static BFS()
        {
            llda = new LowLevelDiskAccess();
        }

        public static void loadBFSTOC(string drive)
        {
            bfsTOC = BFSTOC.FromSector(llda.ReadSector(drive, 5, 4096));
        }

        public static Boolean isBFS(string drive)
        {
            Boolean test = true;
            //Check if first Partition is BFS
            gpt = GPT.FromSectors(llda.ReadSector(drive, 0, 4096 * 5));
            test = test && (gpt.gptPartitionTable.partitions[0].BFSParitionType == new Guid("53525542-4354-494F-4E46-494C45535953"));
            //Check if BFSTOC v1 exists
            bfsTOC = BFSTOC.FromSector(llda.ReadSector(drive, 5, 4096));
            byte[] version = Encoding.ASCII.GetBytes("BFS1");
            test = test && bfsTOC.version.SequenceEqual(version);
            return test;
        }

        public static void FormatDriveGPT(string drive, UInt64 totalSectors, UInt32 bytesPerSector)
        {
            //create GPT
            GPT gpt = new GPT(totalSectors, bytesPerSector);
            byte[] test = gpt.ToByteArray();
            //create BFSTOC
            BFSTOC bfsTOC = BFSTOC.emptyToc(totalSectors, bytesPerSector, true);
            //write GPT
            llda.WriteSector(drive, 0, 4096, gpt.ToByteArray());
            //write MirrorGPT
            gpt.ToggleMirror();
            llda.WriteSector(drive, (Int64)totalSectors -40, 512, gpt.ToGPTMirrorByteArray());
            //write  BFSTOC
            llda.WriteSector(drive, 5, 4096, bfsTOC.ToByteArray());
            //write mirror BFSTOC
            llda.WriteSector(drive, 6 + (Int64)bfsTOC.diskspace, 4096, bfsTOC.ToByteArray());
            //trigger OS partition table re-read
            llda.refreshDrive(drive);
        }

        public static void FormatDriveMBR(string drive, UInt64 totalSectors, UInt32 bytesPerSector)
        {
            //create classic MBR
            MBR mbr = new MBR((UInt32)totalSectors, bytesPerSector, false);
            //inflate to 4k
            Array.Resize(ref mbr.mbr, 4096);
            //create BFSTOC
            BFSTOC bfsTOC = BFSTOC.emptyToc(totalSectors, bytesPerSector, false);
            //write MBR
            llda.WriteSector(drive, 0, 4096, mbr.ToByteArray());
            //write  BFSTOC
            llda.WriteSector(drive, 1, 4096, bfsTOC.ToByteArray());
            //write mirror BFSTOC
            llda.WriteSector(drive, 2 + (Int64)bfsTOC.diskspace, 4096, bfsTOC.ToByteArray());
            //trigger OS partition table re-read
            llda.refreshDrive(drive);
        }
    }
}
