export type UserRole = "Student" | "Employee"

export interface User{
    userId: string,
    firstName: string,
    lastName: string,
    role: UserRole,
    className: string | null
}