import type { UserInChatInfoMemberList } from "./userInChatInfoMemberList";

export type ConfirmAction =
    { type: "PromoteUser"; user: UserInChatInfoMemberList; }
    | { type: "DemoteUser"; user: UserInChatInfoMemberList; }
    | { type: "RemoveUser"; user: UserInChatInfoMemberList; }
    | { type: "LeaveGroup"; }
    | { type: "DeleteGroup"; };