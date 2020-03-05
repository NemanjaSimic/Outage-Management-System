import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ArchivedOutageModalComponent } from './archived-outage-modal.component';

describe('ArchivedOutageModalComponent', () => {
  let component: ArchivedOutageModalComponent;
  let fixture: ComponentFixture<ArchivedOutageModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ArchivedOutageModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ArchivedOutageModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
