import { Component, OnInit, ViewChild } from '@angular/core';
import { Point } from 'src/app/models/point.model';
import { Line } from 'src/app/models/line.model';
declare var cytoscape: any;

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent implements OnInit {
  Points: Point[] = [];
  Lines: Line[];
  @ViewChild('graph', {static: true}) graphRef
  
  constructor() {
  }
  
  ngOnInit() {
    this.initPoints();
    this.drawRectangle();
  }
  
  initPoints() : void {
    let point : Point = new Point();
    point.coordX = 20;
    point.coordY = 20;
    this.Points.push(point);
    let point2 : Point = new Point();
    point2.coordX = 60;
    point2.coordY = 20;
    this.Points.push(point2);
    let point3 : Point = new Point();
    point3.coordX = 30;
    point3.coordY = 100;
    this.Points.push(point3);
  }

  drawRectangle() : void
  {
    let canvas = this.graphRef.nativeElement;
    // canvas.ownerDocument.body.style.backgroundColor= '#383838';
    canvas.ownerDocument.body.style.backgroundColor= '#f0f0f0';


    let context = canvas.getContext('2d');

    this.Points.forEach(point => {
      context.beginPath();
      context.rect(point.coordX, point.coordY,30,30);
      context.stroke();
      context.fillStyle = "red";
      context.fill();
    })
  }
}
