#define KEY_TOGGLE VK_END
#define KEY_INCREASE VK_UP
#define KEY_INCREASE_SMALL VK_RIGHT
#define KEY_DECREASE VK_DOWN
#define KEY_DECREASE_SMALL VK_LEFT
#define FPS_TARGET 144

// ����������Ķ����������㶮

#ifdef WIN32
#error You must build in x64
#endif

#include <Windows.h>
#include <TlHelp32.h>
#include <vector>
#include <string>
#include <thread>
#include <Psapi.h>
#include <direct.h>
#include "inireader.h"
#include <iostream>
#include <sstream>
using namespace std;

std::string GamePath{};
int FpsValue = FPS_TARGET;

// �������� - ������д�� - �������Ŀ���
uintptr_t PatternScan(void* module, const char* signature)
{
	static auto pattern_to_byte = [](const char* pattern) {
		auto bytes = std::vector<int>{};
		auto start = const_cast<char*>(pattern);
		auto end = const_cast<char*>(pattern) + strlen(pattern);

		for (auto current = start; current < end; ++current) {
			if (*current == '?') {
				++current;
				if (*current == '?')
					++current;
				bytes.push_back(-1);
			}
			else {
				bytes.push_back(strtoul(current, &current, 16));
			}
		}
		return bytes;
	};

	auto dosHeader = (PIMAGE_DOS_HEADER)module;
	auto ntHeaders = (PIMAGE_NT_HEADERS)((std::uint8_t*)module + dosHeader->e_lfanew);

	auto sizeOfImage = ntHeaders->OptionalHeader.SizeOfImage;
	auto patternBytes = pattern_to_byte(signature);
	auto scanBytes = reinterpret_cast<std::uint8_t*>(module);

	auto s = patternBytes.size();
	auto d = patternBytes.data();

	for (auto i = 0ul; i < sizeOfImage - s; ++i) {
		bool found = true;
		for (auto j = 0ul; j < s; ++j) {
			if (scanBytes[i + j] != d[j] && d[j] != -1) {
				found = false;
				break;
			}
		}
		if (found) {
			return (uintptr_t)&scanBytes[i];
		}
	}
	return 0;
}

std::string GetLastErrorAsString(DWORD code)
{
	LPSTR buf = nullptr;
	FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, code, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&buf, 0, NULL);
	std::string ret = buf;
	LocalFree(buf);
	return ret;
}

// ��ȡĿ�����DLL��Ϣ
bool GetModule(DWORD pid, std::string ModuleName, PMODULEENTRY32 pEntry)
{
	if (!pEntry)
		return false;

	MODULEENTRY32 mod32{};
	mod32.dwSize = sizeof(mod32);
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
	for (Module32First(snap, &mod32); Module32Next(snap, &mod32);)
	{
		if (mod32.th32ProcessID != pid)
			continue;

		if (mod32.szModule == ModuleName)
		{
			*pEntry = mod32;
			break;
		}
	}
	CloseHandle(snap);

	return pEntry->modBaseAddr;
}

// ͨ����������������ID
DWORD GetPID(std::string ProcessName)
{
	DWORD pid = 0;
	PROCESSENTRY32 pe32{};
	pe32.dwSize = sizeof(pe32);
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	for (Process32First(snap, &pe32); Process32Next(snap, &pe32);)
	{
		if (pe32.szExeFile == ProcessName)
		{
			pid = pe32.th32ProcessID;
			break;
		}
	}
	CloseHandle(snap);
	return pid;
}

void LoadConfig()
{
	printf("\n�ȴ���Ϸ����...\n");
	DWORD pid = 0;
	while (!(pid = GetPID("YuanShen.exe")) && !(pid = GetPID("GenshinImpact.exe")))
		std::this_thread::sleep_for(std::chrono::milliseconds(200));

	// ��ȡ���̾�� - ��Ȩ�޺ܵ͵��� - ��Ӧ�û�ȡ����
	// PROCESS_QUERY_LIMITED_INFORMATION - ���ڲ�ѯ����·�� (K32GetModuleFileNameExA)
	// SYNCHRONIZE - ���ڵȴ����̽��� (WaitForSingleObject)
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION | SYNCHRONIZE, FALSE, pid);
	if (!hProcess)
	{
		DWORD code = GetLastError();
		printf("OpenProcess failed (%d): %s", code, GetLastErrorAsString(code).c_str());
		return;
	}

	char szPath[MAX_PATH]{};
	DWORD length = sizeof(szPath);
	QueryFullProcessImageNameA(hProcess, 0, szPath, &length);

	GamePath = szPath;

	HWND hwnd = nullptr;
	while (!(hwnd = FindWindowA("UnityWndClass", nullptr)))
		std::this_thread::sleep_for(std::chrono::milliseconds(200));

	DWORD ExitCode = STILL_ACTIVE;
	while (ExitCode == STILL_ACTIVE)
	{
		SendMessageA(hwnd, WM_CLOSE, 0, 0);
		GetExitCodeProcess(hProcess, &ExitCode);
		std::this_thread::sleep_for(std::chrono::milliseconds(200));
	}

	// wait for the game to close then continue
	WaitForSingleObject(hProcess, -1);
	CloseHandle(hProcess);

	system("cls");
	return;
}

int main(int argc, char** argv)
{
	HWND testwindow;//�������ھ��
	testwindow = GetForegroundWindow();//��ǰ���ھ��
	ShowWindow(testwindow, SW_HIDE);//���ش���
	std::atexit([] {
		system("pause");
		});

	SetConsoleTitleA("");
	char exePath[MAX_PATH];
	getcwd(exePath, MAX_PATH);
	char sett[] = "\\Config\\Setting.ini";
	char Setfile[500];
	strcpy(Setfile, exePath);
	strcat(Setfile, sett);
	INIReader reader(Setfile);
	int GameWidth = reader.GetInteger("setup", "Width", 1280);
	int GameHeight = reader.GetInteger("setup", "Height", 720);
	FpsValue = reader.GetInteger("setup", "MaxFps", 144);
	string fullscreen = "0";
	string Popup = "";
	string isAutoSize = reader.Get("setup", "isAutoSize", "");
	string isPopup = reader.Get("setup", "isPopup", "");
	if (isAutoSize != "False")
		fullscreen = "1";
	if (isPopup != "False")
		Popup = " -popupwindow ";
	ostringstream comm;
	comm << " -screen-height " << GameHeight << " -screen-width " << GameWidth << " -screen-fullscreen " << fullscreen << Popup << endl;
	std::string CommandLine{ comm.str() };
	if (argc > 1)
	{
		for (int i = 1; i < argc; i++)
			CommandLine += argv[i] + std::string(" ");
	}
	LoadConfig();
	int TargetFPS = FpsValue;
	std::string ProcessPath = GamePath;
	std::string ProcessDir{};

	if (ProcessPath.length() < 8)
		return 0;

	printf("FPS ������\n");

	printf("��Ϸ���:%d\n��Ϸ�߶�:%d\n�Ƿ�ȫ��:%s\n�Ƿ��ޱ߿�:%s\n", GameWidth, GameHeight, isAutoSize, isPopup);
	ProcessDir = ProcessPath.substr(0, ProcessPath.find_last_of("\\"));

	DWORD pid = GetPID(ProcessPath.substr(ProcessPath.find_last_of("\\") + 1));
	if (pid)
	{
		printf("��⵽��Ϸ�������У����ֶ��ر���Ϸ������������\n");
		return 0;
	}

	STARTUPINFOA si{};
	PROCESS_INFORMATION pi{};
	if (!CreateProcessA(ProcessPath.c_str(), (LPSTR)CommandLine.c_str(), nullptr, nullptr, FALSE, 0, nullptr, ProcessDir.c_str(), &si, &pi))
	{
		DWORD code = GetLastError();
		printf("CreateProcess failed (%d): %s", code, GetLastErrorAsString(code).c_str());
		return 0;
	}

	CloseHandle(pi.hThread);
	printf("PID: %d\n", pi.dwProcessId);

	// �ȴ�UnityPlayer.dll���غͻ�ȡDLL��Ϣ
	MODULEENTRY32 hUnityPlayer{};
	while (!GetModule(pi.dwProcessId, "UnityPlayer.dll", &hUnityPlayer))
		std::this_thread::sleep_for(std::chrono::milliseconds(100));

	printf("UnityPlayer: %X%X\n", (uintptr_t)hUnityPlayer.modBaseAddr >> 32 & -1, hUnityPlayer.modBaseAddr);

	// �ڱ�����������UnityPlayer.dll��С���ڴ� - ������������
	LPVOID mem = VirtualAlloc(nullptr, hUnityPlayer.modBaseSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (!mem)
	{
		DWORD code = GetLastError();
		printf("VirtualAlloc failed (%d): %s", code, GetLastErrorAsString(code).c_str());
		return 0;
	}

	// ������ģ�������
	ReadProcessMemory(pi.hProcess, hUnityPlayer.modBaseAddr, mem, hUnityPlayer.modBaseSize, nullptr);

	printf("Searching for pattern...\n");
	/*
		 7F 0F              jg   0x11
		 8B 05 ? ? ? ?      mov eax, dword ptr[rip+?]
	*/
	uintptr_t address = PatternScan(mem, "7F 0F 8B 05 ? ? ? ?");
	if (!address)
	{
		printf("outdated pattern\n");
		return 0;
	}

	// ������Ե�ַ (FPS)
	uintptr_t pfps = 0;
	{
		uintptr_t rip = address + 2;
		uint32_t rel = *(uint32_t*)(rip + 2);
		pfps = rip + rel + 6;
		pfps -= (uintptr_t)mem;
		printf("FPS Offset: %X\n", pfps);
		pfps = (uintptr_t)hUnityPlayer.modBaseAddr + pfps;
	}

	// ������Ե�ַ (��ֱͬ��)
	address = PatternScan(mem, "E8 ? ? ? ? 8B E8 49 8B 1E");
	uintptr_t pvsync = 0;
	if (address)
	{
		uintptr_t ppvsync = 0;
		uintptr_t rip = address;
		int32_t rel = *(int32_t*)(rip + 1);
		rip = rip + rel + 5;
		uint64_t rax = *(uint32_t*)(rip + 3);
		ppvsync = rip + rax + 7;
		ppvsync -= (uintptr_t)mem;
		printf("VSync Offset: %X\n", ppvsync);
		ppvsync = (uintptr_t)hUnityPlayer.modBaseAddr + ppvsync;

		uintptr_t buffer = 0;
		while (!buffer)
		{
			ReadProcessMemory(pi.hProcess, (LPCVOID)ppvsync, &buffer, sizeof(buffer), nullptr);
			std::this_thread::sleep_for(std::chrono::milliseconds(100));
		}

		rip += 7;
		pvsync = *(uint32_t*)(rip + 2);
		pvsync = buffer + pvsync;
	}

	VirtualFree(mem, 0, MEM_RELEASE);
	printf("Done\n\n");

	DWORD dwExitCode = STILL_ACTIVE;
	while (dwExitCode == STILL_ACTIVE)
	{
		GetExitCodeProcess(pi.hProcess, &dwExitCode);

		// ÿ������һ��
		std::this_thread::sleep_for(std::chrono::seconds(2));
		int fps = 0;
		ReadProcessMemory(pi.hProcess, (LPVOID)pfps, &fps, sizeof(fps), nullptr);
		if (fps == -1)
			continue;
		if (fps != TargetFPS)
			WriteProcessMemory(pi.hProcess, (LPVOID)pfps, &TargetFPS, sizeof(TargetFPS), nullptr);

		int vsync = 0;
		ReadProcessMemory(pi.hProcess, (LPVOID)pvsync, &vsync, sizeof(vsync), nullptr);
		if (vsync)
		{
			vsync = 0;
			// �رմ�ֱͬ��
			WriteProcessMemory(pi.hProcess, (LPVOID)pvsync, &vsync, sizeof(vsync), nullptr);
		}
	}

	CloseHandle(pi.hProcess);
	TerminateProcess((HANDLE)-1, 0);

	return 0;
}