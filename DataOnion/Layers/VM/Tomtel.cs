using System;
using System.Collections;
using System.Collections.Generic;

namespace DataOnion.Layers.VM
{
    public class Tomtel
    {
        readonly TomtelStorage Storage;
        List<byte> Output { get; set; } = new List<byte>();
        Dictionary<Operation, Action> OperationLookup;
        enum Operation
        {
            Halt = 1,
            Out = 2,
            Jez = 33,
            Jnz = 34,
            Cmp = 193,
            Add = 194,
            Sub = 195,
            Xor = 196,
            Aptr = 225
        }

        public Tomtel(byte[] bytes)
        {
            Storage = new TomtelStorage(bytes);
            InitOperationLookup();
        }

        public void Execute()
        {
            OperationLoop();
        }

        public byte[] GetOutputBytes()
        {
            return Output.ToArray();
        }

        void InitOperationLookup()
        {
            OperationLookup = new Dictionary<Operation, Action>
            {
                { Operation.Out, Out },
                { Operation.Jez, Jez },
                { Operation.Jnz, Jnz },
                { Operation.Add, Add },
                { Operation.Cmp, Cmp },
                { Operation.Sub, Sub },
                { Operation.Xor, Xor },
                { Operation.Aptr, Aptr },
            };
        }

        void OperationLoop()
        {
            byte instruction;
            BitArray instructionBits;
            Operation operation;

            while (true)
            {
                instruction = Storage.ReadImm8();
                operation = (Operation)instruction;

                if (operation == Operation.Halt)
                {
                    return;
                }

                if (OperationLookup.ContainsKey(operation))
                {
                    OperationLookup[operation].Invoke();
                    continue;
                }

                instructionBits = new BitArray(new byte[] { instruction });
                if (instructionBits[7] != instructionBits[6])
                {
                    MoveSelectOperation(instructionBits);
                    continue;
                }

                throw new Exception($"Invalid operation found: {instruction}");
            }
        }

        void MoveSelectOperation(BitArray insb)
        {
            uint src = Storage.ReadSrcToUInt(insb);

            if (!insb[7] && insb[6])
            {
                if (src == 0)
                {
                    Mvi(insb);
                }
                else
                {
                    Mv(insb);
                }
                return;
            }

            else if (insb[7] && !insb[6])
            {
                if (src == 0)
                {
                    Mvi32(insb);
                }
                else
                {
                    Mv32(insb);
                }
                return;
            }

            throw new Exception($"Invalid operation found: {insb}");
        }

        void Mv(BitArray insb)
        {
            //Sets `{dest}` to the value of `{src}`.
            Storage.Reg(Storage.ReadDstToUInt(insb)) = Storage.Reg((byte)Storage.ReadSrcToUInt(insb));
        }

        void Mvi(BitArray insb)
        {
            //Sets `{dest}` to the value of `imm8`.
            Storage.Reg(Storage.ReadDstToUInt(insb)) = Storage.ReadImm8();
        }

        void Mv32(BitArray insb)
        {
            //Sets `{dest}` to the value of `{src}`.
            Storage.Reg32(Storage.ReadDstToUInt(insb)) = Storage.Reg32(Storage.ReadSrcToUInt(insb));
        }

        void Mvi32(BitArray insb)
        {
            //Sets `{dest}` to the value of `imm32`.
            Storage.Reg32(Storage.ReadDstToUInt(insb)) = Storage.ReadImm32();
        }

        void Add()
        {
            //Sets `a` to the sum of `a` and `b`, modulo 256.
            Storage.A += Storage.B;
        }

        void Aptr()
        {
            //Sets `ptr` to the sum of `ptr` and `imm8`. Overflow behaviour is undefined.
            Storage.Ptr += Storage.ReadImm8();
        }

        void Cmp()
        {
            //Sets `f` to zero if `a` and `b` are equal, otherwise sets `f` to 0x01.
            Storage.F = Convert.ToByte(Storage.A != Storage.B);
        }

        void Jez()
        {
            //If `f` is equal to zero, sets `pc` to `imm32`. Otherwise does nothing.
            if (Storage.F == 0)
            {
                Storage.Pc = Storage.ReadImm32();
            }
            else
            {
                _ = Storage.ReadImm32();
            }
        }

        void Jnz()
        {
            //If `f` is not equal to zero, sets `pc` to `imm32`. Otherwise does nothing.
            if (Storage.F != 0)
            {
                Storage.Pc = Storage.ReadImm32();
            }
            else
            {
                _ = Storage.ReadImm32();
            }
        }

        void Out()
        {
            //Appends the value of `a` to the output stream.
            Output.Add(Storage.A);
        }

        void Sub()
        {
            //Sets `a` to the result of subtracting `b` from `a`.
            //If subtraction would result in a negative number, 256 is added to ensure that the result is non - negative.
            Storage.A -= Storage.B;
        }

        void Xor()
        {
            //Sets `a` to the bitwise exclusive OR of `a` and `b`.
            Storage.A ^= Storage.B;
        }

    }
}
