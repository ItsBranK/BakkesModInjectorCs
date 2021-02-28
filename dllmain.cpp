#include <windows.h>
#include <string>
#include <fstream>
#include <iostream>
#include <sstream>
#include <thread>
#include <filesystem>

#include <cstdio>
#include <tlhelp32.h>

enum class InjectorResult : uint16_t
{
	RETRY_INJECTION = 0,
	FILE_NOT_FOUND = 1,
	PROCESS_NOT_FOUND = 2,
	PROCESS_NOT_SUPPORTED = 3,
	PROCESS_HANDLE_NOT_FOUND = 4,
	LOADLIBRARY_NOT_FOUND = 5,
	VIRTUAL_ALLOCATE_FAIL = 6,
	WRITE_MEMORY_FAIL = 7,
	CREATE_THREAD_FAIL = 8,
	SUCCESS = 9
};

enum class DirectoryReuslt : uint16_t
{
	NOT_LOADED = 0,
	APPDATA_NOT_FOUND = 1,
	BAKKESMOD_FOLDER_NOT_FOUND = 2,
	BAKKESMOD_DLL_NOT_FOUND = 3,
	SUCCESS = 4,
};

std::string GetInjectionResultString(const InjectorResult injectorResult)
{
	switch (injectorResult)
	{
	case InjectorResult::RETRY_INJECTION:
		return "Retry Injection";
		break;
	case InjectorResult::FILE_NOT_FOUND:
		return "File Not Found";
		break;
	case InjectorResult::PROCESS_NOT_FOUND:
		return "Process Not Found";
		break;
	case InjectorResult::PROCESS_NOT_SUPPORTED:
		return "Process Not Supported";
		break;
	case InjectorResult::PROCESS_HANDLE_NOT_FOUND:
		return "Process Handle Not Found";
		break;
	case InjectorResult::LOADLIBRARY_NOT_FOUND:
		return "LoadLibraryA Not Found";
		break;
	case InjectorResult::VIRTUAL_ALLOCATE_FAIL:
		return "Virtual Allocate Fail";
		break;
	case InjectorResult::WRITE_MEMORY_FAIL:
		return "Write Memory Fail";
		break;
	case InjectorResult::CREATE_THREAD_FAIL:
		return "Create Thread Fail";
		break;
	case InjectorResult::SUCCESS:
		return "Success";
		break;
	}

	return "Unknown";
}

class DirectoryLocations
{
private:
	bool DirectoriesLoaded = false;
	DirectoryReuslt DirectoriesResult = DirectoryReuslt::NOT_LOADED;

private:
	std::string RocketLeagueFolder;
	std::string AppDataFolder;
	std::string BakkesModFolder;
	std::string BakkesModDLL;

public:
	bool AreDirectoriesLoaded() const
	{
		return DirectoriesLoaded;
	}

	DirectoryReuslt GetDirectoryResult()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		return DirectoriesResult;
	}

	std::string GetDirectoryResultString()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		switch (DirectoriesResult)
		{
		case DirectoryReuslt::NOT_LOADED:
			return "Directories Not Loaded";
			break;
		case DirectoryReuslt::APPDATA_NOT_FOUND:
			return "AppData Not Found";
			break;
		case DirectoryReuslt::BAKKESMOD_FOLDER_NOT_FOUND:
			return "BakkesMod Folder Not Found";
			break;
		case DirectoryReuslt::BAKKESMOD_DLL_NOT_FOUND:
			return "BakkesMod DLL Not Found";
			break;
		case DirectoryReuslt::SUCCESS:
			return "Success";
			break;
		}

		return "Unknown";
	}

	std::string GetRocketLeagueFolder()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		if (DirectoriesResult == DirectoryReuslt::SUCCESS)
		{
			return RocketLeagueFolder;
		}

		return "null";
	}

	std::string GetAppDataFolder()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		if (DirectoriesResult == DirectoryReuslt::SUCCESS)
		{
			return AppDataFolder;
		}

		return "null";
	}

	std::string GetBakkesModFolder()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		if (DirectoriesResult == DirectoryReuslt::SUCCESS)
		{
			return BakkesModFolder;
		}

		return "null";
	}

	std::string GetBakkesModDLL()
	{
		if (!AreDirectoriesLoaded())
		{
			LoadDirectories();
		}

		if (DirectoriesResult == DirectoryReuslt::SUCCESS)
		{
			return BakkesModDLL;
		}

		return "null";
	}

private:
	void LoadDirectories()
	{
		DirectoriesLoaded = false;
		DirectoriesResult = DirectoryReuslt::NOT_LOADED;

		RocketLeagueFolder = std::filesystem::absolute(std::filesystem::path("")).u8string();

		std::filesystem::path appDataFolder = std::filesystem::absolute(std::filesystem::temp_directory_path().parent_path().parent_path().parent_path());

		if (std::filesystem::exists(appDataFolder))
		{
			AppDataFolder = appDataFolder.u8string();

			std::filesystem::path bakkesModFolder = appDataFolder.append("Roaming\\bakkesmod\\bakkesmod");

			if (std::filesystem::exists(bakkesModFolder))
			{
				BakkesModFolder = bakkesModFolder.u8string();

				std::filesystem::path bakkesModDLL = bakkesModFolder.append("dll\\bakkesmod.dll");

				if (std::filesystem::exists(bakkesModDLL))
				{
					BakkesModDLL = bakkesModDLL.u8string();

					DirectoriesLoaded = true;
					DirectoriesResult = DirectoryReuslt::SUCCESS;
				}
				else
				{
					DirectoriesLoaded = false;
					DirectoriesResult = DirectoryReuslt::BAKKESMOD_DLL_NOT_FOUND;
				}
			}
			else
			{
				DirectoriesLoaded = false;
				DirectoriesResult = DirectoryReuslt::BAKKESMOD_FOLDER_NOT_FOUND;
			}
		}
		else
		{
			DirectoriesLoaded = false;
			DirectoriesResult = DirectoryReuslt::APPDATA_NOT_FOUND;
		}
	}
};

InjectorResult InjectDLL(const std::string& filePath)
{
	PROCESSENTRY32 entry;
	entry.dwSize = sizeof(PROCESSENTRY32);

	HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

	if (Process32First(snapshot, &entry) == TRUE)
	{
		while (Process32Next(snapshot, &entry) == TRUE)
		{
			if (wcscmp(entry.szExeFile, L"RocketLeague.exe") == 0)
			{  
				LPVOID loadLibraryAddress = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryA");

				if (!loadLibraryAddress)
				{
					CloseHandle(snapshot);

					return InjectorResult::LOADLIBRARY_NOT_FOUND;
				}

				HANDLE processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, entry.th32ProcessID);

				if (!processHandle)
				{
					CloseHandle(snapshot);

					return InjectorResult::PROCESS_HANDLE_NOT_FOUND;
				}

				LPVOID allocatedAddress = VirtualAllocEx(processHandle, NULL, filePath.size(), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

				if (!allocatedAddress)
				{
					CloseHandle(processHandle);
					CloseHandle(snapshot);

					return InjectorResult::VIRTUAL_ALLOCATE_FAIL;
				}

				BOOL bWroteMemory = WriteProcessMemory(processHandle, allocatedAddress, filePath.c_str(), filePath.size(), NULL);

				if (!bWroteMemory)
				{
					CloseHandle(processHandle);
					CloseHandle(snapshot);

					return InjectorResult::WRITE_MEMORY_FAIL;
				}

				HANDLE threadHandle = CreateRemoteThread(processHandle, 0, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(loadLibraryAddress), allocatedAddress, 0, 0);

				if (threadHandle == NULL)
				{
					CloseHandle(processHandle);
					CloseHandle(snapshot);

					return InjectorResult::CREATE_THREAD_FAIL;
				}

				CloseHandle(threadHandle);
				CloseHandle(processHandle);
				CloseHandle(snapshot);

				return InjectorResult::SUCCESS;
			}
		}
	}

	CloseHandle(snapshot);

	return InjectorResult::PROCESS_NOT_FOUND;
}

void Initialize(HMODULE hModule)
{
	DisableThreadLibraryCalls(hModule);

	DirectoryLocations directoryLocations;

	if (directoryLocations.GetDirectoryResult() == DirectoryReuslt::SUCCESS)
	{
		InjectorResult injectionResult = InjectDLL(directoryLocations.GetBakkesModDLL());
		
		if (injectionResult != InjectorResult::SUCCESS)
		{
			std::string message = std::string("Always injected failed! Reason: ") + GetInjectionResultString(injectionResult);

			MessageBoxA(NULL, message.c_str(), "BakkesModInjectorCs Community Edition", MB_OK | MB_ICONEXCLAMATION);
		}
	}
	else
	{
		MessageBoxA(NULL, std::string("Always Injected failed! Reason: " + directoryLocations.GetDirectoryResultString()).c_str(), "BakkesModInjectorCs Community Edition", MB_OK | MB_ICONEXCLAMATION);
	}
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		Initialize(hModule);
		break;
	case DLL_THREAD_ATTACH:	
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}