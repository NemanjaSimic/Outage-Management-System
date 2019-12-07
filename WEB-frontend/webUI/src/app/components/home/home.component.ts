import { Component, OnInit, ViewChild } from '@angular/core';
import * as cytoscape from 'cytoscape';
import dagre from 'cytoscape-dagre';
 
cytoscape.use(dagre);

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent implements OnInit {
  

  constructor() {

  }

  ngOnInit() {
    
  
  }

}
