export interface IMailing {
    id: number ;
    email: string;
    userID: number;
    userName: string;
    frequency: string;
    report: string;
    userNames: string[];
    userIDList: number[];
    userList: any[];
    timeSend: Date;
}
