using Sisters.WudiLib.Posts;

namespace OsuQqBot.AttributedFunctions
{
    [Function]
    internal class ApproveInvitationFromAdmin : IGroupInvitation
    {
        public GroupRequestEventHandler GroupInvitation => (api, request) =>
        {
            long developer = OsuQqBot.IdWhoLovesInt100Best;
            if (request.UserId == developer) return new GroupRequestResponse()
            {
                Approve = true,
            };
            return null;
        };
    }
}
