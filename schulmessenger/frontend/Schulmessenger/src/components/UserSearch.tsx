import { useEffect, useState } from "react";
import { fetchWrapper } from "../api/fetchWrapper";
import type { Chat, ChatType } from "../types/chat";
import type { UserInChatMemberList } from "../types/userInChatMembersList";
import type { UserOnChatCreate } from "../types/userOnChatCreate";
import type { User } from "../types/user";
import "../styles/UserSearch.css";
import { getSignalRConnection } from "../services/signalrService";
import type { UserInChatInfoMemberList } from "../types/userInChatInfoMemberList";
import type { Class } from "../types/class";
import ClassSelect from "./ClassSelect";

export function UserSearch({ currentUser, chatMembers, setChatMembers, onSetChatError, chatType, selectedChat, removeUsersFromMemberList, memberCount }
    : {
        currentUser: User,
        chatMembers: UserOnChatCreate[]; setChatMembers: React.Dispatch<React.SetStateAction<UserOnChatCreate[]>>;
        onSetChatError: (error: string | null) => void; chatType: ChatType | null;
        selectedChat: Chat | null; removeUsersFromMemberList?: (removeFunction: (userIdsToRemove: string[]) => void) => void;
        memberCount?: number | null;
    }) {

    const [chatMemberList, setChatMemberList] = useState<UserInChatMemberList[]>([]);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const [classes, setClasses] = useState<Class[]>([]);
    const [selectedClasses, setSelectedClasses] = useState<Class[]>([]);

    const isGroup = chatType === "Groupchat" || selectedChat?.chatType === "Groupchat";

    const filteredMembers = chatMemberList.filter((member) => {
        const fullName = `${member.firstName} ${member.lastName}`.toLowerCase();
        const matchesSearch = fullName.includes(searchTerm.trim().toLowerCase());

        const matchesClass = selectedClasses.length === 0 ||
            selectedClasses.some(c => c.className === member.className);

        if (currentUser.role === "Student" && isGroup && member.role === "Employee")
            return false;

        return matchesSearch && matchesClass;
    });

    useEffect(() => {
        fetchWrapper("/api/Class")
            .then((data) => setClasses(data));
    }, []);

    useEffect(() => {
        fetchWrapper("/api/User/filtered" + (selectedChat?.chatId ? `?excludedChatId=${selectedChat?.chatId}` : ""))
            .then((data) => setChatMemberList(data));
    }, [currentUser.userId, selectedChat?.chatId, memberCount]);

    useEffect(() => {
        const connection = getSignalRConnection();

        const handleReceiveUserAdded = (chatId: string, userToAdd: UserInChatInfoMemberList) => {
            if (chatId === selectedChat?.chatId) {
                setChatMemberList(prev => prev.filter(m => m.userId !== userToAdd.userId));
                setChatMembers(prev => prev.filter(m => m.userId !== userToAdd.userId));
            }
        };

        connection.on("ReceiveUserAdded", handleReceiveUserAdded);

        return () => {
            connection.off("ReceiveUserAdded", handleReceiveUserAdded);
        };
    }, [selectedChat?.chatId, setChatMembers]);

    useEffect(() => {
        removeUsersFromMemberList?.((userIdsToRemove: string[]) => {
            setChatMemberList(prev => prev.filter(m => !userIdsToRemove.includes(m.userId)));
        });
    }, [removeUsersFromMemberList]);

    useEffect(() => {
        if (currentUser.role === "Student" && isGroup) {
            const employeeIds = chatMemberList.map((m) => {
                if (m.role === "Employee")
                    return m.userId;
            });
            setChatMembers(prev => prev.filter((m) => !employeeIds.includes(m.userId)));
        }
    }, [currentUser.role, isGroup, setChatMembers, chatMemberList])

    function handleToggleMember(member: UserInChatMemberList) {
        onSetChatError(null);

        const isSelected: UserOnChatCreate | undefined = chatMembers.find((m) => m.userId === member.userId);

        if (!isSelected) {
            if (chatType === "Direct" && chatMembers.length >= 1) {
                onSetChatError("Es darf nur genau ein anderes Mitglied angegeben werden!");
                return;
            }
            else if (chatType === "Groupchat" && chatMembers.length >= 49) {
                { /* Selbst miteingerechnet */ }
                onSetChatError("Es dürfen maximal 50 Mitglieder angegeben werden!");
                return;
            }
        }

        setChatMembers((prev) =>
            isSelected
                ? prev.filter((m) => m.userId !== member.userId)
                : [...prev, { userId: member.userId }]
        );
    }

    return (
        <div className="chatMembersWrapper">
            <p>*Benutzer auswählen:</p>
            <div className="chatMembersFilterRow">
                <input type="search" placeholder="Benutzername..." onChange={(e) => setSearchTerm(e.target.value)} />
                <ClassSelect classes={classes} selectedClasses={selectedClasses} onSelectedClassesChanged={setSelectedClasses} />
            </div>
            <ul className="chatMembersList">
                {
                    filteredMembers.map((member) => {
                        return (
                            <li key={member.userId} className="chatMembersListItem" onClick={() => handleToggleMember(member)}>
                                {`${member.firstName} ${member.lastName}`}
                                {member.className && <span className="userClassBadge">{member.className} </span>}
                                {member.role === "Employee" && <span className="userEmployeeBadge">{"Mitarbeiter"}</span>}

                                <input type="checkbox" readOnly // damit nicht doppelt geklickt wird
                                    checked={chatMembers.some((m) => m.userId === member.userId)}
                                />
                            </li>
                        );
                    })
                }
            </ul>
        </div>
    );
}

export default UserSearch;