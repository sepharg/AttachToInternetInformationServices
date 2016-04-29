using System;

namespace AttachToInternetInformationServices
{
    internal static class Guids
    {
        public const string GuidiisAttachPkgString = "40d9f297-25fb-4264-99ed-7785f8331c94";
        public const string GuidiisAttachCmdSetString = "B2C8E135-0E7A-4696-963E-BD3280F8578C";

        public static readonly Guid GuidiisAttachPkg = new Guid(GuidiisAttachPkgString);
        public static readonly Guid GuidiisAttachCmdSet = new Guid(GuidiisAttachCmdSetString);
    };
}