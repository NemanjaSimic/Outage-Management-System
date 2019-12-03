export class Point {
    coordX: number;
    coordY: number;

    constructor(obj?: any) {
        this.coordX = obj && obj.coordX || null;
        this.coordY = obj && obj.coordY || null;
    }
}