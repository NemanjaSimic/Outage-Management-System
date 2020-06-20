import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ActiveOutageModalComponent } from './active-outage-modal.component';

describe('ActiveOutageModalComponent', () => {
  let component: ActiveOutageModalComponent;
  let fixture: ComponentFixture<ActiveOutageModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ActiveOutageModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ActiveOutageModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
