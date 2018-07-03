using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Threading.Tasks;
using System.Collections;

namespace BFS4WIN
{
    // routines taken from https://code.msdn.microsoft.com/windowsapps/CCS-LABS-C-Low-Level-Disk-91676ca9
    class LowLevelDiskAccess
    {
        SafeFileHandle iFile;

        #region "API CALLS" 

        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Boolean SetFilePointerEx(
            [In] SafeFileHandle hFile,
            [In] Int64 liDistanceToMove,
            [Out] out Int64 lpNewFilePointer,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint SetFilePointer(
            [In] SafeFileHandle hFile,
            [In] int lDistanceToMove,
            [Out] out int lpDistanceToMoveHigh,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
          uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
          uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        internal extern static int ReadFile(SafeFileHandle handle, byte[] bytes,
           int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);

        [DllImport("kernel32", SetLastError = true)]
        internal extern static int WriteFile(SafeFileHandle handle,IntPtr bytes, //byte[] bytes,
           int numBytesToWrite, out int numBytesRead, IntPtr overlapped_MustBeZero);

        [DllImportAttribute("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern Boolean DeviceIoControl(SafeFileHandle hDevice, Int32 dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, Int32 nOutBufferSize, ref Int32 lpBytesReturned, IntPtr lpOverlapped);

        #endregion

        public Boolean refreshDrive(string drive)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;
            Boolean test;
            SafeFileHandle handleValue = CreateFile(drive, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            Int32 read = 0;

            int IOCTL_DISK_UPDATE_PROPERTIES = 0x70140;
            test = DeviceIoControl(handleValue, IOCTL_DISK_UPDATE_PROPERTIES, IntPtr.Zero, 0, IntPtr.Zero, 0, ref read, IntPtr.Zero);
            handleValue.Close();
            return test;
        }

        // get proper amount of sectors
        public Int64 GetSectors(string drive)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;

            SafeFileHandle handleValue = CreateFile(drive, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
   
            Int64 DriveSize = 0;


            IntPtr outDriveSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int64)));

            Int32 read = 0;

            int IOCTL_DISK_GET_LENGTH_INFO = 0x7405c;
            if (DeviceIoControl(handleValue, IOCTL_DISK_GET_LENGTH_INFO, IntPtr.Zero, 0, outDriveSize, Marshal.SizeOf(typeof(Int64)), ref read, IntPtr.Zero))
                {
                handleValue.Close();
                DriveSize = (Int64)Marshal.PtrToStructure(outDriveSize, typeof(Int64));
                Marshal.FreeHGlobal(outDriveSize);
                return DriveSize;
            }
            else {
                handleValue.Close();
                return 0;
            }
        }
        /// <summary> 
        /// Returns the Sector from the drive at the specified location 
        /// </summary> 
        /// <param name="drive"> 
        /// The drive to have a sector read 
        /// </param> 
        /// <param name="sector"> 
        /// The sector number to read. 
        /// </param> 
        /// <param name="bytesPerSector"></param> 
        /// <returns></returns> 
        public byte[] ReadSector(string drive, Int64 sector, Int32 bytesPerSector)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;

            SafeFileHandle handleValue = CreateFile(drive, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            //calculate byte position
            Int64 sec = sector * bytesPerSector;

            byte[] buf = new byte[bytesPerSector];
            Int64 filePos;
            if (!SetFilePointerEx(handleValue, sec, out filePos, EMoveMethod.Begin))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            int read = 0;
            ReadFile(handleValue, buf, bytesPerSector, out read, IntPtr.Zero);
            handleValue.Close();
            return buf;
        }


        public Boolean OpenW(string drive)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;

            iFile = CreateFile(drive, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (iFile.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return true;
        }

        public void Close()
        {
            iFile.Close();
        }


        public void Seek(Int64 position)
        {
            Int64 filePos;
            if (!SetFilePointerEx(iFile, position, out filePos, EMoveMethod.Begin))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public int Write(byte[] data, Int32 offset, Int32 length)
        {
            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            int write = 0;
            WriteFile(iFile, IntPtr.Add(pointer,offset), length, out write, IntPtr.Zero);
            pinnedArray.Free();
            return write;
        }

        public byte[] ReadSectors(string drive, Int64 sector, Int32 bytesPerSector, Int32 number)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;

            SafeFileHandle handleValue = CreateFile(drive, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            //calculate byte position
            Int64 sec = sector * bytesPerSector;

            byte[] buf = new byte[bytesPerSector];
            Int64 filePos;
            if (!SetFilePointerEx(handleValue, sec, out filePos, EMoveMethod.Begin))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            int read = 0;
            ReadFile(handleValue, buf, bytesPerSector*number, out read, IntPtr.Zero);
            handleValue.Close();
            return buf;
        }

        public void WriteSector(string drive, Int64 sector, Int32 bytesPerSector, byte[] data)
        {
            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_READ = 0x80000000;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;
            uint OPEN_EXISTING = 3;

            SafeFileHandle handleValue = CreateFile(drive, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            Int64 sec = sector * bytesPerSector;
            Int64 filePos;
            if (!SetFilePointerEx(handleValue, sec, out filePos, EMoveMethod.Begin))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            int write = 0;
            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            WriteFile(handleValue, pointer, data.Length, out write, IntPtr.Zero);
            pinnedArray.Free();
            handleValue.Close();
            //return buf;
        }

        #region "WMI LOW LEVEL COMMANDS" 

        /// <summary> 
        /// Returns the number of bytes that the drive sectors contain. 
        /// </summary> 
        /// <param name="drive"> 
        /// Int: The drive number to scan. 
        /// </param> 
        /// <returns> 
        /// Int: The number of bytes the sector contains. 
        /// </returns> 
        public UInt32 BytesPerSector(int drive)
        {
            int driveCounter = 0;
            try
            {
               ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (driveCounter == drive)
                    {
                        var t = queryObj["BytesPerSector"];
                        return UInt32.Parse(t.ToString());

                    }
                    driveCounter++;
                }
            }
            catch (ManagementException)
            {
                return 0;
            }
            return 0;
        }

        /// <summary> 
        /// Returns a list of physical drive IDs 
        /// </summary> 
        /// <returns> 
        /// ArrayList: Device IDs of all connected physical hard drives 
        ///  </returns> 
        public ArrayList GetDriveList()
        {
            ArrayList drivelist = new ArrayList();

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    drivelist.Add(queryObj["DeviceID"].ToString());
                }
            }
            catch (ManagementException)
            {
                return null;
            }
            return drivelist;
        }

        /// <summary> 
        /// Returns the total sectors on the specified drive 
        /// </summary> 
        /// <param name="drive"> 
        /// int: The drive to be queried. 
        /// </param> 
        /// <returns> 
        /// int: Returns the total number of sectors 
        /// </returns> 
        public UInt64 GetTotalSectors(int drive)
        {
            int driveCount = 0;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (driveCount == drive)
                    {
                        var t = queryObj["TotalSectors"];
                        return UInt64.Parse(t.ToString());

                    }
                    driveCount++;
                }
            }
            catch (ManagementException)
            {
                return 0;
            }
            return 0;
        }

        /// <summary> 
        /// Returns the caption of the drive. 
        /// </summary> 
        /// <param name="drive"> 
        /// The drive to be queried. 
        /// </param> 
        /// <returns> 
        /// string: drive caption
        /// </returns> 
        public string GetCaption(int drive)
        {
            int driveCount = 0;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (driveCount == drive)
                    {
                        var t = queryObj["Caption"];
                        return t.ToString();
                    }
                    driveCount++;
                }
            }
            catch (ManagementException)
            {
                return "";
            }
            return "";
        }        
        #endregion
    }
}
