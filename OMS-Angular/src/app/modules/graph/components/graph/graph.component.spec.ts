import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { DebugElement } from '@angular/core';

import { GraphComponent } from './graph.component';
import { GraphService } from '@services/notification/graph.service';
import { of, Subscription } from 'rxjs';

describe('GraphComponent', () => {
  let component: GraphComponent;
  let fixture: ComponentFixture<GraphComponent>;
  let debugElement: DebugElement;
  let graphService: GraphService;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GraphComponent ],
      providers: [ GraphService ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphComponent);
    component = fixture.componentInstance;
    debugElement = fixture.debugElement;
    fixture.detectChanges();

    graphService = debugElement.injector.get(GraphService);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('graph service - startConnection gets called and passes', () => {
    spyOn(graphService, 'startConnection').and.returnValue(of(true));
    
    component.startConnection();
    
    expect(graphService.startConnection).toHaveBeenCalled();
    expect(component.connectionSubscription).not.toEqual(Subscription.EMPTY);
    expect(component.connectionSubscription).toBeDefined();
    expect(component.updateSubscription).not.toEqual(Subscription.EMPTY);
    expect(component.updateSubscription).toBeDefined();
  });

  it('graph service - startConnection fails', () => {
    spyOn(graphService, 'startConnection').and.returnValue(of(false));

    component.startConnection();
    
    expect(graphService.startConnection).toHaveBeenCalled();
    expect(component.connectionSubscription).not.toEqual(Subscription.EMPTY);
    expect(component.connectionSubscription).toBeDefined();
    expect(component.updateSubscription).toEqual(Subscription.EMPTY);

  });
});
