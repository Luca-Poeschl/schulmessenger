import { useEffect, useRef, useState } from "react";
import type { Class } from "../types/class";
import "../styles/ClassSelect.css";

export function ClassSelect({ classes, selectedClasses, onSelectedClassesChanged }:
    { classes: Class[], selectedClasses: Class[], onSelectedClassesChanged: React.Dispatch<React.SetStateAction<Class[]>> }) {
    const [showSelection, setShowSelection] = useState<boolean>(false);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const wrapperRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!showSelection) return;

        function handleClickOutside(event: MouseEvent) {
            if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
                setShowSelection(false);
            }
        }

        document.addEventListener("pointerdown", handleClickOutside);

        return () => document.removeEventListener("pointerdown", handleClickOutside);
    }, [showSelection]);

    const visibleClasses = classes.filter(c => c.className.toLowerCase().includes(searchTerm.trim().toLowerCase()));

    function toggleClass(classObject: Class) {
        if (selectedClasses.some(c => c.classId === classObject.classId)) {
            onSelectedClassesChanged(selectedClasses.filter(c => c.classId !== classObject.classId));
        }
        else {
            onSelectedClassesChanged([...selectedClasses, classObject]);
        }
    }

    return (
        <div ref={wrapperRef} className="classSelectWrapper">
            <button type="button" className="classSelectShowButton" onClick={() => setShowSelection(!showSelection)}>
                {selectedClasses.length === 0
                    ? "Alle Klassen" : `${selectedClasses.length} ausgewählt`}
            </button>

            {showSelection &&
                <div className="classSelectMenu">
                    <input type="text" className="classSelectFilterInput" placeholder="Klasse suchen..." value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)} />

                    <ul className="classSelectClassList">
                        {
                            visibleClasses.map(c => {
                                return <li key={c.classId} className="classSelectClassListItem" onClick={() => toggleClass(c)}>
                                    {c.className}
                                    <input type="checkbox" readOnly checked={selectedClasses.some(cl => cl.classId === c.classId)} />
                                </li>
                            })
                        }
                    </ul>
                </div>}
        </div>
    );
}

export default ClassSelect;
