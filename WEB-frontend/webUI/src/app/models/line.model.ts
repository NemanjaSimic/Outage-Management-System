import { Point } from './point.model';

export class Line {
    end1: Point;
    end2: Point;

    constructor(obj?: any) {
        this.end1 = obj && obj.end1 || null;
        this.end2 = obj && obj.end2 || null;
    }
}