import { TestBed } from '@angular/core/testing';

import { OutageNotificationService } from './outage-notification.service';

describe('OutageNotificationService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: OutageNotificationService = TestBed.get(OutageNotificationService);
    expect(service).toBeTruthy();
  });
});
