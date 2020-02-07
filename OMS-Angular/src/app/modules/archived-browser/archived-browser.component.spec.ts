import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ArchivedBrowserComponent } from './archived-browser.component';

describe('ArchivedBrowserComponent', () => {
  let component: ArchivedBrowserComponent;
  let fixture: ComponentFixture<ArchivedBrowserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ArchivedBrowserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ArchivedBrowserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
