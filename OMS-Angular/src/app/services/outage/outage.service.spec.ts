import { TestBed } from '@angular/core/testing';

import { OutageService } from './outage.service';

describe('OutageService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: OutageService = TestBed.get(OutageService);
    expect(service).toBeTruthy();
  });
});
