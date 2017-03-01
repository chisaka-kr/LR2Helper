using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR2Helper_GV.program {
    public partial class LR2helper {
        private class assmble {
            void analyze() {
                //미완성.
                var vmem_analyze_poor_asm = Program.lr2helper.sharp.Memory.Allocate(256).BaseAddress; //메모리 할당
                var vmem_analyze_total_asm = Program.lr2helper.sharp.Memory.Allocate(256).BaseAddress; //메모리 할당
                var vmem_analyze_main_asm = Program.lr2helper.sharp.Memory.Allocate(256).BaseAddress; //메모리 할당

                var vmem_analyze_poor_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_fast_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_slow_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_perfect_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_main_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;

                Program.lr2helper.sharp.Assembly.Inject(
                    new string[] {
                        "jmp "+vmem_analyze_poor_asm
                    }, Program.lr2helper.prog_baseaddr + 0x643A);

                Program.lr2helper.sharp.Assembly.Inject(
                new String[]{
                    "mov [edi+97990],edx",
                    "mov [edi+207d4],ebp",
                    "mov ebp,[edi-1d4]",
                    "cmp [edi-1f0],1",
                    "je @ANALYZE_POOR2",
                    "jmp @ANALYZE_POOR3",

                    "@ANALYZE_POOR2:",
                    "mov [edi+a02e4],2",
                    "add [EDI+EBP*4+1E4C8],1",
                    "jmp @ANALYZE_POOR3",

                    "@ANALYZE_POOR3:",
                    "jmp 43C051"
                }, vmem_analyze_poor_asm);

                String[] str_vmem_analyze_total_asm = {

                };
                String[] str_vmem_analyze_main_asm = {

                };



            }
            
        }

    }
}
