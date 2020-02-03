import { TestBed } from '@angular/core/testing';

import { ScadaService } from './scada.service';

describe('GraphService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ScadaService = TestBed.get(ScadaService);
    expect(service).toBeTruthy();
  });
});
