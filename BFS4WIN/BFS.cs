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

        public static void LoadBFSTOC(string drive)
        {
            bfsTOC = BFSTOC.FromSector(llda.ReadSector(drive, 5, 4096));
        }


        public static void SaveBFSTOC(string drive)
        {
            //write  BFSTOC
            llda.WriteSector(drive, 5, 4096, bfsTOC.ToByteArray());
            //write mirror BFSTOC
            llda.WriteSector(drive, 6 + (Int64)bfsTOC.diskspace, 4096, bfsTOC.ToByteArray());
        }


        public static Boolean IsBFS(string drive)
        {
            Boolean test = true;
            //Check if first Partition is BFS
            byte[] first;
            first = llda.ReadSector(drive, 0, 4096 * 5);
            if (first != null)
            {
                gpt = GPT.FromSectors(first);
                test = test && (gpt.gptPartitionTable.partitions[0].BFSParitionType == new Guid("53525542-4354-494F-4E46-494C45535953"));
                //Check if BFSTOC v1 exists
                bfsTOC = BFSTOC.FromSector(llda.ReadSector(drive, 5, 4096));
                byte[] version = Encoding.ASCII.GetBytes("BFS1");
                test = test && bfsTOC.version.SequenceEqual(version);
                return test;
            }
            return false;
        }

        public static int AddPlotFile(String drive, UInt64 start, UInt32 nonces, UInt32 status, UInt32 pos)
        {
            int result;
            LoadBFSTOC(drive);
            result = bfsTOC.AddPlotFile(start, nonces, status, pos);
            SaveBFSTOC(drive);
            return result;
        }

        public static void SetPos(string drive, int file_id, UInt32 pos)
        {
            LoadBFSTOC(drive);
            bfsTOC.plotFiles[file_id].pos = pos;
            SaveBFSTOC(drive);
        }

        public static void SetStatus(string drive, int file_id, UInt32 status)
        {
            LoadBFSTOC(drive);
            bfsTOC.plotFiles[file_id].status = status;
            SaveBFSTOC(drive);
        }

        public static Boolean SetID(String drive, UInt64 id)
        {
            LoadBFSTOC(drive);
            bfsTOC.id = id ;
            SaveBFSTOC(drive);
            return true;
        }

        public static Boolean DeleteLastPlotFile(string drive)
        {
            Boolean result;
            //Read current bfsTOC
            LoadBFSTOC(drive);
            //update bfsTOC if delete success
            result = bfsTOC.DeleteLastPlotFile();
            if (result) SaveBFSTOC(drive);
            return result;
        }

        public static void FormatDriveGPT(string drive, UInt64 totalSectors, UInt32 bytesPerSector, UInt64 id)
        {
            //create GPT
            GPT gpt = new GPT(totalSectors, bytesPerSector);
            byte[] test = gpt.ToByteArray();
            //create BFSTOC
            BFSTOC bfsTOC = BFSTOC.emptyToc(totalSectors, bytesPerSector, true);
            bfsTOC.id = id;
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
    }
}
