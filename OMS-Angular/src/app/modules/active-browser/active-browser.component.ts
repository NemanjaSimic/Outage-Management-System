import { Component, OnInit } from '@angular/core';

import * as outages from './outage-mock.json'

@Component({
  selector: 'app-active-browser',
  templateUrl: './active-browser.component.html',
  styleUrls: ['./active-browser.component.css']
})

export class ActiveBrowserComponent implements OnInit {
  displayedColumns: string[] = ['id', 'dateCreated'];
  dataSource = outages;

  constructor() { }

  ngOnInit() {
  }

}

