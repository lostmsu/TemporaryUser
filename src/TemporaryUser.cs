namespace TemporaryUser;

using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;

using Windows.Win32;
using Windows.Win32.Foundation;

[SupportedOSPlatform("windows7.0")]
public sealed class TemporaryUser: IDisposable {
    const string USERS_GROUP_SID = "S-1-5-32-545";
    readonly PrincipalContext context;
    readonly UserPrincipal user;

    public SecurityIdentifier SID => this.user.Sid;
    public DirectoryInfo ProfilePath { get; }

    public TemporaryUser(string name, string password) {
        this.context = new PrincipalContext(ContextType.Machine);
        try {
            using var users =
                GroupPrincipal.FindByIdentity(this.context, IdentityType.Sid, USERS_GROUP_SID)
             ?? throw new KeyNotFoundException("Unable to find Users group by SID");

            this.user = new UserPrincipal(this.context, samAccountName: name, password: password,
                                          enabled: true);
            this.user.UserCannotChangePassword = true;
            this.user.PasswordNeverExpires = true;
            this.user.Save();

            string profilePath;
            try {
                if (this.user.SamAccountName != name)
                    throw new InvalidProgramException();

                users.Members.Add(this.user);
                users.Save();

                profilePath = CreateProfile(this.user);
            } catch (Exception) {
                Delete(this.user, this.context);
                throw;
            }
            this.ProfilePath = new(profilePath);
        } catch (Exception) {
            this.context.Dispose();
            throw;
        }
    }

    static unsafe string CreateProfile(UserPrincipal user) {
        char* profilePath = stackalloc char[260];
        PInvoke.CreateProfile(pszUserSid: user.Sid.Value, pszUserName: user.SamAccountName,
                              pszProfilePath: new PWSTR(profilePath),
                              cchProfilePath: 256).ThrowOnFailure();
        return new(profilePath);
    }

    static void Delete(UserPrincipal user, PrincipalContext context) {
        if (!PInvoke.DeleteProfile(user.Sid.Value, null, null)) {
            var error = new Win32Exception();
            if ((WIN32_ERROR)error.NativeErrorCode != WIN32_ERROR.ERROR_FILE_NOT_FOUND)
                throw error;
        }
        user.Delete();
        user.Dispose();
        context.Dispose();
    }

    public void Dispose() {
        try {
            Delete(this.user, this.context);
        } catch (ObjectDisposedException) { }
    }
}