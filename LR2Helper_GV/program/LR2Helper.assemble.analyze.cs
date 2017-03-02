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

                var vmem_analyze_note_timing_asm = Program.lr2helper.sharp.Memory.Allocate(64).BaseAddress; //노트 타이밍 긁어올 코드
                var vmem_analyze_note_timing_p_val = Program.lr2helper.sharp.Memory.Allocate(4).BaseAddress; //노트 타이밍 저장하는 위치
                var vmem_analyze_note_timing_t_val = Program.lr2helper.sharp.Memory.Allocate(4).BaseAddress; //노트 타이밍 저장하는 위치

                var vmem_analyze_poor_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_fast_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_slow_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_perfect_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;
                var vmem_analyze_main_val = Program.lr2helper.sharp.Memory.Allocate(2048).BaseAddress;

                Program.lr2helper.sharp.Assembly.Inject(
                new string[] {
                    "jmp "+vmem_analyze_note_timing_asm
                }, Program.lr2helper.prog_baseaddr + 0x18a24); //판정시간을 가져올 주소
                Program.lr2helper.sharp.Assembly.Inject(
                    new string[] {
                        "mov ebp,[esp+30]", // 기존에 있던 코드 복구, 기준 판정 시간 
                        "mov ecx,[esp+10]", // 기존에 있던 코드 복구, 누른 판정 시간
                        "mov ["+vmem_analyze_note_timing_p_val+"], ebp", //저장1
                        "mov ["+vmem_analyze_note_timing_t_val+"], ebp", //저장2
                        "jmp 418a2c" //원래 코드로 jmp
                    }, vmem_analyze_note_timing_asm);

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

                /*
                      406440:
      jmp ANALYZE_POOR

      ANALYZE_POOR:
      mov [edi+207d4],ebp
      mov ebp,[edi-1d4]
      cmp [edi-1f0],1
      je ANALYZE_POOR2
      jmp ANALYZE_POOR3

      ANALYZE_POOR2:
      mov [edi+a02e4],2
      add [EDI+EBP*4+1E4C8],1
      jmp ANALYZE_POOR3

      ANALYZE_POOR3:
      jmp 43C051

      408255:
      JMP JUDGE_BAR_2

      JUDGE_BAR_2:
      mov [ESI+20714],EAX
      mov EAX,[ESI+21EE0]
      MOV [ESI+EAX*4+213CC],2
      add [MUSICBAR_POOR+EAX*4],1
      ADD [ESI+A02F0],1
      mov EAX,[ESI+20714]
      jmp 40829A

      40615D:
      jmp ANALYZE_TOTAL
      nop

      4BA713:
      nop
      nop
      nop

      ANALYZE_TOTAL:
      add [eax+97a6c],edx
      mov ecx,[eax-1d4]
      cmp ecx,20
      jl ANALYZE_TOTAL2
      jmp 406163

      ANALYZE_TOTAL2:
      add [eax+ecx*4+1DEC8],1
      jmp 406163

      418840:
      jmp ANALYZE_1

      ANALYZE_1:
      mov ecx,[eax-1d4]
      mov [eax+207c8],esi
      mov [eax+207d0],edi
      mov edi,[eax+27ac8]
      mov esi,[eax+27acc]

      jmp FS_FREQ_1

      FS_FREQ_1:
      sub edi,esi
      add edi,#100
      cmp edi,0
      jl FS_FREQ_TOO_FAST
      cmp edi,#199
      jg FS_FREQ_TOO_SLOW
      FS_FREQ_2:
      shr edi,1
      add [FS_FREQ+edi*4],1
      mov esi,[FS_FREQ+edi*4]
      cmp esi,#10
      jl FS_FREQ_3
      add [FS_FREQ2+edi*4],1
      mov [FS_FREQ+edi*4],0
      mov esi,[FS_FREQ2+edi*4]
      cmp esi,#9
      jl FS_FREQ_3
      mov [FS_FREQ2+edi*4],9
      FS_FREQ_3:
      mov edi,[eax+27ac8]
      mov esi,[eax+27acc]
      jmp ANALYZE_1_1

      FS_FREQ_TOO_FAST:
      mov edi,[eax+27ac8]
      jmp ANALYZE_1_1
      FS_FREQ_TOO_SLOW:
      mov edi,[eax+27ac8]
      jmp ANALYZE_1_1

      ANALYZE_1_1:
      cmp edx,4
      jnl ANALYZE_1_1_1
      jmp ANALYZE_1_2

      ANALYZE_1_1_1:
      add [eax+ecx*4+1EAC8],1
      cmp edx,5
      je ANALYZE_1_1_2
      jmp ANALYZE_1_2

      ANALYZE_1_1_2:
      add [eax+ecx*4+1EAC8],1
      jmp ANALYZE_1_2

      ANALYZE_1_2:
      shl edx,7
      add eax,edx
      cmp edi,esi
      jg ANALYZE_S
      jl ANALYZE_F
      jmp ANALYZE_P
      ANALYZE_S:
      add [eax+ecx*4+1E4C8],1
      jmp ANALYZE_END
      ANALYZE_F:
      add [eax+ecx*4+1E1C8],1
      jmp ANALYZE_END
      ANALYZE_P:
      add [eax+ecx*4+1E7C8],1
      jmp ANALYZE_END

      ANALYZE_END:
      sub eax,edx
      shr edx,7
      mov esi,[eax+207c8]
      mov edi,[eax+207d0]
      JMP MUSICBAR_GETVALUE

      MUSICBAR_GETVALUE:
      mov [MUSICBAR_BACKUP_1],ECX
      mov [MUSICBAR_BACKUP_2],EBX
      mov ECX,[EAX+21EE0]

      cmp edx,0
      jg MUSICBAR_GET_POOR
      jmp MUSICBAR_GETVALUE_RETURN
      MUSICBAR_GET_POOR:
      add [MUSICBAR_POOR+ECX*4],1
      mov EBX,[MUSICBAR_POOR+ECX*4]
      cmp edx,1
      jg MUSICBAR_GET_BAD
      jmp MUSICBAR_GETVALUE_RETURN
      MUSICBAR_GET_BAD:
      add [MUSICBAR_BAD+ECX*4],1
      mov EBX,[MUSICBAR_BAD+ECX*4]
      cmp edx,2
      jg MUSICBAR_GET_GOOD
      jmp MUSICBAR_GETVALUE_RETURN
      MUSICBAR_GET_GOOD:
      add [MUSICBAR_GOOD+ECX*4],1
      mov EBX,[MUSICBAR_GOOD+ECX*4]
      cmp edx,3
      jg MUSICBAR_GET_GREAT
      jmp MUSICBAR_GETVALUE_RETURN
      MUSICBAR_GET_GREAT:
      add [MUSICBAR_GREAT+ECX*4],1
      mov EBX,[MUSICBAR_GREAT+ECX*4]
      cmp edx,4
      jg MUSICBAR_GET_PGREAT
      jmp MUSICBAR_GETVALUE_RETURN
      MUSICBAR_GET_PGREAT:
      add [MUSICBAR_PGREAT+ECX*4],1
      mov EBX,[MUSICBAR_PGREAT+ECX*4]
      MUSICBAR_GETVALUE_RETURN:
      cmp edx,0
      JE MUSICBAR_GETVALUE_RETURN2
      cmp [MUSICBAR_MAXVALUE],EBX
      JGE MUSICBAR_GETVALUE_RETURN2
      mov [MUSICBAR_MAXVALUE],EBX
      MUSICBAR_GETVALUE_RETURN2:
      mov ECX,[MUSICBAR_BACKUP_1]
      mov EBX,[MUSICBAR_BACKUP_2]
      jmp 7343f3

                */

            }

        }

    }
}
