import { useRef, useState } from "react";
import { fetchWrapper } from "../api/fetchWrapper";
import "../styles/ChatInfoInterface.css";
import type { Chat, ChatInfoType } from "../types/chat";
import type { ConfirmAction } from "../types/confirmAction";
import type { GroupAction } from "../types/groupAction";
import type { User } from "../types/user";
import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";
import ConfirmInterface from "./ConfirmInterface";
import UserSearch from "./UserSearch";
import type { UserOnChatCreate } from "../types/userOnChatCreate";
import { addUsers } from "../api/addUsers";

function ChatInfoInterface({ currentUser, currentUserIsAdmin, selectedChat, chatMembers, onMemberRemoved, onMemberPromoted, onMemberDemoted, onReturnButtonClicked, onRemoveChatById, onAddUsers }:
    {
        currentUser: User; currentUserIsAdmin: boolean, selectedChat: Chat; chatMembers: UserInChatInfoMemberList[], onMemberRemoved: (user: UserInChatInfoMemberList) => void,
        onMemberPromoted: (user: UserInChatInfoMemberList) => void, onMemberDemoted: (user: UserInChatInfoMemberList) => void, onReturnButtonClicked: () => void; onRemoveChatById: (chatId: string) => void;
        onAddUsers: (usersToAdd: UserInChatInfoMemberList[]) => void;
    }) {

    const [actionToConfirm, setActionToConfirm] = useState<ConfirmAction | null>(null);
    const [selectedGroupAction, setSelectedGroupAction] = useState<GroupAction>("Organize");
    const [usersToAdd, setUsersToAdd] = useState<UserOnChatCreate[]>([]);
    const [chatMemberError, setChatMemberError] = useState<string | null>(null);

    const removeUsersRef = useRef<((userIdsToRemove: string[]) => void) | null>(null);

    async function handlePromoteUserButtonClicked(user: UserInChatInfoMemberList) {
        try {
            await fetchWrapper("/api/Chat/" + selectedChat.chatId + "/promote/" + user.userId, { method: "PATCH" });

            onMemberPromoted(user);
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    async function handleDemoteUserButtonClicked(user: UserInChatInfoMemberList) {
        try {
            await fetchWrapper("/api/Chat/" + selectedChat.chatId + "/demote/" + user.userId, { method: "PATCH" });

            onMemberDemoted(user);
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    async function handleRemoveUserButtonClicked(user: UserInChatInfoMemberList) {
        try {
            await fetchWrapper("/api/Chat/" + selectedChat.chatId + "/remove/" + user.userId, { method: "DELETE" });

            onMemberRemoved(user);
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    async function handleLeaveGroupButton() {
        try {
            await fetchWrapper("/api/Chat/" + selectedChat.chatId + "/leave", { method: "DELETE" });

            onRemoveChatById(selectedChat.chatId);
            onReturnButtonClicked();
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    async function handleDeleteGroupButton() {
        try {
            await fetchWrapper("/api/Chat/" + selectedChat.chatId + "/delete", { method: "DELETE" });

            onRemoveChatById(selectedChat.chatId);
            onReturnButtonClicked();
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    async function handleConfirm() {
        if (!actionToConfirm) return;

        switch (actionToConfirm.type) {
            case "PromoteUser":
                await handlePromoteUserButtonClicked(actionToConfirm.user);
                break;
            case "DemoteUser":
                await handleDemoteUserButtonClicked(actionToConfirm.user);
                break;
            case "RemoveUser":
                await handleRemoveUserButtonClicked(actionToConfirm.user);
                break;
            case "LeaveGroup":
                await handleLeaveGroupButton();
                break;
            case "DeleteGroup":
                await handleDeleteGroupButton();
                break;
        }
        setActionToConfirm(null);
    }

    async function handleAddUsers() {
        try {
            if (usersToAdd.length === 0) {
                setChatMemberError("Es muss mindestens ein Mitglied angegeben werden!");
                return;
            }

            const users = await addUsers(selectedChat.chatId, usersToAdd);

            if (removeUsersRef.current)
                removeUsersRef.current(usersToAdd.map(u => u.userId));

            onAddUsers(users);
            setUsersToAdd([]);
        }
        catch {
            // wird in fetchWrapper gehandelt
        }
    }

    function handleChatInfoTypeSwitch(chatInfoType: ChatInfoType) {
        setSelectedGroupAction(chatInfoType);
        setChatMemberError(null);
    }

    return (
        <>
            {actionToConfirm != null && <ConfirmInterface action={actionToConfirm} onConfirm={() => handleConfirm()} onCancel={() => setActionToConfirm(null)} />}
            <div className="chatInfoInterfaceBackground" onClick={onReturnButtonClicked}>
                <div className={`chatInfoInterfaceWrapper ${selectedChat.chatType === "Direct" ? "directChat" : ""}`} onClick={(e) => e.stopPropagation()}>

                    <div className="chatInfoHeaderBar">
                        <h2 className="chatInfoHeaderTitle">{selectedChat.name}</h2>
                        <span className="chatInfoHeaderSubtitle">
                            {selectedChat.chatType === "Groupchat"
                                ? `${chatMembers.length} Mitglieder`
                                : "Direktchat"}
                        </span>
                    </div>

                    {selectedChat.chatType === "Groupchat" && (
                        <>
                            {selectedChat.description && (
                                <p className="chatInfoDescription">{selectedChat.description}</p>
                            )}

                            <div className="groupActionSwitcher">
                                <div className={`pillGlider ${selectedGroupAction === "AddUser" ? "addUser" : ""}`}></div>
                                <input type="button" value="Organisieren" className={`pillButton ${selectedGroupAction === "Organize" ? "active" : ""}`} onClick={() => { handleChatInfoTypeSwitch("Organize"); }} />
                                <input type="button" disabled={!currentUserIsAdmin} value="Benutzer hinzufügen" className={`pillButton ${selectedGroupAction === "AddUser" ? "active" : ""}`} onClick={() => { handleChatInfoTypeSwitch("AddUser"); }} />
                            </div>

                            <div className="chatInfoModesWrapper">
                                <div className={`chatInfoModePanel organizePanel ${selectedGroupAction === "Organize" ? "active" : ""}`}>
                                    <ul className="chatInfoMemberList">
                                        {chatMembers.map(member => (
                                            <li key={member.userId} className="chatInfoMemberListItem">
                                                <p className="chatInfoMemberName">
                                                    <span className="chatInfoMemberNameText">{member.firstName} {member.lastName}</span>
                                                    {member.className && <span className="userClassBadge">{member.className}</span>}
                                                    {member.role === "Employee" && <span className="userEmployeeBadge">{"Mitarbeiter"}</span>}
                                                    {member.userId === currentUser.userId && (
                                                        <span className="chatInfoYouBadge">(Du)</span>
                                                    )}
                                                </p>

                                                <div className="memberActions">
                                                    {member.isAdmin && <span className="adminBadge">Admin</span>}
                                                    {member.userId !== currentUser.userId && <>
                                                        {member.isAdmin ? (
                                                            <button type="button" className="promoteButton" disabled={!currentUserIsAdmin} onClick={() => setActionToConfirm({ type: "DemoteUser", user: member })}>
                                                                Degradieren
                                                            </button>
                                                        ) : (
                                                            <button type="button" className="promoteButton" disabled={!currentUserIsAdmin} onClick={() => setActionToConfirm({ type: "PromoteUser", user: member })}>
                                                                Befördern
                                                            </button>
                                                        )}

                                                        <button type="button" className="removeMemberButton" disabled={!currentUserIsAdmin} onClick={() => setActionToConfirm({ type: "RemoveUser", user: member })}>
                                                            Entfernen
                                                        </button>
                                                    </>}
                                                </div>
                                            </li>
                                        ))}
                                    </ul>

                                    <div className="chatInfoFooter">
                                        <button type="button" className="leaveGroupButton" onClick={() => setActionToConfirm({ type: "LeaveGroup" })}>
                                            Gruppe verlassen
                                        </button>
                                        <button type="button" className="deleteGroupButton" disabled={!currentUserIsAdmin} onClick={() => setActionToConfirm({ type: "DeleteGroup" })}>
                                            Gruppe löschen
                                        </button>
                                    </div>
                                </div>

                                <div className={`chatInfoModePanel addUserPanel ${selectedGroupAction === "AddUser" ? "active" : ""}`}>
                                    <div className="addUserContent">
                                        <UserSearch currentUser={currentUser} chatMembers={usersToAdd} setChatMembers={setUsersToAdd} onSetChatError={(error: string | null) => setChatMemberError(error)}
                                            chatType={null} selectedChat={selectedChat} removeUsersFromMemberList={(func) => { removeUsersRef.current = func; }} memberCount={chatMembers.length} />
                                        <p className={`createChatError ${chatMemberError ? "visible" : ""}`}>{chatMemberError}</p>
                                        <button className="addUserButton" type="button" onClick={() => handleAddUsers()}>Hinzufügen</button>
                                    </div>
                                </div>
                            </div>
                        </>
                    )}

                    <button type="button" className="returnButton" onClick={onReturnButtonClicked}>
                        Zurück
                    </button>
                </div>
            </div>
        </>
    );
}

export default ChatInfoInterface;
