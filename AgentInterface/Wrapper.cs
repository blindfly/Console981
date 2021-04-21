using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AgentInterface
{
    public enum AppState
    {
        Wait = 0,
        Run,
    }

    public enum Command
    {
        None = 0,
        Run,
        Stop,
        Pause
    }

    [Serializable]
    public struct InterfaceStruct
    {
        public int state;
        public int command;
    }

    public interface IAgentCommander
    {
        void SetCommand(Command comm);
    }

    public partial class MMFWrapper
    {
        static private IAgentCommander commaner_ = null;
        static public IAgentCommander sCommander {
            get{
                return commaner_;
            }
        }


        private MemoryMappedFile mmfAgentStruct;
        private InterfaceStruct m_Struct = new InterfaceStruct();

        static private byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        static private int ByteArrayToStructure(byte[] data, ref object obj)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.

            Marshal.Copy(data, 0, buff, data.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.

            obj = Marshal.PtrToStructure(buff, obj.GetType()); // 복사된 데이터를 구조체 객체로 변환한다.

            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            if (Marshal.SizeOf(obj) != data.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return 0; // 크기가 다르면 null 리턴
            }
            return 1; // 구조체 리턴
        }

        public MMFWrapper()
        {
            int structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(InterfaceStruct));

            try
            {
                using (mmfAgentStruct = MemoryMappedFile.OpenExisting("_AGENT_MMF_"))
                {
                    mmfAgentStruct = MemoryMappedFile.OpenExisting(
                      "_AGENT_MMF_", MemoryMappedFileRights.ReadWrite);
                }
            }
            catch (FileNotFoundException)
            {
                mmfAgentStruct = MemoryMappedFile.CreateNew("_AGENT_MMF_", structSize);
            }

            commaner_ = this as IAgentCommander;
        }

        private void WriteToMMF(InterfaceStruct st)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(InterfaceStruct));

            MemoryMappedViewAccessor accessor = mmfAgentStruct.CreateViewAccessor();
            accessor.WriteArray(0, StructureToByteArray((object)st), 0, size);
            accessor.Dispose();
        }


        unsafe public void ReadMMF(ref InterfaceStruct st)
        {
            using (var stream = mmfAgentStruct.CreateViewStream())
            {
                using (BinaryReader binReader = new BinaryReader(stream))
                {
                    object obj = (object)st;
                    ByteArrayToStructure(binReader.ReadBytes((int)stream.Length), ref obj);
                    st = (InterfaceStruct)obj;
                }
            }
        }
    }


    public partial class MMFWrapper: IAgentCommander
    {
        public void SetCommand(Command comm)
        {
            Console.WriteLine("[SetCommand] " + comm);

            m_Struct.command = (int)comm;
            WriteToMMF(m_Struct);
        }
    }


    public partial class MMFWrapper
    {
        public void SetState(AppState s)
        {
            m_Struct.state = (int)s;
            WriteToMMF(m_Struct);
        }


        public Command GetCommand()
        {
            ReadMMF(ref m_Struct);
            Command ret = (Command)m_Struct.command;
            m_Struct.command = (int)Command.None;
            WriteToMMF(m_Struct);
            return ret;
        }
    }
}





