using System;
using System.Runtime.InteropServices;
using MemPlus.Business.RAM;

namespace MemPlus.Business.UTILS
{
    /// <summary>
    /// Interaction logic for native methods
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///  Retrieves the locally unique identifier (LUID) used on a specified system to locally represent the specified privilege name.
        /// </summary>
        /// /// <param name="lpSystemName">A pointer to a null-terminated string that specifies the name of the system on which the privilege name is retrieved. If a null string is specified, the function attempts to find the privilege name on the local system</param>
        /// <param name="lpName">A pointer to a null-terminated string that specifies the name of the privilege, as defined in the Winnt.h header file. For example, this parameter could specify the constant, SE_SECURITY_NAME, or its corresponding string, "SeSecurityPrivilege"</param>
        /// <param name="pluid">A pointer to a variable that receives the LUID by which the privilege is known on the system specified by the lpSystemName parameter.</param>
        /// <returns>If the function succeeds, the function returns nonzero. Otherwise, return value will be zero</returns>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long pluid);
        /// <summary>
        /// Enables or disables privileges in the specified access token. Enabling or disabling privileges in an access token requires TOKEN_ADJUST_PRIVILEGES access
        /// </summary>
        /// <param name="tokenHandle">A handle to the access token that contains the privileges to be modified. The handle must have TOKEN_ADJUST_PRIVILEGES access to the token. If the PreviousState parameter is not NULL, the handle must also have TOKEN_QUERY access</param>
        /// <param name="disableAllPrivileges">Specifies whether the function disables all of the token's privileges. If this value is TRUE, the function disables all privileges and ignores the NewState parameter. If it is FALSE, the function modifies privileges based on the information pointed to by the NewState parameter</param>
        /// <param name="newState">A pointer to a TOKEN_PRIVILEGES structure that specifies an array of privileges and their attributes. If the DisableAllPrivileges parameter is FALSE, the AdjustTokenPrivileges function enables, disables, or removes these privileges for the token</param>
        /// <param name="bufferLength">Specifies the size, in bytes, of the buffer pointed to by the PreviousState parameter. This parameter can be zero if the PreviousState parameter is NULL</param>
        /// <param name="previousState">A pointer to a buffer that the function fills with a TOKEN_PRIVILEGES structure that contains the previous state of any privileges that the function modifies. That is, if a privilege has been modified by this function, the privilege and its previous state are contained in the TOKEN_PRIVILEGES structure referenced by PreviousState. If the PrivilegeCount member of TOKEN_PRIVILEGES is zero, then no privileges have been changed by this function. This parameter can be NULL</param>
        /// <param name="returnLength">A pointer to a variable that receives the required size, in bytes, of the buffer pointed to by the PreviousState parameter. This parameter can be NULL if PreviousState is NULL</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TokenPrivileges newState, int bufferLength, IntPtr previousState, IntPtr returnLength);
        /// <summary>Change Windows System parameters</summary>
        /// <param name="infoClass"></param>
        /// <param name="info"></param>
        /// <param name="length">Allocated bytes for the Info block</param>
        /// <returns>Opposite of boolean.  Zero means success, non-zero means fail and use GetLastError</returns>
        [DllImport("ntdll.dll")]
        internal static extern uint NtSetSystemInformation(int infoClass, IntPtr info, int length);
        /// <summary>
        /// Removes as many pages as possible from the working set of the specified process
        /// </summary>
        /// <param name="hwProc">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right and the PROCESS_SET_QUOTA access right</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("psapi.dll")]
        internal static extern int EmptyWorkingSet(IntPtr hwProc);
    }
}
