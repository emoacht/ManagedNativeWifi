using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo
{
	public static class Account
	{
		/// <summary>
		/// Checks if current user is a member of Administrators group.
		/// </summary>
		/// <returns>True if a member of the Administrators group</returns>
		/// <remarks>This method can tell whether current user is a member of Administrators group under UAC,
		/// even if current process is not elevated.</remarks>
		public static bool IsUserIsMemberOfAdmin()
		{
			using (var context = new PrincipalContext(ContextType.Machine))
			{
				// SID: S-1-5-32-544
				// Name: Administrators
				// Well-Known Security Identifiers
				// https://technet.microsoft.com/en-us/library/cc978401.aspx
				var administratorsGroupPrincipal = GroupPrincipal.FindByIdentity(context, "S-1-5-32-544");

				return UserPrincipal.Current.IsMemberOf(administratorsGroupPrincipal);
			}
		}
	}
}