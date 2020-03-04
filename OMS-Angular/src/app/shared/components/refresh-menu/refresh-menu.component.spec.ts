import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RefreshMenuComponent } from './refresh-menu.component';

describe('RefreshMenuComponent', () => {
  let component: RefreshMenuComponent;
  let fixture: ComponentFixture<RefreshMenuComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RefreshMenuComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RefreshMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
