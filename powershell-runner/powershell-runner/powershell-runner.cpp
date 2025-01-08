// ConsoleApplication1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <windows.h>
#include <string>

std::string RunPowerShellScript(const char* script) {
    if (script == NULL) return "";
    std::string ret;

    // Create pipes for the child process's STDOUT and STDERR.
    HANDLE hReadPipe, hWritePipe;
    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(SECURITY_ATTRIBUTES);
    sa.bInheritHandle = TRUE; // Allow child processes to inherit the handle
    sa.lpSecurityDescriptor = NULL;

    // Create a pipe for STDOUT.
    if (!CreatePipe(&hReadPipe, &hWritePipe, &sa, 0)) {
        return "";
    }

    // Set up the STARTUPINFO structure.
    STARTUPINFOA si;
    PROCESS_INFORMATION pi;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    si.hStdOutput = hWritePipe; // Redirect STDOUT to the write end of the pipe
    si.hStdError = hWritePipe;   // Redirect STDERR to the write end of the pipe
    si.dwFlags |= STARTF_USESTDHANDLES;

    // Prepare the PowerShell command
    char command[512];
    snprintf(command, sizeof(command), "powershell -Command \"%s\"", script);

    // Create the child process.
    if (!CreateProcessA(NULL, command, NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi)) {
        CloseHandle(hReadPipe);
        CloseHandle(hWritePipe);
        return "";
    }

    // Close the write end of the pipe since we don't need it in the parent process.
    CloseHandle(hWritePipe);

    // Read the output from the pipe.
    char buffer[128];
    DWORD bytesRead;
    while (ReadFile(hReadPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL) && bytesRead > 0) {
        buffer[bytesRead] = '\0'; // Null-terminate the string
        ret += buffer;      // Print the output to the console
    }

    // Cleanup
    CloseHandle(hReadPipe);
    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    return ret;
}

std::string RunPowerShellScriptNew(const std::string script) {
    
    // Prepare the command
    std::string command = "powershell -Command \"" + script + "\"";

    // Set up security attributes for the pipe
    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(SECURITY_ATTRIBUTES);
    sa.bInheritHandle = TRUE;
    sa.lpSecurityDescriptor = NULL;

    // Create a pipe for the output
    HANDLE hReadPipe, hWritePipe;
    if (!CreatePipe(&hReadPipe, &hWritePipe, &sa, 0)) {
        std::cerr << "Failed to create pipe." << std::endl;
        return "";
    }

    // Set the write end of the pipe to be inheritable
    SetHandleInformation(hReadPipe, HANDLE_FLAG_INHERIT, 0);

    // Set up the process information
    STARTUPINFOA si;
    PROCESS_INFORMATION pi;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    si.hStdError = hWritePipe;
    si.hStdOutput = hWritePipe;
    si.dwFlags |= STARTF_USESTDHANDLES;

    std::string result;
    // Create the process
    if (CreateProcessA(NULL, const_cast<char*>(command.c_str()), NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi)) {
        // Close the write end of the pipe
        CloseHandle(hWritePipe);

        // Read the output from the pipe
        char buffer[128];        
        DWORD bytesRead;

        while (ReadFile(hReadPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL) && bytesRead > 0) {
            buffer[bytesRead] = '\0'; // Null-terminate the string
            result += buffer; // Append to result
        }

        // Wait for the process to finish
        WaitForSingleObject(pi.hProcess, INFINITE);

        // Close process and thread handles
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
    else {
        std::cerr << "CreateProcess failed (" << GetLastError() << ")." << std::endl;
    }

    // Close the read end of the pipe
    CloseHandle(hReadPipe);

    return result;
}

std::string replaceQuotes(const std::string& input) {
    std::string result = input;
    size_t pos = 0;

    while ((pos = result.find('"', pos)) != std::string::npos) {
        result.replace(pos, 1, "\\\"");
        pos += 2; // Move past the newly inserted escaped quote
    }

    return result;
}

int main()
{
    std::string script = R"(
        # Check the status of the Windows Firewall for the Public profile
        $publicFirewallStatus = Get-NetFirewallProfile -Profile Public| Select-Object Name, Enabled
        $domainFirewallStatus = Get-NetFirewallProfile -Profile Domain| Select-Object Name, Enabled
        $privateFirewallStatus = Get-NetFirewallProfile -Profile Private| Select-Object Name, Enabled
        # Display the status
        if ($domainFirewallStatus.Enabled -and $domainFirewallStatus.Enabled) 
        {
            Write-Output 'Firewall-ON'
        } 
        else 
        {
            Write-Output 'Firewall-OFF'
        }
    )";

    std::string script2 = R"(
        $isWin11 = (Get-WmiObject Win32_OperatingSystem).Caption -Match "Windows 11"
        echo $isWin11 
    )";

    std::string input = replaceQuotes(script);

    std::string r = RunPowerShellScriptNew(input);
    std::string ret = RunPowerShellScript(input.c_str());

    return 0;
}
