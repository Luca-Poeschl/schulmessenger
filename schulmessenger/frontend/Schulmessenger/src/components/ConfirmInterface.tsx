import type { ConfirmAction } from "../types/confirmAction";
import '../styles/ConfirmInterface.css';

export function ConfirmInterface({ action, onConfirm, onCancel }:
    { action: ConfirmAction, onConfirm: () => void, onCancel: () => void; }) {

    let confirmText = "";
    switch (action.type) {
        case "PromoteUser":
            confirmText = (`${action.user.firstName} ${action.user.lastName} wirklich befördern`);
            break;
        case "DemoteUser":
            confirmText = (`${action.user.firstName} ${action.user.lastName} wirklich degradieren`);
            break;
        case "RemoveUser":
            confirmText = (`${action.user.firstName} ${action.user.lastName} wirklich aus der Gruppe entfernen`);
            break;
        case "LeaveGroup":
            confirmText = ("die Gruppe wirklich verlassen");
            break;
        case "DeleteGroup":
            confirmText = ("die Gruppe wirklich löschen");
            break;
    }

    return (
        <div className="confirmBackground" onClick={() => onCancel()}>
            <div className="confirmCard" onClick={(e) => e.stopPropagation()}>
                <p>Möchtest du {confirmText}?</p>

                <div className="confirmButtonGroup">
                    <button className="confirmConfirmButton" type="button" onClick={() => onConfirm()}>Bestätigen</button>
                    <button className="confirmCancelButton" type="button" onClick={() => onCancel()}>Abbrechen</button>
                </div>
            </div>
        </div>
    );
}

export default ConfirmInterface;
