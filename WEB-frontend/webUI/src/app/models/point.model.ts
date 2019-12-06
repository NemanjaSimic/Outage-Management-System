export class Point {
    coordX: number;
    coordY: number;
    parents: Point [];
    children: Point[];
    
    constructor(obj?: any) {
        this.coordX = obj && obj.coordX || null;
        this.coordY = obj && obj.coordY || null;
        this.children = obj && obj.children || null;
    }
}