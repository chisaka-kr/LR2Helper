using System;

namespace LR2Helper_GV.program {
    public partial class LR2helper {
        //greenvalue.cs
        private void InitGreenvalue() {
            vmem_dstnumber_300_asm = sharp.Memory.Allocate(256).BaseAddress;
            vmem_dstnumber_300_reg = sharp.Memory.Allocate(128).BaseAddress;
            String[] str_vmem_dstnumber_300_asm =
            {
                    "mov eax,[esp+0x8]",
                    "sub esp,0x20",
                    "sub eax,0xa",
                    "push esi",
                    "push edi",
                    "cmp eax,0x122", //demical 290
                    "jge @new_number1",
                    "jmp "+(prog_baseaddr+0x24dc),
                    "@new_number1:",
                    "mov ecx,[esp+0x2c]",
                    "cmp eax,0x127", //decmial 295
                    "jl @new_number2",
                    "mov eax,[ecx+eax*4+0x1eb1c]",
                    "@new_number3:",
                    "pop edi",
                    "pop esi",
                    "add esp,0x20",
                    "retn",
                    "@new_number2:",
                    "cmp eax, 0x123", //decimal 291
                    "je @new_number3",
                    "sub eax, 0x122",
                    "mov eax,["+ vmem_dstnumber_300_reg+"+eax*0x4]",
                    "pop edi",
                    "pop esi",
                    "add esp,0x20",
                    "retn"
                };
            String[] str_vmem_dstnumber_300_jmp =
            {
                    "jmp "+vmem_dstnumber_300_asm
                };

            sharp.Assembly.Inject(str_vmem_dstnumber_300_asm, vmem_dstnumber_300_asm); // injection
            sharp.Assembly.Inject(str_vmem_dstnumber_300_jmp, prog_baseaddr + 0x24d0); // injection
        }
        private void GetGreenvalue() {

            //계산에 필요한 변수 긁어오기
            LR2value.bpm = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x97950), false);
            LR2value.lanecover = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x20), false);
            LR2value.hispeed = sharp.Read<int>((IntPtr)LR2value.baseaddr, false);
            LR2value.hispeed_option = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x48), false);
            LR2value.bpm_power = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x9FCE0), false);
            LR2value.scrollspeed = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x98), false);
            //여기까지 녹숫 계산을 위한 변수들

            if (flag_resolution_manual_mode == 0) {
                LR2value.resolution_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x61700), false);
                LR2value.resolution_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f), false);
                LR2value.window_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f0), false);
                LR2value.window_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616ec), false);
                LR2value.fullscreen_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x60890), false);
                LR2value.fullscreen_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x6088c), false);
                //보험용
                if ((LR2value.window_width == 0) || (LR2value.window_height == 0)) {
                    LR2value.resolution_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x61700 - 0x60), false);
                    LR2value.resolution_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616fc - 0x60), false);
                    LR2value.window_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f0 - 0x60), false);
                    LR2value.window_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616ec - 0x60), false);
                    LR2value.fullscreen_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x60890 - 0x60), false);
                    LR2value.fullscreen_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x6088c - 0x60), false);
                }
            }
            //여기까지 흰숫 계산을 위한 변수들

            //만약 window_width나 height가 0일 경우 풀스크린으로 계산
            if ((LR2value.window_height == 0) || (LR2value.window_width == 0)) {
                LR2value.window_width = LR2value.fullscreen_width;
                LR2value.window_height = LR2value.fullscreen_height;
            }
            //하이스피드 고정 옵션이 있다면 보정한다
            if ((LR2value.hispeed_option > 0) && (LR2value.hispeed_option < 4)) {
                LR2value.cal_bpm = Convert.ToInt32(LR2value.bpm_power * (int)LR2value.bpm);
            } else if (LR2value.hispeed_option == 4) {
                LR2value.cal_bpm = 150;
            } else {
                LR2value.cal_bpm = (int)LR2value.bpm;
            }

            if (LR2value.cal_bpm == 0) {
                LR2value.cal_bpm = 150;
            }
            LR2value.green_number = 10 * ((2173.0 / 725.0) * 1000) * (LR2value.dst_y) / (LR2value.hispeed * LR2value.scrollspeed) * (150.0 / LR2value.cal_bpm) * (1.0 - (LR2value.lanecover / 100.0));
            //게산식 : 10*((2173/725)*1000)*(DST_Y)/(HISPD*SCRSPD)*(150/BPM)*(1-(LANECOVER/100))
            //white_number = (1.0 - ((288.0 * dst_y * (LR2value.window_width * 1.0 / LR2value.window_height * 1.0)) / (482.0 * dst_x * (LR2value.resolution_width * 1.0 / LR2value.resolution_height * 1.0))) * (1.0 - (LR2value.lanecover * 1.0 / dst_y * 1.0))) * 1000.0;
            LR2value.white_number = (1.0 - ((288.0 * LR2value.dst_y * (LR2value.window_width * 1.0 / LR2value.window_height * 1.0)) / (482.0 * LR2value.dst_x * (LR2value.resolution_width * 1.0 / LR2value.resolution_height * 1.0))) * (1.0 - (LR2value.lanecover * 1.0 / 100 * 1.0))) * 1000.0;
            //계산식 : ((1-((288*DST_Y*(WINDOW_W/WINDOW_H))/(482*DST_X*(RESOL_W/RESOL_H)))*(1-(LANECOVER/DST_Y)))*1000)

            if (LR2value.scene == 4) {
                Program.runningForm.SetTooltipStrip("Green Number: " + Convert.ToInt32(LR2value.green_number).ToString() + " White Number: " + Convert.ToInt32(LR2value.white_number).ToString());
            }

            //302와 303에 녹숫과 흰숫을 쓴다. 참고로 vmem_dstnumber_300_reg는 300번 주소를 가리킨다.
            sharp.Write<int>(vmem_dstnumber_300_reg + 0x8, Convert.ToInt32(LR2value.green_number), false);
            sharp.Write<int>(vmem_dstnumber_300_reg + 0xC, Convert.ToInt32(LR2value.white_number), false);

            //unsupported skin mode가 on이면 fps에 녹숫을 덮어씌운다
            if (flag_unsupportedskinmode == 1) {
                sharp.Write<double>((IntPtr)(LR2value.baseaddr + 0x20E08), Convert.ToInt32(LR2value.green_number), false);
            }
        }
    }
}
